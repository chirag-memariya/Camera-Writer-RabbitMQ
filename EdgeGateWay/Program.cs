using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();
builder.Services.AddHostedService<EdgeGatewayAgent>();
builder.Services.AddLogging();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseAuthorization();
app.MapControllers();


app.MapGet("/", () => "EdgeGatewayAgent is running");

app.Run();

// Models
public class CameraEvent
{
    public string CameraId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CameraProfile
{
    public string CameraId { get; set; } = string.Empty;
    public List<string> InterestedParties { get; set; } = new();
}

// Interfaces
public interface IRabbitMQPublisher
{
    Task PublishEventAsync(CameraEvent cameraEvent, string routingKey);
}

// RabbitMQ Publisher
public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMQPublisher> _logger;
    private const string ExchangeName = "camera_events";

    public RabbitMQPublisher(ILogger<RabbitMQPublisher> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Declare exchange
        _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);
        
        _logger.LogInformation("RabbitMQ Publisher initialized");
    }

    public async Task PublishEventAsync(CameraEvent cameraEvent, string routingKey)
    {
        var message = JsonSerializer.Serialize(cameraEvent);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: ExchangeName,
            routingKey: routingKey, // one of the items from InterestedParties // must match exactly
            basicProperties: null,
            body: body);

        _logger.LogInformation($"Published event to routing key '{routingKey}': {message}");
        
        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}

// Edge Gateway Agent Service
public class EdgeGatewayAgent : BackgroundService
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<EdgeGatewayAgent> _logger;
    private readonly List<CameraProfile> _cameraProfiles;

    public EdgeGatewayAgent(IRabbitMQPublisher publisher, ILogger<EdgeGatewayAgent> logger)
    {
        _publisher = publisher;
        _logger = logger;
        
        // Embedded camera profiles - in real scenario this would come from DeviceProfileService
        _cameraProfiles = new List<CameraProfile>
        {
            new CameraProfile
            {
                CameraId = "CAMERA-001",
                InterestedParties = new List<string> { "NVR-1", "Storage-1" }
            },
            new CameraProfile
            {
                CameraId = "CAMERA-002", 
                InterestedParties = new List<string> { "NVR-2", "Storage-2" }
            }
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EdgeGatewayAgent started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            // Simulate events from different cameras
            foreach (var cameraProfile in _cameraProfiles)
            {
                await SimulateCameraEvent(cameraProfile);
                
                // Small delay between cameras
                await Task.Delay(2000, stoppingToken);
            }
            
            // Wait 8 seconds before next round of events
            await Task.Delay(8000, stoppingToken);
        }
    }

    private async Task SimulateCameraEvent(CameraProfile cameraProfile)
    {
        var eventTypes = new[] { "MotionDetected", "AudioAlert", "ObjectDetection", "TamperAlert" };
        var random = new Random();
        
        var cameraEvent = new CameraEvent
        {
            CameraId = cameraProfile.CameraId,
            EventType = eventTypes[random.Next(eventTypes.Length)],
            Timestamp = DateTime.UtcNow,
            Description = $"Event detected by camera {cameraProfile.CameraId}"
        };

        // Publish to all interested parties for this specific camera
        // foreach (var interestedParty in cameraProfile.InterestedParties)
        // {
        //     await _publisher.PublishEventAsync(cameraEvent, interestedParty);
        // }

        // _logger.LogInformation($"Camera '{cameraProfile.CameraId}' event '{cameraEvent.EventType}' published to interested parties: [{string.Join(", ", cameraProfile.InterestedParties)}]");
    }
}