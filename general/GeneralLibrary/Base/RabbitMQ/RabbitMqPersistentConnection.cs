using System.Net.Sockets;
using GeneralLibrary.Base.EventBus.Logging;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace GeneralLibrary.Base.RabbitMQ;

public class RabbitMqPersistentConnection : IRabbitMqPersistentConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly IEventBusLogger _logger;
    private readonly int _retryCount;

    [CanBeNull]
    private IConnection _connection;

    private bool _disposed;

    private readonly object _syncRoot = new();

    public RabbitMqPersistentConnection(IOptions<RabbitMqConnectionSettings> conSettings, IEventBusLogger logger)
    {
        _logger = logger;
        _connectionFactory = new ConnectionFactory
        {
            HostName = conSettings.Value.HostName,
            Port = conSettings.Value.Port,
            UserName = conSettings.Value.UserName,
            Password = conSettings.Value.Password,
            VirtualHost = conSettings.Value.VirtualHost,
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            DispatchConsumersAsync = true,
        };
        _retryCount = conSettings.Value.ConnectionRetryCount;
    }

    public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

    public bool TryConnect()
    {
        _logger.LogInformation("RabbitMQ Client is trying to connect");

        lock (_syncRoot)
        {
            var policy = Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning("RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                    }
                );

            policy.Execute(() =>
            {
                _connection = _connectionFactory.CreateConnection();
            });

            if (IsConnected)
            {
                _connection!.ConnectionShutdown += OnConnectionShutdown;
                _connection!.CallbackException += OnCallbackException;
                _connection!.ConnectionBlocked += OnConnectionBlocked;
                _connection!.ConnectionUnblocked += OnConnectionUnblocked;

                _logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{HostName}'", _connection?.Endpoint?.HostName);

                return true;
            }

            _logger.LogError("FATAL ERROR: RabbitMQ connections could not be created and opened");

            return false;
        }
    }

    public IModel CreateModel()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
        }

        return _connection?.CreateModel();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;

        try
        {
            if (IsConnected)
            {
                if (_connection != null)
                {
                    _connection.ConnectionShutdown -= OnConnectionShutdown;
                    _connection.CallbackException -= OnCallbackException;
                    _connection.ConnectionBlocked -= OnConnectionBlocked;
                    _connection.Close();
                }
            }

            _connection?.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogError(ex.Message);
        }
    }

    private void OnCallbackException(object sender, CallbackExceptionEventArgs args)
    {
        _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
        TryConnectIfNotDisposed();
    }

    private void OnConnectionShutdown(object sender, ShutdownEventArgs args)
    {
        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
        TryConnectIfNotDisposed();
    }

    private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs args)
    {
        _logger.LogWarning("A RabbitMQ connection is blocked. Trying to re-connect...");
        TryConnectIfNotDisposed();
    }

    private void OnConnectionUnblocked(object sender, EventArgs args)
    {
        _logger.LogWarning("A RabbitMQ connection is unblocked. Trying to re-connect...");
        TryConnectIfNotDisposed();
    }

    private void TryConnectIfNotDisposed()
    {
        if (_disposed)
        {
            _logger.LogInformation("RabbitMQ client is disposed. No action will be taken.");
            return;
        }

        TryConnect();
    }
}