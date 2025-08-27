# OdixPay Notifications System

A comprehensive notification system built with .NET 8, following Clean Architecture principles with Domain, Application, Infrastructure, and API layers.

## Architecture

### Layers

- **Domain**: Core business entities, interfaces, and enums
- **Application**: Business logic, CQRS commands/queries, and handlers using MediatR
- **Infrastructure**: Data access with Dapper, external service implementations
- **API**: RESTful API controllers and dependency injection configuration

### Technologies

- **.NET 8**
- **Dapper** for data access
- **SQL Server** with stored procedures
- **MediatR** for CQRS pattern
- **Swagger** for API documentation

## Features

### Notification Types
- Email
- SMS
- Push Notifications
- In-App Notifications

### Notification Management
- Create and send notifications
- Template-based notifications
- Retry mechanism for failed notifications
- Status tracking (Pending, Sent, Delivered, Failed, Cancelled)
- Priority levels (Low, Normal, High, Critical)
- Scheduled notifications

### API Endpoints

#### Notifications
- `POST /api/notifications` - Create a new notification
- `GET /api/notifications/{id}` - Get notification by ID
- `GET /api/notifications/user/{userId}` - Get user notifications with pagination
- `GET /api/notifications/user/{userId}/unread-count` - Get unread notification count
- `POST /api/notifications/{id}/send` - Send a specific notification
- `POST /api/notifications/{id}/mark-read` - Mark notification as read

#### Templates
- `POST /api/templates` - Create a new template
- `GET /api/templates/{id}` - Get template by ID
- `GET /api/templates/by-name/{name}` - Get template by name

## Database Setup

1. Execute the SQL scripts in the `database` folder in order:
   - `01_CreateTables.sql` - Creates database and tables
   - `02_NotificationStoredProcedures.sql` - Creates notification-related stored procedures
   - `03_TemplateStoredProcedures.sql` - Creates template-related stored procedures

2. Update the connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=OdixPayNotifications;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

## Getting Started

1. **Prerequisites**
   - .NET 8 SDK
   - SQL Server
   - Visual Studio 2022 or VS Code

2. **Setup**
   ```bash
   # Restore packages
   dotnet restore

   # Build the solution
   dotnet build

   # Run the API
   cd src/OdixPay.Notifications.API
   dotnet run
   ```

3. **Access the API**
   - Swagger UI: `https://localhost:7001/swagger`
   - API Base URL: `https://localhost:7001/api`

## Project Structure

```
src/
├── OdixPay.Notifications.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Interfaces/
├── OdixPay.Notifications.Application/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   └── DTOs/
├── OdixPay.Notifications.Infrastructure/
│   ├── Data/
│   ├── Repositories/
│   └── Services/
└── OdixPay.Notifications.API/
    └── Controllers/

database/
├── 01_CreateTables.sql
├── 02_NotificationStoredProcedures.sql
└── 03_TemplateStoredProcedures.sql
```

## Configuration

### Email Service
TODO: Configure SMTP settings or integrate with SendGrid, AWS SES, etc.

### SMS Service
TODO: Configure Twilio, AWS SNS, or other SMS providers

### Push Notifications
TODO: Configure Firebase FCM, Apple APNS, or other push notification services

## Development Notes

- The infrastructure services (Email, SMS, Push) are currently implemented as placeholders
- All database operations use stored procedures for optimal performance
- The system supports retry mechanisms for failed notifications
- Templates support variable substitution using `{{variableName}}` syntax
- Notifications can be scheduled for future delivery

## Next Steps

1. Implement actual email service integration
2. Implement SMS service integration
3. Implement push notification service integration
4. Add authentication and authorization
5. Add background job processing for scheduled notifications
6. Add notification analytics and reporting
7. Add webhook support for delivery confirmations
