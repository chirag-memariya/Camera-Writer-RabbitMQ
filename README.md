#  Camera Event Routing System

This project demonstrates a RabbitMQ-based event routing system for multiple camera sources and interested services.

##  Project Structure

- `EdgeGatewayAgent` – Simulates camera event generation and publishes events via RabbitMQ.
- `NVR1Service`     – Subscribes to `"CAMERA-001"` routing key.
- `NVR2Service`     – Subscribes to `"CAMERA-002"` routing key.
- `Storage1Service` – Subscribes to `"CAMERA-001"` routing key.
- `Storage2Service` – Subscribes to `"CAMERA-002"` routing key.
- `camera-event-visualizer` – Vite + React frontend for visualizing event flow (optional).


##  Getting Started

**Github Repo :-**  [ Camera-Writer-RabbitMQ](https://github.com/chirag-memariya/Camera-Writer-RabbitMQ)  
### 1. Start All Services with Docker Compose
    docker compose up --build
This builds and starts RabbitMQ, EdgeGatewayAgent, NVR services, Storage services, and the Web UI — all using docker-compose.yml.

### 2. 🐰 Access RabbitMQ Management UI
    http://localhost:15672

**Username: guest**     
**Password: guest**

### 3. 🌐 Access Services
Service	URL

    EdgeGatewayAgent	http://localhost:8080
    NVR1 Service	    http://localhost:5001
    NVR2 Service	    http://localhost:5002
    Storage1 Service	http://localhost:5003
    Storage2 Service	http://localhost:5004
    Web UI (Visualizer)	http://localhost:5172

#### Expected Behavior
- EdgeGatewayAgent emits events for CAMERA-001 and CAMERA-002.
- Events are published to a RabbitMQ direct exchange based on routing keys.
- NVR and Storage services consume only the events relevant to them.
- No cross-event delivery between camera consumers.
- Web UI shows event routing visually in real-time.

#### Technologies Used
- .NET 8 (ASP.NET Core Minimal APIs + Background Workers)
- RabbitMQ (Header Exchange)
- React + Vite (Web Frontend)
- Docker Compose (Multi-service orchestration)

