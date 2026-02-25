# ProtectedPayment - Protected Payment System 🛒💳

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![Apache Kafka](https://img.shields.io/badge/Apache%20Kafka-231F20?style=for-the-badge&logo=apache-kafka&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

</div>

---

## 📌 Project Introduction

**ProtectedPayment** is a protected payment system implementation for e-commerce platforms, designed according to **Clean Architecture** principles and using asynchronous messaging technologies. It is designed around a scenario where users have the right to hold or cancel payment for 10 minutes after placing an order.

### 🎯 Key Features:
- ✅ **Clean Architecture**: Domain-centric, layered architecture with inward dependencies
- ✅ **Order Management**: Management of pending, shipping, and cancellation events
- ✅ **10-Minute Cancellation Window**: Users can hold or cancel payment after placing an order
- ✅ **Asynchronous Processing**: Payment is held in provision, operations are performed asynchronously
- ✅ **Three Different Messaging Technologies**: RabbitMQ, Apache Kafka, and Redis Streams
- ✅ **Outbox Pattern**: Consistency guarantee with transactional messaging
- ✅ **Inbox Pattern**: Idempotent message processing
- ✅ **Event-Driven Architecture**: Loose coupling with asynchronous messaging
- ✅ **Scalable Architecture**: Layered and testable structure
- ✅ **Docker Support**: Containerized deployment

---

## 🏗️ Clean Architecture Structure

The project is organized into layers according to **Clean Architecture** principles:

```
ProtectedPayment/
├── src/
│   ├── domain/                          # Domain Layer (Innermost layer)
│   │   ├── ProtectedPayment.Domain/     # Business rules and domain logic
│   │   │   ├── Entities/                # Domain entities (Order, OutboxMessage, InboxMessage, IdempotencyKey)
│   │   │   ├── Events/                  # Event models (OrderPendingEvent, OrderCancelledEvent, etc.)
│   │   │   ├── Repositories/            # Repository interfaces (IOrderRepository, IUnitOfWork, etc.)
│   │   │   └── Errors/                  # Domain errors and Result types
│   │   └── ProtectedPayment.SharedKernel/  # Shared domain primitives
│   │
│   ├── application/                     # Application Layer
│   │   ├── ProtectedPayment.Application/           # Use cases and business logic
│   │   │   ├── Orders/                  # Order service and use cases
│   │   │   ├── OutboxMessages/          # Outbox pattern implementation
│   │   │   └── InboxMessages/           # Inbox pattern implementation
│   │   └── ProtectedPayment.Application.Contracts/ # DTOs and Contracts
│   │
│   ├── infrastructure/                  # Infrastructure Layer
│   │   └── ProtectedPayment.Infrastructure/
│   │       ├── Database/                # DbContext, Configurations, Migrations
│   │       ├── Repositories/            # Repository implementations
│   │       └── Messaging/               # Messaging implementations
│   │           ├── RabbitMQ/            # RabbitMQ bus service
│   │           ├── Kafka/               # Kafka bus service
│   │           └── Redis/               # Redis bus service
│   │
│   └── host/                            # Presentation/Host Layer
│       ├── ProtectedPayment.Order.API/  # RESTful API (HTTP endpoints)
│       └── ProtectedPayment.Worker/     # Background services (Message consumers)
│
└── docker-compose files                 # Container orchestration
```

### 🎯 Layer Responsibilities

#### 1️⃣ **Domain Layer** (`domain/`)
- **Dependencies**: No dependencies on any layer (completely isolated)
- **Contents**: 
  - Domain entities (Order, OutboxMessage, InboxMessage, IdempotencyKey)
  - Event models (OrderPendingEvent, OrderCancelledEvent, OrderConfirmedEvent, OrderShippedEvent)
  - Repository interfaces
  - Domain errors and business rules
  - SharedKernel (Shared types, primitives)

#### 2️⃣ **Application Layer** (`application/`)
- **Dependencies**: Only depends on Domain Layer
- **Contents**:
  - Use cases and application services (OrderService)
  - Outbox Pattern implementation (OutboxMessageService)
  - Inbox Pattern implementation (InboxMessageService)
  - DTOs and Contracts
  - Business orchestration

#### 3️⃣ **Infrastructure Layer** (`infrastructure/`)
- **Dependencies**: Depends on Domain and Application layers
- **Contents**:
  - Entity Framework Core DbContext
  - Repository implementations
  - Messaging implementations (RabbitMQ, Kafka, Redis)
  - Database migrations
  - External service integrations
  - SaveChangesInterceptor (for Outbox pattern)

#### 4️⃣ **Host Layer** (`host/`)
- **Dependencies**: Depends on all layers (composition root)
- **Contents**:
  - **Order.API**: RESTful API endpoints, controllers
  - **Worker**: Background services (OrderPendingConsumer, OrderCancelledConsumer, etc.)
  - Dependency Injection configuration
  - Middleware pipeline
  - Application startup

---

## 🔄 Workflow

### Event-Driven Flow
```
1️⃣ User creates an order
   ↓
2️⃣ Payment is pending. (Status: Pending)
   ↓
3️⃣ The 10-minute timer starts
   ↓
4️⃣ If the user cancels → Payment will be refunded
   OR
   If 10 minutes pass → Payment will be automatically confirmed
   ↓
5️⃣ Order proceeds to shipping stage
```

### Messaging Patterns

#### 🔸 **Outbox Pattern**
Generated events are written to the `OutboxMessage` table within a database transaction. A background service reads these messages and sends them to the message broker. This solves the distributed transaction problem.

#### 🔸 **Inbox Pattern**
Incoming messages are recorded in the `InboxMessage` table before processing. If the same message arrives again, an idempotency check is performed and the operation is skipped.

---

## ⚡ Technologies Used

### Backend Framework
- **.NET 10** - Modern, high-performance web API and background service applications
- **ASP.NET Core** - Order API
- **.NET Worker Service** - Background services (Event consumers)
- **Entity Framework Core 10** - ORM

### Messaging Systems
- **RabbitMQ** - Delayed processing with TTL + Dead Letter Exchange
- **Apache Kafka** - Timestamp-based delayed message handling
- **Redis** - Scheduled job processing with Sorted Set

### Database & ORM
- **PostgreSQL** - Relational database
- **Entity Framework Core 10** - ORM and database management
- **Entity Framework Core Migrations** - Database versioning
- **SaveChangesInterceptor** - Automatic event recording for Outbox pattern

### Design Patterns & Principles
- **Clean Architecture** - Domain-centric layered architecture
- **Outbox Pattern** - Transactional messaging guarantee
- **Inbox Pattern** - Idempotent message processing
- **Repository Pattern** - Data access abstraction
- **Unit of Work Pattern** - Transaction management
- **Event-Driven Architecture** - Asynchronous messaging between services
- **Dependency Injection** - Inversion of Control

### Other Libraries
- **Confluent.Kafka** (v2.13.0) - Kafka client
- **StackExchange.Redis** (v2.10.1) - Redis client
- **RabbitMQ.Client** (v7.2.0) - RabbitMQ client
- **Newtonsoft.Json** (v13.0.4) - JSON serialization
- **Polly** (v8.6.5) - Resilience and error handling
- **Scalar.AspNetCore** - API documentation
- **ETPackages.Result** (v1.0.0) - Result pattern implementation and error handling
- **ETPackages.Endpoints** (v2.0.0) - Minimal API endpoint definition infrastructure

### Containerization
- **Docker** - Application containerization
- **Docker Compose** - Multi-container orchestration

---

## 🛠 Installation Instructions

### Requirements
- .NET 10 SDK ([Download](https://dotnet.microsoft.com/download))
- Docker & Docker Compose ([Download](https://www.docker.com/products/docker-desktop))
- Git ([Download](https://git-scm.com))

### 1️⃣ Clone the Project

```bash
git clone https://github.com/etartar/ProtectedPayment.git
cd ProtectedPayment
```

### 2️⃣ Install Dependencies

```bash
dotnet restore
```

### 3️⃣ Start the System with Docker

#### Option A: Using RabbitMQ
```bash
docker-compose -f docker-compose.rabbitmq.yml up -d
```

#### Option B: Using Kafka
```bash
docker-compose -f docker-compose.kafka.yml up -d
```

#### Option C: Using Redis
```bash
docker-compose -f docker-compose.redis.yml up -d
```

#### Option D: If you want to debug the project in Visual Studio environment
```bash
docker-compose -f docker-compose.yml up -d
```

When you run the docker-compose file, all services will start up and you can debug the project in your desired environment from VS RabbitMQ Profile, VS Kafka Profile, and VS Redis Profile sections.

---

## 🧪 Usage Examples

### 1️⃣ List All Orders
```bash
GET /orders
```

### 2️⃣ Create Order
```bash
POST /orders
Content-Type: application/json

{
  "productName": "Iphone 16",
  "amount": 120000
}
```

### 3️⃣ Cancel Order
```bash
POST /orders/019c0bd7-2368-7c43-a703-c16761405ab3/cancel
Content-Type: application/json
```

---

## 📚 API Documentation

After the Order API is started, access the Scalar UI:

```
http://localhost:5300/scalar/v1
```

---

## 📧 Contact

You can open GitHub Issues for questions or suggestions.

---

## 🙏 Acknowledgments

- Backend Guru training program
- .NET and open source community
