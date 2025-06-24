using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MiniPipeline.Core;

namespace MiniPipeline.WebSocket
{
    public class PipelineClientHandler : IClientHandler
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpRequestParser _parser;
        private readonly IWebSocketRequestValidator _validator;
        private readonly ISocketFeaturesFactory _factory;
        private readonly ILogger<PipelineClientHandler> _logger;
        private readonly PipelineSocketOptions? _sslOptions;
        public PipelineClientHandler(
            IHttpRequestParser parser,
            IWebSocketRequestValidator validator,
            ISocketFeaturesFactory factory,
            ILogger<PipelineClientHandler> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<PipelineSocketOptions> sslOptions)
        {
            _parser = parser;
            _validator = validator;
            _factory = factory;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _sslOptions = sslOptions.Value;
        }

        public async Task HandleClientAsync(ITcpClient client, CancellationToken cancellationToken)
        {

            using var scope = _scopeFactory.CreateScope();
            var pipeline = scope.ServiceProvider.GetRequiredService<IHttpContextPipeline>();
            var stream = client.GetStream();



            if (_sslOptions != null && _sslOptions.UseSsl)
            {
                try
                {

                    X509Certificate2 cert = null;


                    if (!string.IsNullOrEmpty(_sslOptions.PfxPath))
                    {
                        cert = new X509Certificate2(fileName: _sslOptions.PfxPath, password: _sslOptions.Password);
                    }
                    else if (!string.IsNullOrEmpty(_sslOptions.PemCertPath) && !string.IsNullOrEmpty(_sslOptions.PemKeyPath))
                    {
                        var certPem = File.ReadAllText(_sslOptions.PemCertPath);
                        var keyPem = File.ReadAllText(_sslOptions.PemKeyPath);

                        using var certBuilder = RSA.Create();
                        certBuilder.ImportFromPem(keyPem.ToCharArray());

                        var certReq = new CertificateRequest($"CN={_sslOptions.PemCertCommonName}", certBuilder, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                        cert = certReq.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));
                    }
                    else
                    {
                        throw new InvalidOperationException("No valid certificate found in SSL options.");
                    }

                    var sslStream = new SslStream(stream, false);
                    await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                    {
                        ServerCertificate = cert,
                        EnabledSslProtocols = _sslOptions.Protocols ?? SslProtocols.Tls12 | SslProtocols.Tls13,
                        ClientCertificateRequired = false,
                        CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                    }, CancellationToken.None);

                    stream = sslStream;
                }
                catch (Exception ex)
                {
                    client.Close();
                    _logger.LogWarning("SSL handshake failed, disconnected client. Reason: " + ex.Message);
                    return;
                }
            }

            try
            {
                var buffer = new byte[4096];
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                var requestText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var rawRequest = _parser.Parse(requestText);

                //only serve http upgrade handshake via websocket object browser since CEF doesnt capture ws/wss scheme,
                if (!_validator.IsWebSocketUpgradeRequest(rawRequest, out var secWebSocketKey))
                {
                    client.Close();
                    return;
                }

                var features = _factory.CreateFeatures(rawRequest, stream);
                var request = features.Get<IHttpRequestFeature>();
                var uri = new Uri($"{request.Scheme}://{request.Headers["HOST"]}{request.Path}");

                var context = pipeline.BuildHttpContext(features);
                var upgradeFeature = new SocketUpgradeFeature(stream, context);
                context.Features.Set<IHttpUpgradeFeature>(upgradeFeature);


                var endpointDataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();

                if (context.GetEndpoint() == null)
                {
                    var endpoints = endpointDataSource.Endpoints
                    .OfType<RouteEndpoint>()
                    .FirstOrDefault(endpoint => endpoint.RoutePattern.RawText == uri.AbsolutePath);
                    if(endpoints!=null)
                      context.SetEndpoint(endpoints);
                }
                
                await pipeline.SocketInvokeAsync(context);
                 

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling client");
            }
            finally
            {
                client.Close();

            }
        }

    }




}
