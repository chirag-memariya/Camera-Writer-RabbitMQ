using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CameraEventController : ControllerBase
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<CameraEventController> _logger;

    public CameraEventController(IRabbitMQPublisher publisher, ILogger<CameraEventController> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    [HttpPost("{cameraId}")]
    public async Task<IActionResult> TriggerCameraEvent(string cameraId)
    {
        if (cameraId != "CAMERA-001" &&  cameraId!= "CAMERA-002")
            return NotFound($"Only CAMERA-001 and CAMERA-002 is supported in this setup.");

        var cameraEvent = new CameraEvent
        {
            CameraId    = cameraId,
            EventType   = "ManualTrigger",
            Timestamp   = DateTime.UtcNow,
            Description = $"Manually triggered event for {cameraId}"
        };

        // Send with header: interestedParty = CAMERA-001
        var headers = new Dictionary<string, object>
        {
            ["cameraId"]        = cameraId,
            ["interestedParty"] = cameraId
        };

        await _publisher.PublishEventAsync(cameraEvent, headers);

        _logger.LogInformation($"Event for {cameraId} published to interestedParty = {cameraId}");

        return Ok(new { Message = $"Event sent for {cameraId}" });
    }
}
