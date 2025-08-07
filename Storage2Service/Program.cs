using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<IRabbitMQConsumer, RabbitMQConsumer>();
builder.Services.AddHostedService<Storage2Service>();
builder.Services.AddLogging();

var app = builder.Build();

app.MapGet("/", () => "Storage2Service is running");

app.Run();

// Models
public class CameraEvent
{
    public string CameraId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
}

// Interfaces
public interface IRabbitMQConsumer
{
    Task StartConsumingAsync(Func<CameraEvent, Task> eventHandler, CancellationToken cancellationToken);
}

// RabbitMQ Consumer
public class RabbitMQConsumer : IRabbitMQConsumer, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQConsumer> _logger;
    private const string ExchangeName = "camera_events";
    private const string QueueName = "storage2_queue";
    private const string RoutingKey = "CAMERA-002";

    public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger)
    {
        _logger = logger;
        
var factory = new ConnectionFactory()
{
    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbit",
    Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
    UserName = "guest",
    Password = "guest",
    DispatchConsumersAsync = true
};

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Headers, durable: true);
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);
            // bind queue with header‐matching criteria:
            // here we want messages where interestedParty = "CAMERA-002"
            var bindArgs = new Dictionary<string, object>
            {
                ["x-match"]         = "all",       // or "any"
                ["interestedParty"] = "CAMERA-002"
            };

            // Bind queue to exchange with routing key
            _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: string.Empty, arguments: bindArgs);
            
            var queueInfo = _channel.QueueDeclarePassive(QueueName);
            _logger.LogInformation($"RabbitMQ Consumer initialized. Queue '{QueueName}' bound to exchange '{ExchangeName}' with routing key '{RoutingKey}'. Messages in queue: {queueInfo.MessageCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Consumer");
            throw;
        }
    }

    public async Task StartConsumingAsync(Func<CameraEvent, Task> eventHandler, CancellationToken cancellationToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation($"Received message: {message}");
                
                var cameraEvent = JsonSerializer.Deserialize<CameraEvent>(message);
                if (cameraEvent != null)
                {
                    await eventHandler(cameraEvent);
                }
                
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                _channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        var consumerTag = _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
        
        _logger.LogInformation($"Started consuming messages with consumer tag: {consumerTag}");
        
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(5000, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer stopped due to cancellation");
        }
        finally
        {
            try
            {
                _channel.BasicCancel(consumerTag);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cancelling consumer");
            }
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}

// Storage2 Service
public class Storage2Service : BackgroundService
{
    private readonly IRabbitMQConsumer _consumer;
    private readonly ILogger<Storage2Service> _logger;

    public Storage2Service(IRabbitMQConsumer consumer, ILogger<Storage2Service> logger)
    {
        _consumer = consumer;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Storage2Service started and waiting for camera events");
        
        await _consumer.StartConsumingAsync(ProcessCameraEvent, stoppingToken);
    }

    private async Task ProcessCameraEvent(CameraEvent cameraEvent)
    {
        _logger.LogInformation($"[STORAGE2] Processing camera event: Camera={cameraEvent.CameraId}, Type={cameraEvent.EventType}, Time={cameraEvent.Timestamp}");
        
        await Task.Delay(800);
        
        switch (cameraEvent.EventType)
        {
            case "MotionDetected":
                _logger.LogInformation($"[STORAGE2] Storing motion detection footage from {cameraEvent.CameraId}");
                break;
            case "AudioAlert":
                _logger.LogInformation($"[STORAGE2] Storing audio alert recording from {cameraEvent.CameraId}");
                break;
            case "ObjectDetection":
                _logger.LogInformation($"[STORAGE2] Storing object detection data from {cameraEvent.CameraId}");
                break;
            case "TamperAlert":
                _logger.LogInformation($"[STORAGE2] CRITICAL - Storing tamper alert evidence from {cameraEvent.CameraId}");
                break;
            default:
                _logger.LogWarning($"[STORAGE2] Unknown event type: {cameraEvent.EventType}");
                break;
        }
        
        await Task.CompletedTask;
    }
}