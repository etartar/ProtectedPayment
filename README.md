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

**ProtectedPayment**, e-ticaret platformları için geliştirilmiş, asenkron mesajlaşma teknolojileri kullanarak korumalı ödeme sistemi implementasyonudur. Kullanıcıların sipariş verdikten sonra 10 dakika boyunca ödemeyi durdurma veya iptal etme hakkına sahip olduğu bir senaryo üzerine kurgulanmıştır.

### 🎯 Temel Özellikler:
- ✅ **Sipariş Yönetimi**: Bekleme, gönderim ve iptal olaylarının yönetimi
- ✅ **10 Dakikalık İptal Penceresi**: Kullanıcı sipariş sonrası ödemeyi durdurabilir veya iptal edebilir
- ✅ **Asenkron İşlem**: Ödeme provizyonda bekletilir, işlemler asenkron olarak gerçekleşir
- ✅ **Üç Farklı Mesajlaşma Teknolojisi**: RabbitMQ, Apache Kafka ve Redis Streams
- ✅ **Ölçeklenebilir Mimari**: Mikro hizmetler yapısı
- ✅ **Event Sourcing**: Tüm ödeme olayları kayıt altında
- ✅ **Docker Desteği**: Containerize edilmiş dağıtım

### 🔄 İş Akışı
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

---

## ⚡ Kullanılan Teknolojiler

### Backend Framework
- **.NET 10** - Modern, high-performance web api ve background service uygulamaları
- **ASP.NET Core** - Order API
- **.NET Worker Service** - Background services (Event consumers)
- **Entity Framework Core 10** - ORM

### Mesajlaşma Sistemleri
- **RabbitMQ** - TTL + Dead Letter Exchange ile delayed processing
- **Apache Kafka** - Timestamp-based delayed message handling
- **Redis** - Sorted Set ile scheduled job processing

### Veritabanı & ORM
- **PostgreSQL** - İlişkisel veritabanı
- **Entity Framework Core 10** - ORM ve database management
- **Entity Framework Core Migrations** - Veritabanı versionlama

### Diğer Kütüphaneler
- **Confluent.Kafka** (v2.13.0) - Kafka client
- **StackExchange.Redis** (v2.10.1) - Redis client
- **RabbitMQ.Client** (v7.2.0) - RabbitMQ client
- **Newtonsoft.Json** (v13.0.4) - JSON serialization
- **Polly** (v8.6.5) - Dayanıklılık ve hata yönetimi
- **Scalar.AspNetCore** - API dokümantasyonu

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
