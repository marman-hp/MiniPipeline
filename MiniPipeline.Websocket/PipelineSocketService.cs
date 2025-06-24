using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MiniPipeline.WebSocket
{

    public class PipelineSocketService : BackgroundService, IBackgroundStatus
    {
        private readonly ITcpListener _listener;
        private readonly IClientHandler _clientHandler;
        private readonly ILogger<PipelineSocketService> _logger;

        public PipelineSocketService(ITcpListener listener, IClientHandler clientHandler, ILogger<PipelineSocketService> logger)
        {
            _listener = listener;
            _clientHandler = clientHandler;
            _logger = logger;
        }
        private volatile bool _isRunning;

        public bool IsRunning => _isRunning;

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _listener.Start();
            _isRunning = true;
            while (!cancellationToken.IsCancellationRequested)
            {
                ITcpClient client = null!;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(cancellationToken);
                    _ = _clientHandler.HandleClientAsync(client, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting client");
                    client?.Dispose();
                }
            }
            _isRunning = false;

        }


    }




}
