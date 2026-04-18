you MUST read and internalize the project-wide rules in: 👉 [./.ai/MASTER_INSTRUCTIONS.md]

# FitTech - Smart Gym Management Platform

FitTech is a modern, distributed gym management ecosystem built on .NET 9/10, using a microservices architecture orchestrated by **.NET Aspire**. It provides a comprehensive solution for managing members, coaches, subscriptions, class bookings, and gym access via NFC.

## 🏗️ Architecture & Technology Stack

- **Orchestration**: [.NET Aspire](https://learn.microsoft.com/en-us/dotnet/aspire/) for local development orchestration and service discovery.
- **Microservices**:
  - **Identity Service**: Handles authentication, user management, and Role-Based Access Control (Admin, Coach, Member).
  - **Membership Service**: Core business logic for members, subscription plans, NFC card management, and health profiles.
  - **Notification Service**: Real-time alerts and communication via RabbitMQ.
  - **Gateway**: [YARP](https://microsoft.github.io/reverse-proxy/) based API Gateway (integrated with Aspire).
- **Communication**:
  - **Asynchronous**: [Wolverine](https://wolverine.netlify.app/) for messaging over **RabbitMQ**.
  - **Synchronous**: [Refit](https://github.com/reactiveui/refit) for typed HTTP clients between services.
- **Data Persistence**: **PostgreSQL** using **Entity Framework Core**.
- **API Style**: Minimal APIs organized with **Carter**.
- **Documentation**: [Scalar](https://scalar.com/) for modern API reference and testing.

## 🚀 Getting Started

### Prerequisites
- .NET 9.0+ SDK (Note: Project uses some .NET 10 preview/RC packages).
- Docker Desktop or Podman (Required for Aspire to run Postgres and RabbitMQ containers).

### Running the Project
1. Clone the repository.
2. Ensure Docker is running.
3. Run the AppHost project:
   ```bash
   dotnet run --project FitTech.AppHost/FitTech.AppHost/FitTech.AppHost.csproj
   ```
4. Access the **Aspire Dashboard** (URL provided in console) to monitor services, logs, and traces.
5. API documentation is available via Scalar on each service's endpoint (e.g., `/scalar/v1`).

### Testing
- `test.http` file in the root directory contains sample requests for testing the APIs.

## 🛠️ Development Conventions

### Code Structure
- **Vertical Slice / Feature-based**: Logic is organized by features (e.g., `Services/Membership/Features/Members/CreateMember`).
- **Minimal APIs**: Use `Carter` modules to register routes.
- **Command/Query Handling**: Uses `Wolverine` for in-process mediation and external messaging.

### Patterns & Libraries
- **Result Pattern**: Uses `ErrorOr<T>` for functional error handling in handlers.
- **Validation**: `FluentValidation` for request validation, integrated into Wolverine middleware.
- **Dependency Management**: Centralized package management via `Directory.Packages.props`.
- **Response Wrapping**: Consistent API responses using `Shared/Wrappers/Response.cs`.

### Service Communication
- Define events in the `Shared` project or service-specific `Messaging/Events` folders.
- Use `IMessageBus` (Wolverine) to publish events.
- Use `Refit` interfaces for synchronous inter-service calls (e.g., `IIdentityServiceClient`).

## 📁 Directory Structure
- `FitTech.AppHost/`: Aspire orchestration logic.
- `FitTech.ServiceDefaults/`: Common service configurations (OpenTelemetry, health checks).
- `Services/`: Individual microservices.
- `Shared/`: Shared models, events, and common wrappers.
- `resources/diagrams/`: System architecture and database diagrams.
