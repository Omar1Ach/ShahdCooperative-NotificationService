# ShahdCooperative Notification Service

**Production-grade notification microservice** for the ShahdCooperative ecosystem, handling email, SMS, push notifications, and in-app notifications through event-driven architecture.

**Author:** Omar Achbani
**Framework:** .NET 8
**Architecture:** Clean Architecture with CQRS

## Overview

This microservice consumes events from RabbitMQ (Auth Service, Main API) and sends notifications through multiple channels:
- Email (SMTP, SendGrid, AWS SES)
- SMS (Twilio, Vonage)
- Push Notifications (Firebase Cloud Messaging)
- In-App Notifications (SignalR real-time)

## Technology Stack

- **.NET 8** - Framework
- **Dapper** - High-performance data access
- **RabbitMQ** - Event consumption
- **Hangfire** - Background job processing
- **SignalR** - Real-time notifications
- **MediatR** - CQRS pattern
- **Polly** - Retry policies and circuit breakers
- **Serilog** - Structured logging

## Project Structure

```
├── API/                  # Controllers, SignalR Hubs, Background Services
├── Application/          # Commands, Queries, DTOs, Handlers
├── Domain/              # Entities, Interfaces, Enums
└── Infrastructure/      # Repositories, External Services (Email, SMS, Push)
```

## Features (In Development)

- [ ] RabbitMQ Event Consumer
- [ ] Email Service (SMTP, SendGrid, AWS SES)
- [ ] SMS Service (Twilio, Vonage)
- [ ] Push Notification Service (FCM)
- [ ] In-App Notifications with SignalR
- [ ] Template Engine
- [ ] Notification Queue System
- [ ] Background Job Processing (Hangfire)
- [ ] User Preferences Management
- [ ] Retry Policies and Circuit Breakers

## Database

Uses the shared `ShahdCooperative` database with `Notification` schema:
- NotificationLogs (existing)
- NotificationTemplates
- NotificationPreferences
- InAppNotifications
- NotificationQueue
- DeviceTokens

## Getting Started

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run API
dotnet run --project ShahdCooperative.NotificationService.API
```

## Configuration

Update `appsettings.json` with:
- Database connection string
- RabbitMQ settings
- Email provider credentials
- SMS provider credentials
- Firebase credentials

---

**Developed by Omar Achbani**
