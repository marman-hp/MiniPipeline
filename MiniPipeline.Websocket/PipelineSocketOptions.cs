using System.Net;
using System.Security.Authentication;

namespace MiniPipeline.WebSocket
{
    public class PipelineSocketOptions
    {
        public IPAddress IPAddress { get; set; } = IPAddress.Loopback;
        public int Port { get; set; } = 8080;
        public bool UseSsl { get; set; } = false;

        // PFX option
        public string? PfxPath { get; set; }
        public string? Password { get; set; }

        // PEM option
        public string? PemCertCommonName { get; set; } = "localhost";
        public string? PemCertPath { get; set; }

        public string? PemKeyPath { get; set; }


        public SslProtocols? Protocols { get; set; } = SslProtocols.Tls12 | SslProtocols.Tls13;
        public bool CheckRevocation { get; set; } = false;
    }




}
