# SmartProjectManagementSystem (SPMS)

A microservices-based project management system built with .NET 9.0, ASP.NET Core, and Docker.

## Services
- UserService: Manages user registration, login, and roles.
- ProjectService: Manages projects and assignments.
- TaskService: Manages tasks within projects.
- TeamService: Manages teams and members.
- ApiGateway: Routes requests to appropriate services.

## Setup
1. Install .NET 9.0 SDK.
2. Run `docker-compose up` to start services.
3. Access APIs via `https://localhost:7169`.

## Tech Stack
- Backend: .NET 9.0, ASP.NET Core
- Database: SQL Server
- Cache: Redis
- Message Queue: RabbitMQ
- Authentication: JWT
- Testing: xUnit, Moq
- Containerization: Docker, Docker Compose
