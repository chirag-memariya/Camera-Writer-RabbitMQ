using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CameraEventController : ControllerBase
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<CameraEventController> _logger;

    // Hardcoded profiles for simplicity
    private static readonly Dictionary<string, List<string>> CameraProfiles = new()
    {
        { "CAMERA-001", new List<string> { "NVR-1", "Storage-1" } },
        { "CAMERA-002", new List<string> { "NVR-2", "Storage-2" } }
    };

    public CameraEventController(IRabbitMQPublisher publisher, ILogger<CameraEventController> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    [HttpPost("{cameraId}")]
    public async Task<IActionResult> TriggerCameraEvent(string cameraId)
    {
        if (!CameraProfiles.TryGetValue(cameraId, out var interestedParties))
            return NotFound($"CameraId '{cameraId}' not found.");

        var cameraEvent = new CameraEvent
        {
            CameraId = cameraId,
            EventType = "ManualTrigger",
            Timestamp = DateTime.UtcNow,
            Description = $"Manually triggered event for camera {cameraId}"
        };

        foreach (var party in interestedParties)
        {
            await _publisher.PublishEventAsync(cameraEvent, party);
        }

        _logger.LogInformation($"Manually triggered event for {cameraId} sent to: {string.Join(", ", interestedParties)}");

        return Ok(new { Message = $"Event sent for {cameraId}" });
    }
}
