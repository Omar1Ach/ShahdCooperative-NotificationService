# Docker Setup Guide

## Prerequisites
- Docker Desktop installed
- Docker Compose installed

## Running with Docker Compose

### Start all services (SQL Server, RabbitMQ, NotificationService API)

```bash
docker-compose up -d
```

This will start:
- **SQL Server Developer 2022** on port `1433`
- **RabbitMQ** on ports `5672` (AMQP) and `15672` (Management UI)
- **NotificationService API** on port `5093`

### View logs

```bash
docker-compose logs -f
```

### View logs for specific service

```bash
docker-compose logs -f notification-api
```

### Stop all services

```bash
docker-compose down
```

### Stop and remove volumes (clean database)

```bash
docker-compose down -v
```

## Service URLs

- **NotificationService API**: http://localhost:5093
- **NotificationService Swagger**: http://localhost:5093/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)

## SQL Server Connection

**Connection String for external tools (SSMS, Azure Data Studio):**
```
Server=localhost,1433;Database=ShahdCooperative;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True
```

## Development Modes

### Option 1: Run locally without Docker
```bash
dotnet run --project ShahdCooperative.NotificationService.API
```
Uses local SQL Server Express (`.\\SQLEXPRESS`)

### Option 2: Run with Docker
```bash
docker-compose up
```
Uses containerized SQL Server Developer edition

## Environment Variables

You can override any configuration in `docker-compose.yml`:

```yaml
environment:
  - ConnectionStrings__DefaultConnection=YourConnectionString
  - RabbitMQ__Host=your-rabbitmq-host
  - EmailSettings__SmtpHost=your-smtp-host
  # etc...
```

## Building the Docker Image Manually

```bash
docker build -t shahdcooperative-notification-api .
```

## Running Single Container

```bash
docker run -p 5093:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=ShahdCooperative;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True" \
  -e RabbitMQ__Host=host.docker.internal \
  shahdcooperative-notification-api
```

## Database Migrations

The application uses Hangfire which auto-creates tables on startup. No manual migration needed.

## Troubleshooting

### SQL Server container won't start
- Ensure you have enough memory allocated to Docker (minimum 2GB)
- Check if port 1433 is already in use

### RabbitMQ connection fails
- Wait for RabbitMQ to fully start (check health: `docker-compose ps`)
- Ensure port 5672 is not in use

### API can't connect to SQL Server
- Check SQL Server health: `docker-compose ps`
- Verify SQL Server is accepting connections
- Check connection string in `docker-compose.yml`
