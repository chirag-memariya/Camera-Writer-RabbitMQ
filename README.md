#  Camera Event Routing System

This project demonstrates a RabbitMQ-based event routing system for multiple camera sources and interested services.

##  Project Structure

- `EdgeGatewayAgent` – Simulates camera event generation and publishes events via RabbitMQ.
- `NVR1Service` – Subscribes to `"NVR-1"` routing key.
- `NVR2Service` – Subscribes to `"NVR-2"` routing key.
- `Storage1Service` – Subscribes to `"Storage-1"` routing key.
- `Storage2Service` – Subscribes to `"Storage-2"` routing key.
- `camera-event-visualizer` – Vite + React frontend for visualizing event flow (optional).

---

##  Camera Configuration

| Camera      | Interested Parties         |
|-------------|----------------------------|
| CAMERA-001  | `["NVR-1", "Storage-1"]`   |
| CAMERA-002  | `["NVR-2", "Storage-2"]`   |

---

##  Routing Key Mapping

| Service         | Routing Key  | Expected Events From |
|-----------------|--------------|----------------------|
| NVR1Service     | `NVR-1`      | CAMERA-001           |
| Storage1Service | `Storage-1`  | CAMERA-001           |
| NVR2Service     | `NVR-2`      | CAMERA-002           |
| Storage2Service | `Storage-2`  | CAMERA-002           |

---

##  Getting Started

### 1. Start RabbitMQ

```bash
docker run --hostname rabbit --name rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```
###  RabbitMQ Management UI

**Access it here:** [http://localhost:15672](http://localhost:15672)  
**Username:** `guest`  
**Password:** `guest`

### 2. Start All Services
In separate terminals :
```bash
# Terminal 1 - EdgeGatewayAgent
dotnet run --project EdgeGateWay

# Terminal 2 - NVR1Service
dotnet run --project NVR1Service

# Terminal 3 - Storage1Service
dotnet run --project Storage1Service

# Terminal 4 - NVR2Service
dotnet run --project NVR2Service

# Terminal 5 - Storage2Service
dotnet run --project Storage2Service

# Terminal 6 - Web UI (optional)
cd camera-event-visualizer
npm install
npm run dev
```
### Expected Behavior
    - EdgeGatewayAgent emits camera events for CAMERA-001 and CAMERA-002.
    - Events are published to a direct exchange with routing keys matching interested parties.
    - Consumers (NVR/Storage services) receive only relevant events.
    - No cross-contamination between camera routes.
    - Web UI can be used to visualize routing in real time.

### Technologies Used
    - .NET 8 (ASP.NET Core Workers)
    - RabbitMQ (Direct Exchange)
    - React + Vite

