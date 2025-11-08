# ShahdCooperative Notification Service

**Production-grade multi-channel notification microservice** for the ShahdCooperative ecosystem, handling email, SMS, push notifications, and real-time in-app notifications through event-driven architecture.

**Developer:** Omar Achbani - Full-Stack React .NET Developer
**Framework:** .NET 8
**Architecture:** Clean Architecture with CQRS

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Notification Channels](#notification-channels)
- [Event Integration](#event-integration)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)
- [Configuration](#configuration)

---

## Overview

ShahdCooperative Notification Service is a comprehensive notification management system that provides multi-channel communication capabilities. It consumes events from other microservices (AuthService, Main API) via RabbitMQ and delivers notifications through email, SMS, push notifications, and real-time in-app messaging.

### Key Highlights

- **Multi-Channel Delivery**: Email, SMS, Push, In-App notifications
- **Event-Driven**: RabbitMQ consumer for asynchronous event processing
- **Real-Time Updates**: SignalR for instant in-app notifications
- **Background Processing**: Hangfire for queued notification delivery
- **Template System**: Customizable notification templates
- **Retry Logic**: Automatic retry with exponential backoff using Polly
- **User Preferences**: Per-user notification channel preferences
- **Clean Architecture**: Separation of concerns with 4-layer design

---

## Architecture

This project follows **Clean Architecture** principles with event-driven design:

```
┌─────────────────────────────────────────────────────────┐
│                     API Layer                            │
│  Controllers, SignalR Hubs, Background Services          │
│  (HTTP, WebSocket, RabbitMQ Consumer)                    │
└───────────────────┬─────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                       │
│  Commands, Queries, Event Handlers, DTOs                 │
│  (CQRS with MediatR, Business Logic)                    │
└───────────────────┬─────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────────────────┐
│                   Domain Layer                           │
│  Entities, Interfaces, Enums, Domain Events              │
│  (Business Rules, Domain Logic)                          │
└───────────────────┬─────────────────────────────────────┘
                    │
┌─────────────────────────────────────────────────────────┐
│                Infrastructure Layer                      │
│  Repositories, External Services, RabbitMQ, Hangfire     │
│  (Data Access with Dapper, Email/SMS/Push Providers)    │
└─────────────────────────────────────────────────────────┘
```

### Design Patterns

- **CQRS**: Command Query Responsibility Segregation with MediatR
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Loose coupling and testability
- **Event-Driven Architecture**: RabbitMQ topic exchange consumer
- **Background Processing**: Hangfire for reliable job execution
- **Circuit Breaker**: Polly for fault tolerance
- **Strategy Pattern**: Multiple notification channel implementations

### Event Processing Flow

```
RabbitMQ Event → RabbitMQEventConsumer → MediatR Command
    ↓
Command Handler → Create Notification → Queue in Database
    ↓
Hangfire Background Job → Process Notification
    ↓
Notification Service (Email/SMS/Push/InApp)
    ↓
Update Status & Log → Send Real-Time Update (SignalR)
```

---

## Features

### Notification Queue Management

- **Queued Processing**: All notifications queued in database before sending
- **Priority Levels**: High, Normal, Low priority support
- **Status Tracking**: Pending, Sent, Failed, Retrying
- **Retry Logic**: Automatic retry with configurable max attempts
- **Next Retry Calculation**: Exponential backoff for failed deliveries
- **Error Logging**: Detailed error messages for troubleshooting

### Template System

- **Template Management**: Create, update, delete notification templates
- **Template Types**: Email, SMS, Push, InApp
- **Dynamic Content**: Variable substitution in templates
- **Template Keys**: Unique identifier for each template type
- **Version Control**: Track template changes over time
- **Multi-Channel Templates**: Same event, different templates per channel

### In-App Notifications

- **Real-Time Delivery**: SignalR hub for instant notifications
- **Notification Types**: Info, Success, Warning, Error, System
- **Read/Unread Tracking**: Mark as read functionality
- **Pagination**: Efficient retrieval of large notification lists
- **Expiration Support**: Auto-cleanup of expired notifications
- **Action URLs**: Deep links to relevant application pages
- **Categories**: Organize notifications by category

### User Preferences

- **Channel Control**: Enable/disable per channel (Email, SMS, Push, InApp)
- **Preference Management**: User-specific notification settings
- **Respect Preferences**: Honor user choices during delivery
- **Default Settings**: Sensible defaults for new users

### Device Token Management

- **Multi-Device Support**: Track multiple devices per user
- **Device Types**: iOS, Android, Web
- **Token Lifecycle**: Active/inactive status tracking
- **Last Used Tracking**: Monitor device activity
- **Auto-Cleanup**: Remove stale/inactive devices

### Background Processing

- **Hangfire Integration**: Reliable background job processing
- **Recurring Jobs**: Scheduled notification processing
- **Job Dashboard**: Monitor job execution and failures
- **Retry Policies**: Automatic retry for failed jobs
- **Batch Processing**: Process multiple notifications efficiently

---

## Technology Stack

### Core Technologies
- **.NET 8** - Latest LTS framework
- **C#** - Primary programming language
- **ASP.NET Core** - Web API and SignalR hosting

### Data Access & Storage
- **SQL Server** - Primary database
- **Dapper** - High-performance micro-ORM
- **Microsoft.Data.SqlClient** - SQL Server driver

### Architecture & Patterns
- **MediatR** - CQRS implementation and mediator pattern
- **FluentValidation** - Request validation
- **AutoMapper** - Object-to-object mapping

### Background Processing
- **Hangfire** - Background job processing
- **Hangfire.SqlServer** - SQL Server storage for Hangfire
- **Hangfire.AspNetCore** - ASP.NET Core integration

### Real-Time Communication
- **SignalR** - Real-time WebSocket communication
- **SignalR Client** - Client libraries for browsers and apps

### Messaging & Events
- **RabbitMQ.Client** - AMQP client for event consumption
- **Topic Exchange** - Event routing with routing keys

### Resilience & Reliability
- **Polly** - Retry policies, circuit breaker, timeout
- **Polly.Extensions.Http** - HTTP client resilience

### External Integrations
- **SendGrid** - Email delivery service (optional)
- **Twilio** - SMS delivery service (optional)
- **Firebase Cloud Messaging** - Push notification service (optional)
- **SMTP** - Standard email protocol support

### Logging & Monitoring
- **Serilog** - Structured logging
- **Serilog.Sinks.Console** - Console output
- **Serilog.Sinks.File** - File logging
- **Serilog.Sinks.Elasticsearch** - Centralized logging (optional)

---

## Project Structure

```
ShahdCooperative.NotificationService/
│
├── ShahdCooperative.NotificationService.API/
│   ├── Controllers/
│   │   ├── NotificationsController.cs    # Notification management
│   │   ├── TemplatesController.cs        # Template CRUD
│   │   └── PreferencesController.cs      # User preferences
│   ├── Hubs/
│   │   └── NotificationHub.cs            # SignalR real-time hub
│   ├── BackgroundServices/
│   │   ├── RabbitMQEventConsumer.cs      # Event consumer
│   │   └── NotificationProcessor.cs      # Background processor
│   ├── Program.cs                         # Application startup
│   └── appsettings.json                   # Configuration
│
├── ShahdCooperative.NotificationService.Application/
│   ├── Commands/
│   │   ├── ProcessUserRegisteredCommand.cs
│   │   ├── ProcessOrderCreatedCommand.cs
│   │   ├── ProcessPasswordChangedCommand.cs
│   │   ├── ProcessFeedbackReceivedCommand.cs
│   │   ├── SendNotificationCommand.cs
│   │   ├── MarkAsReadCommand.cs
│   │   └── UpdatePreferencesCommand.cs
│   ├── Queries/
│   │   ├── GetInAppNotificationsQuery.cs
│   │   ├── GetUnreadCountQuery.cs
│   │   └── GetNotificationPreferencesQuery.cs
│   ├── DTOs/                              # Data Transfer Objects
│   ├── Validators/                        # FluentValidation validators
│   └── DependencyInjection.cs
│
├── ShahdCooperative.NotificationService.Domain/
│   ├── Entities/
│   │   ├── NotificationQueue.cs           # Queued notifications
│   │   ├── NotificationTemplate.cs        # Email/SMS templates
│   │   ├── InAppNotification.cs           # In-app notification
│   │   ├── NotificationLog.cs             # Delivery history
│   │   ├── NotificationPreference.cs      # User preferences
│   │   └── DeviceToken.cs                 # Push notification tokens
│   ├── Interfaces/
│   │   ├── INotificationQueueRepository.cs
│   │   ├── INotificationTemplateRepository.cs
│   │   ├── IInAppNotificationRepository.cs
│   │   ├── INotificationLogRepository.cs
│   │   ├── INotificationPreferenceRepository.cs
│   │   ├── IDeviceTokenRepository.cs
│   │   ├── IEmailService.cs
│   │   ├── ISmsService.cs
│   │   └── IPushNotificationService.cs
│   ├── Enums/
│   │   ├── NotificationType.cs            # Email, SMS, Push, InApp
│   │   ├── NotificationPriority.cs        # High, Normal, Low
│   │   └── NotificationStatus.cs          # Pending, Sent, Failed
│   └── Events/
│       └── NotificationSentEvent.cs
│
├── ShahdCooperative.NotificationService.Infrastructure/
│   ├── Repositories/
│   │   ├── NotificationQueueRepository.cs
│   │   ├── NotificationTemplateRepository.cs
│   │   ├── InAppNotificationRepository.cs
│   │   ├── NotificationLogRepository.cs
│   │   ├── NotificationPreferenceRepository.cs
│   │   └── DeviceTokenRepository.cs
│   ├── Services/
│   │   ├── EmailService.cs                # Email sending
│   │   ├── SmsService.cs                  # SMS sending
│   │   ├── PushNotificationService.cs     # Push notifications
│   │   └── NotificationTemplateService.cs # Template rendering
│   └── DependencyInjection.cs
│
└── Tests/
    ├── ShahdCooperative.NotificationService.API.Tests/
    ├── ShahdCooperative.NotificationService.Application.Tests/
    ├── ShahdCooperative.NotificationService.Application.IntegrationTests/
    └── ShahdCooperative.NotificationService.Infrastructure.Tests/
```

---

## Notification Channels

### Email Notifications

**Features:**
- SMTP support for standard email servers
- SendGrid integration for scalable delivery
- HTML and plain-text templates
- Attachment support
- CC and BCC recipients
- Template variable substitution

**Use Cases:**
- Welcome emails on registration
- Order confirmation emails
- Password reset emails
- Order shipping notifications
- Admin alerts

**Configuration:**
```json
{
  "EmailSettings": {
    "Provider": "SMTP",
    "FromEmail": "noreply@shahdcooperative.com",
    "FromName": "ShahdCooperative",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "app-password",
    "UseSsl": true
  }
}
```

### SMS Notifications

**Features:**
- Twilio integration
- Vonage (Nexmo) support
- International number support
- Delivery status tracking
- Character count validation

**Use Cases:**
- Two-factor authentication codes
- Order status updates
- Urgent alerts
- Delivery notifications

**Configuration:**
```json
{
  "SmsSettings": {
    "Provider": "Twilio",
    "AccountSid": "your-twilio-sid",
    "AuthToken": "your-twilio-token",
    "FromNumber": "+1234567890"
  }
}
```

### Push Notifications

**Features:**
- Firebase Cloud Messaging (FCM)
- iOS and Android support
- Web push notifications
- Device token management
- Badge and sound customization
- Deep linking support

**Use Cases:**
- Real-time order updates
- New product announcements
- Promotional campaigns
- System alerts

**Configuration:**
```json
{
  "PushSettings": {
    "Provider": "FCM",
    "ServerKey": "your-fcm-server-key",
    "SenderId": "your-sender-id"
  }
}
```

### In-App Notifications

**Features:**
- Real-time delivery via SignalR
- Notification center/inbox
- Read/unread status
- Categorization
- Action buttons and deep links
- Expiration dates

**Use Cases:**
- System messages
- Feature announcements
- Account alerts
- Activity notifications

**SignalR Hub:**
- Hub URL: `/hubs/notification`
- Methods: `ReceiveNotification`, `NotificationRead`
- Authentication: JWT bearer token required

---

## Event Integration

### Events Consumed from RabbitMQ

This service listens to the following routing keys on the `shahdcooperative.events` exchange:

#### User Events (from AuthService)

1. **`user.registered`**
   - Trigger: New user registration
   - Action: Send welcome email
   - Template: `welcome-email`
   - Channels: Email, InApp

2. **`user.logged-in`**
   - Trigger: Successful login
   - Action: Log login activity (optional notification)
   - Channels: InApp (optional)

3. **`password.changed`**
   - Trigger: Password successfully changed
   - Action: Send confirmation email
   - Template: `password-changed-email`
   - Channels: Email, InApp

#### Order Events (from Main Service)

4. **`order.created`**
   - Trigger: New order placed
   - Action: Send order confirmation
   - Template: `order-confirmation-email`
   - Channels: Email, SMS (optional), InApp

5. **`order.shipped`**
   - Trigger: Order shipped
   - Action: Send shipping notification with tracking
   - Template: `order-shipped-email`
   - Channels: Email, SMS, Push, InApp

#### Product Events (from Main Service)

6. **`product.out-of-stock`**
   - Trigger: Product stock below threshold
   - Action: Alert admin of low inventory
   - Template: `low-stock-alert-email`
   - Channels: Email (admin), InApp (admin)

#### Feedback Events (from Main Service)

7. **`feedback.received`**
   - Trigger: Customer submits feedback
   - Action: Notify admin, acknowledge customer
   - Templates: `feedback-admin-alert`, `feedback-acknowledgment`
   - Channels: Email (both), InApp (admin)

### Event Processing Architecture

**RabbitMQ Configuration:**
- Exchange: `shahdcooperative.events` (topic exchange)
- Queue: `notification-service-queue`
- Bindings: All routing keys listed above
- Durability: Durable queue for reliability
- Auto-Recovery: Enabled with 5-second interval
- Prefetch Count: 10 messages

**Consumer Behavior:**
- Acknowledges messages only after successful processing
- Negative acknowledgment (nack) on failures with requeue
- Dead letter queue for permanently failed messages
- Exponential backoff for retries

---

## API Endpoints

### Notification Management

```http
POST /api/notifications/send
Authorization: Bearer {token}
Content-Type: application/json

{
  "userId": "user-guid",
  "type": "Email",
  "subject": "Important Update",
  "message": "Your order has shipped!",
  "priority": "High"
}
```

### In-App Notifications

```http
GET /api/notifications/in-app?pageNumber=1&pageSize=20
Authorization: Bearer {token}

Response: 200 OK
{
  "items": [
    {
      "id": "guid",
      "title": "Order Shipped",
      "message": "Your order #ORD-20251108-00042 has shipped",
      "type": "Info",
      "isRead": false,
      "createdAt": "2025-11-08T14:30:00Z",
      "actionUrl": "/orders/order-guid"
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 20
}
```

```http
GET /api/notifications/unread-count
Authorization: Bearer {token}

Response: 200 OK
{
  "count": 5
}
```

```http
POST /api/notifications/in-app/{id}/mark-read
Authorization: Bearer {token}

Response: 200 OK
```

### Template Management

```http
GET /api/templates
GET /api/templates/{id}
POST /api/templates                # Admin only
PUT /api/templates/{id}            # Admin only
DELETE /api/templates/{id}         # Admin only
```

**Example: Create Template**
```http
POST /api/templates
Authorization: Bearer {adminToken}
Content-Type: application/json

{
  "key": "order-confirmation-email",
  "name": "Order Confirmation",
  "type": "Email",
  "subject": "Order Confirmation - {{OrderNumber}}",
  "body": "<h1>Thank you for your order!</h1><p>Order #{{OrderNumber}} total: {{TotalAmount}}</p>",
  "isActive": true
}
```

### User Preferences

```http
GET /api/preferences
Authorization: Bearer {token}

Response: 200 OK
{
  "userId": "user-guid",
  "emailEnabled": true,
  "smsEnabled": false,
  "pushEnabled": true,
  "inAppEnabled": true
}
```

```http
PUT /api/preferences
Authorization: Bearer {token}
Content-Type: application/json

{
  "emailEnabled": true,
  "smsEnabled": true,
  "pushEnabled": true,
  "inAppEnabled": true
}
```

---

## Getting Started

### Prerequisites

- **.NET 8 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **SQL Server 2022** or newer
- **RabbitMQ** - Message broker
- **Visual Studio 2022** / **VS Code** / **Rider**

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/Omar1Ach/ShahdCooperative-NotificationService.git
cd ShahdCooperative-NotificationService
```

2. **Configure Database**

The service uses the shared `ShahdCooperative` database with `Notification` schema. Ensure the database exists with required tables.

3. **Configure Application**

Update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ShahdCooperative;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "shahdcooperative.events",
    "QueueName": "notification-service-queue",
    "RoutingKeys": [
      "user.registered",
      "user.logged-in",
      "password.changed",
      "order.created",
      "order.shipped",
      "product.out-of-stock",
      "feedback.received"
    ]
  }
}
```

4. **Restore Dependencies**
```bash
dotnet restore
```

5. **Build the Project**
```bash
dotnet build
```

6. **Run the Application**
```bash
dotnet run --project ShahdCooperative.NotificationService.API
```

7. **Access Swagger UI**
```
https://localhost:5003/swagger
```

8. **Access Hangfire Dashboard**
```
https://localhost:5003/hangfire
```

---

## Configuration

### Notification Settings

```json
{
  "NotificationSettings": {
    "MaxRetries": 3,
    "RetryDelayMinutes": 5,
    "BatchSize": 50,
    "ProcessingIntervalSeconds": 30
  }
}
```

### RabbitMQ Settings

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "shahdcooperative.events",
    "ExchangeType": "topic",
    "QueueName": "notification-service-queue",
    "PrefetchCount": 10,
    "AutomaticRecoveryEnabled": true,
    "NetworkRecoveryInterval": 5
  }
}
```

### Hangfire Configuration

```json
{
  "Hangfire": {
    "DashboardPath": "/hangfire",
    "WorkerCount": 5,
    "PollingInterval": 15
  }
}
```

---

## Background Processing

### Hangfire Jobs

**Notification Processing Job:**
- **Schedule**: Every 30 seconds
- **Function**: Process pending notifications from queue
- **Batch Size**: 50 notifications per run
- **Retry**: Failed notifications requeued with exponential backoff

**Cleanup Job:**
- **Schedule**: Daily at 2 AM
- **Function**: Remove expired in-app notifications
- **Function**: Archive old notification logs

**Device Token Cleanup:**
- **Schedule**: Weekly
- **Function**: Remove inactive device tokens (not used in 90 days)

### Job Monitoring

Access the Hangfire dashboard at `/hangfire` to:
- Monitor job execution
- View failed jobs
- Manually trigger jobs
- See job history and statistics

---

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Unit Tests Only
```bash
dotnet test --filter "FullyQualifiedName!~IntegrationTests"
```

### Run Integration Tests
```bash
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

### Test Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

---

## Performance Considerations

- **Dapper ORM**: High-performance data access (10x faster than EF Core for reads)
- **Async/Await**: All I/O operations are asynchronous
- **Connection Pooling**: SQL Server connection pooling enabled
- **Batch Processing**: Process notifications in batches to reduce overhead
- **SignalR**: Scalable real-time communication
- **Hangfire**: Distributed job processing for horizontal scaling
- **Polly**: Circuit breaker prevents cascading failures
- **Message Prefetch**: RabbitMQ prefetch for controlled consumption

---

## License

**Proprietary License** - Copyright © 2025 ShahdCooperative
All rights reserved. Unauthorized copying, modification, distribution, or use of this software is strictly prohibited.

---

## Developer

**Omar Achbani**
Full-Stack React .NET Developer

- GitHub: [@Omar1Ach](https://github.com/Omar1Ach)
- Project: [ShahdCooperative Notification Service](https://github.com/Omar1Ach/ShahdCooperative-NotificationService)

---

**Built for ShahdCooperative**
