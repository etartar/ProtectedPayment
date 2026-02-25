# ProtectedPayment - Korumalı Ödeme Sistemi 🛒💳

<div align="center">

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![Apache Kafka](https://img.shields.io/badge/Apache%20Kafka-231F20?style=for-the-badge&logo=apache-kafka&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

</div>

---

## 📌 Proje Tanıtımı

**ProtectedPayment**, e-ticaret platformları için geliştirilmiş, **Clean Architecture** prensiplerine uygun olarak tasarlanmış ve asenkron mesajlaşma teknolojileri kullanarak korumalı ödeme sistemi implementasyonudur. Kullanıcıların sipariş verdikten sonra 10 dakika boyunca ödemeyi durdurma veya iptal etme hakkına sahip olduğu bir senaryo üzerine kurgulanmıştır.

### 🎯 Temel Özellikler:
- ✅ **Clean Architecture**: Domain-centric, bağımlılıkların içe doğru olduğu katmanlı mimari
- ✅ **Sipariş Yönetimi**: Bekleme, gönderim ve iptal olaylarının yönetimi
- ✅ **10 Dakikalık İptal Penceresi**: Kullanıcı sipariş sonrası ödemeyi durdurabilir veya iptal edebilir
- ✅ **Asenkron İşlem**: Ödeme provizyonda bekletilir, işlemler asenkron olarak gerçekleşir
- ✅ **Üç Farklı Mesajlaşma Teknolojisi**: RabbitMQ, Apache Kafka ve Redis Streams
- ✅ **Outbox Pattern**: Transactional messaging ile tutarlılık garantisi
- ✅ **Inbox Pattern**: İdempotent message processing
- ✅ **Event-Driven Architecture**: Asenkron mesajlaşma ile loose coupling
- ✅ **Ölçeklenebilir Mimari**: Katmanlı ve test edilebilir yapı
- ✅ **Docker Desteği**: Containerize edilmiş dağıtım

---

## 🏗️ Clean Architecture Yapısı

Proje, **Clean Architecture** prensiplerine uygun olarak katmanlara ayrılmıştır:

```
ProtectedPayment/
├── src/
│   ├── domain/                          # Domain Layer (En içteki katman)
│   │   ├── ProtectedPayment.Domain/     # İş kuralları ve domain mantığı
│   │   │   ├── Entities/                # Domain entities (Order, OutboxMessage, InboxMessage, IdempotencyKey)
│   │   │   ├── Events/                  # Event models (OrderPendingEvent, OrderCancelledEvent, vb.)
│   │   │   ├── Repositories/            # Repository interfaces (IOrderRepository, IUnitOfWork, vb.)
│   │   │   └── Errors/                  # Domain errors ve Result types
│   │   └── ProtectedPayment.SharedKernel/  # Paylaşılan domain primitives
│   │
│   ├── application/                     # Application Layer
│   │   ├── ProtectedPayment.Application/           # Use cases ve business logic
│   │   │   ├── Orders/                  # Order service ve use cases
│   │   │   ├── OutboxMessages/          # Outbox pattern implementation
│   │   │   └── InboxMessages/           # Inbox pattern implementation
│   │   └── ProtectedPayment.Application.Contracts/ # DTOs ve Contracts
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

### 🎯 Katman Sorumlulukları

#### 1️⃣ **Domain Layer** (`domain/`)
- **Bağımlılık**: Hiçbir katmana bağımlı değil (tamamen izole)
- **İçerik**: 
  - Domain entities (Order, OutboxMessage, InboxMessage, IdempotencyKey)
  - Event models (OrderPendingEvent, OrderCancelledEvent, OrderConfirmedEvent, OrderShippedEvent)
  - Repository interfaces
  - Domain errors ve business rules
  - SharedKernel (Shared types, primitives)

#### 2️⃣ **Application Layer** (`application/`)
- **Bağımlılık**: Sadece Domain Layer'a bağımlı
- **İçerik**:
  - Use cases ve application services (OrderService)
  - Outbox Pattern implementation (OutboxMessageService)
  - Inbox Pattern implementation (InboxMessageService)
  - DTOs ve Contracts
  - Business orchestration

#### 3️⃣ **Infrastructure Layer** (`infrastructure/`)
- **Bağımlılık**: Domain ve Application katmanlarına bağımlı
- **İçerik**:
  - Entity Framework Core DbContext
  - Repository implementations
  - Messaging implementations (RabbitMQ, Kafka, Redis)
  - Database migrations
  - External service integrations
  - SaveChangesInterceptor (Outbox pattern için)

#### 4️⃣ **Host Layer** (`host/`)
- **Bağımlılık**: Tüm katmanlara bağımlı (composition root)
- **İçerik**:
  - **Order.API**: RESTful API endpoints, controllers
  - **Worker**: Background services (OrderPendingConsumer, OrderCancelledConsumer, vb.)
  - Dependency Injection configuration
  - Middleware pipeline
  - Application startup

---

## 🔄 İş Akışı

### Event-Driven Flow
```
1️⃣ Kullanıcı sipariş oluşturur
   ↓
2️⃣ Ödeme provizyonda bekletilir (Status: Pending)
   ↓
3️⃣ 10 dakikalık süre başlar
   ↓
4️⃣ Kullanıcı iptal ederse → Ödeme iade edilir
   VEYA
   10 dakika dolarsa → Ödeme otomatik onaylanır
   ↓
5️⃣ Sipariş sevkiyat aşamasına geçer
```

### Messaging Patterns

#### 🔸 **Outbox Pattern**
Veritabanı transaction'ı içinde oluşturulan event'ler `OutboxMessage` tablosuna yazılır. Background service bu mesajları okuyup message broker'a gönderir. Bu sayede distributed transaction problemi çözülür.

#### 🔸 **Inbox Pattern**
Gelen mesajlar işlenmeden önce `InboxMessage` tablosuna kaydedilir. Aynı mesaj tekrar gelirse idempotency kontrolü yapılır ve işlem atlanır.

---

## ⚡ Kullanılan Teknolojiler

### Backend Framework
- **.NET 10** - Modern, yüksek performanslı web API ve arka plan servis uygulamaları
- **ASP.NET Core** - Order API
- **.NET Worker Service** - Background services (Event consumers)
- **Entity Framework Core 10** - ORM

### Mesajlaşma Sistemleri
- **RabbitMQ** - TTL + Dead Letter Exchange ile delayed processing
- **Apache Kafka** - Timestamp-based delayed message handling
- **Redis** - Sorted Set ile scheduled job processing

### Veritabanı & ORM
- **PostgreSQL** - İlişkisel veritabanı
- **Entity Framework Core 10** - ORM ve veritabanı yönetimi
- **Entity Framework Core Migrations** - Veritabanı versiyonlama
- **SaveChangesInterceptor** - Outbox pattern için otomatik event kayıt

### Design Patterns & Principles
- **Clean Architecture** - Domain-centric layered architecture
- **Outbox Pattern** - Transactional messaging guarantee
- **Inbox Pattern** - Idempotent message processing
- **Repository Pattern** - Data access abstraction
- **Unit of Work Pattern** - Transaction management
- **Event-Driven Architecture** - Asynchronous messaging between services
- **Dependency Injection** - Inversion of Control

### Diğer Kütüphaneler
- **Confluent.Kafka** (v2.13.0) - Kafka client
- **StackExchange.Redis** (v2.10.1) - Redis client
- **RabbitMQ.Client** (v7.2.0) - RabbitMQ client
- **Newtonsoft.Json** (v13.0.4) - JSON serialization
- **Polly** (v8.6.5) - Dayanıklılık ve hata yönetimi
- **Scalar.AspNetCore** - API dokümantasyonu
- **ETPackages.Result** (v1.0.0) - Result pattern implementasyonu ve hata yönetimi
- **ETPackages.Endpoints** (v2.0.0) - Minimal API endpoint tanımlama altyapısı

### Containerization
- **Docker** - Application containerization
- **Docker Compose** - Multi-container orchestration

---

## 🛠 Kurulum Talimatları

### Gereksinimler
- .NET 10 SDK ([Download](https://dotnet.microsoft.com/download))
- Docker & Docker Compose ([Download](https://www.docker.com/products/docker-desktop))
- Git ([Download](https://git-scm.com))

### 1️⃣ Projeyi Klonlayın

```bash
git clone https://github.com/etartar/ProtectedPayment.git
cd ProtectedPayment
```

### 2️⃣ Bağımlılıkları Yükleyin

```bash
dotnet restore
```

### 3️⃣ Sistemi Docker Üzerinde Başlatın

#### Option A: RabbitMQ kullanarak
```bash
docker-compose -f docker-compose.rabbitmq.yml up -d
```

#### Option B: Kafka kullanarak
```bash
docker-compose -f docker-compose.kafka.yml up -d
```

#### Option C: Redis kullanarak
```bash
docker-compose -f docker-compose.redis.yml up -d
```

#### Option D: Eğer projeyi Visual Studio ortamında debug etmek isterseniz
```bash
docker-compose -f docker-compose.yml up -d
```

docker-compose dosyasını çalıştırdığınızda tüm servisler ayağa kalkacaktır ve VS RabbitMQ Profile, VS Kafka Profile ve VS Redis Profile kısmından istediğiniz ortamda projeyi debug edebileceksiniz.

---

## 🧪 Kullanım Örnekleri

### 1️⃣ Tüm Siparişleri Listeleme
```bash
GET /orders
```

### 2️⃣ Sipariş Oluşturma
```bash
POST /orders
Content-Type: application/json

{
  "productName": "Iphone 16",
  "amount": 120000
}
```

### 3️⃣ Siparişi İptal Etme
```bash
POST /orders/019c0bd7-2368-7c43-a703-c16761405ab3/cancel
Content-Type: application/json
```

---

## 📚 API Dökümantasyonu

Order API başlatıldıktan sonra Scalar UI'ya erişin:

```
http://localhost:5300/scalar/v1
```

---

## 📧 İletişim

Sorular veya öneriler için GitHub Issues açabilirsiniz.

---

## 🙏 Teşekkürler

- Backend Guru eğitim programı
- .NET ve açık kaynak komunite
