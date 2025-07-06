# Plus Appointment – Development Setup

Welcome to the **Plus Appointment** backend project. This guide will help you run the application locally using Docker with hot reload and persistent services like PostgreSQL and Redis.

---

## Prerequisites

- [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/install/)
- .NET SDK 8.0+ (for local builds or testing outside Docker, optional)

---

## Run the App in Development Mode (with Hot Reload)

This will start the ASP.NET Core server using `dotnet watch run`, and spin up PostgreSQL and Redis for local testing.

```bash
docker compose -f docker-compose.dev.yml build
docker compose -f docker-compose.dev.yml up -d
