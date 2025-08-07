| Feature              | Direct Exchange                                 | Topic Exchange                                    | Headers Exchange                                                                             |
| -------------------- | ----------------------------------------------- | ------------------------------------------------- | -------------------------------------------------------------------------------------------- |
| **Routing Logic**    | Uses **exact** routing key match                | Uses routing key with wildcards (`*`, `#`)        | Uses message headers for routing                                                             |
| **Binding Matching** | Exact match between routing key and binding key | Match routing key with binding key using patterns | Match headers using exact key-value pairs or `"x-match"` logic                               |
| **Performance**      | Fast (simple string comparison)                 | Faster (string comparison with wildcard parsing)  | Slightly slower (more complex matching logic)                                                |
| **Flexibility**      | Low – only exact keys                           | Good – structured topics (e.g., logs, camera.\*)  | High – complex filtering (e.g., multiple attributes)                                         |
| **Example Use**      | `routingKey = "CAMERA-001"`                     | `camera.CAMERA-001`, `camera.*`                   | `{ cameraId: CAMERA-001, location: "NVR-1" }`                                                |
| **Used in Code**     | `BasicPublish(exchange, "CAMERA-001", ...)`     | `PublishEventAsync(..., "camera.nvr.CAMERA-001")` | `PublishEventAsync(..., headers: new Dictionary<string, object>{{"cameraId","CAMERA-001"}})` |

---

## Direct Exchange:

### 🔸Publisher (`CameraEventController`):

```csharp
await _publisher.PublishEventAsync(cameraEvent, "CAMERA-001");
```

### 🔸Consumer:

```csharp
_channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: "CAMERA-001");
```

    Messages are routed based on the **exact** routing key match.

## Topic Exchange:

### 🔸Publisher (`CameraEventController`):

```csharp
await _publisher.PublishEventAsync(cameraEvent, cameraId); // e.g., "camera.nvr.CAMERA-001"
```

### 🔸Consumer:

```csharp
_channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: "camera.nvr.*");
```

    Messages are routed based on the routing key matching the wildcard pattern.

## Headers Exchange Instead:

### 🔸Publisher:

```csharp
var props = _channel.CreateBasicProperties();
props.Headers = new Dictionary<string, object>
{
    { "cameraId", "CAMERA-001" },
    { "location", "NVR-1" }
};
_channel.BasicPublish(
    exchange: "camera-headers-exchange",
    routingKey: string.Empty, // ignored
    basicProperties: props,
    body: messageBody
);
```

### 🔸Consumer:

```csharp
var args = new Dictionary<string, object>
{
    { "x-match", "all" }, // or "any"
    { "cameraId", "CAMERA-001" },
    { "location", "NVR-1" }
};

_channel.QueueBind(
    queue: _queueName,
    exchange: "camera-headers-exchange",
    routingKey: string.Empty,
    arguments: args
);
```
| `"x-match"` Value | Meaning                                                            |
| ----------------- | ------------------------------------------------------------------ |
| `"all"`           | All specified headers must match **exactly** (both key and value). |
| `"any"`           | At least **one header** must match.                                |

    Messages are routed if headers match exactly according to `x-match`.

## When to Use Which?

| Use Case Example                                         | Direct Exchange | Topic Exchange | Headers Exchange |
| -------------------------------------------------------- | --------------- | -------------- | ---------------- |
| Route by `cameraId` only                                 | ✅               |                |                  |
| Route by `cameraId` with wildcard (e.g., `camera.*`)     |                 | ✅              |                  |
| Route based on multiple fields (`cameraId` + `location`) |                 |                | ✅                |
| Need exact match on header values                        |                 |                | ✅                |
