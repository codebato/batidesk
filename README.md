# NvnDesk

**AI-Powered Multi-Tenant Helpdesk / Support Ticket SaaS**

🔗 Live demo: [Frontend](https://batidesk-frontend.onrender.com) · [API](https://nvndesk.onrender.com)

> ⚠️ Hosted on Render's free tier — the first request after a period of inactivity may take up to ~50 seconds to wake the service up.

**[English](#english) | [Türkçe](#türkçe)**

---

## English

### Overview

NvnDesk is a full-stack, multi-tenant customer support ticketing platform. Each company (tenant) gets an isolated workspace where team members can create, track, and resolve support tickets — with AI automatically categorizing, prioritizing, and summarizing every ticket as it comes in.

### Features

- **Multi-tenant authentication** — JWT-based auth with full tenant data isolation
- **Ticket lifecycle management** — create tickets and move them through `Open → In Progress → Resolved → Closed`
- **AI-assisted triage** — Google Gemini automatically predicts category/priority and generates a short summary for every new ticket
- **Real-time updates** — new tickets appear instantly for all connected users via SignalR, no page refresh needed
- **Background job processing** — email notifications are queued and processed asynchronously with Hangfire
- **Caching layer** — ticket lists are cached in Redis for performance, with cache invalidation on every write to keep data fresh
- **Subscription billing** — Stripe integration for tenant subscription plans

### Tech Stack

**Backend**
- ASP.NET Core Web API (.NET 10) — Clean Architecture (Domain / Application / Infrastructure / API layers)
- Entity Framework Core + PostgreSQL ([Neon](https://neon.tech), serverless Postgres)
- Redis ([Upstash](https://upstash.com)) — caching
- Hangfire — background jobs (email delivery)
- SignalR — real-time notifications
- Google Gemini API — AI ticket categorization & summarization
- Stripe — subscription billing
- JWT — authentication

**Frontend**
- React + TypeScript + Vite
- React Router
- Axios
- SignalR client

**Infrastructure**
- Docker
- Deployed on [Render](https://render.com) (Web Services)
- PostgreSQL on Neon, Redis on Upstash

### Architecture

The backend follows Clean Architecture principles:

```
src/
├── NvnDesk.Domain          # Entities, enums — no external dependencies
├── NvnDesk.Application     # DTOs, interfaces, business rules
├── NvnDesk.Infrastructure  # EF Core, Redis, Hangfire, SignalR, external services
└── NvnDesk.API             # Controllers, DI wiring, Program.cs
```

### Key Engineering Decisions

- **Cache invalidation strategy**: rather than trying to update the cached ticket list in place on every write (error-prone), the cache key is simply deleted on `Create`/`Update`. The next read transparently repopulates it from the database — trading a slightly slower next read for correctness.
- **AI calls are non-blocking for ticket creation**: if the AI service fails or times out, the ticket is still created successfully; category/summary are simply left empty rather than failing the whole request.

---

## Türkçe

### Genel Bakış

NvnDesk, çok kiracılı (multi-tenant) bir müşteri destek/ticket yönetim platformudur. Her şirket (tenant) kendi izole çalışma alanına sahip olur; ekip üyeleri destek talepleri (ticket) oluşturabilir, takip edebilir ve çözümleyebilir. Yeni bir ticket oluşturulduğunda yapay zeka otomatik olarak kategori/öncelik tahmini yapar ve kısa bir özet üretir.

### Özellikler

- **Çok kiracılı kimlik doğrulama** — JWT tabanlı, tam veri izolasyonu ile
- **Ticket yaşam döngüsü yönetimi** — `Open → InProgress → Resolved → Closed` akışı
- **Yapay zeka destekli triyaj** — Google Gemini her yeni ticket için otomatik kategori/öncelik tahmini yapar ve özet çıkarır
- **Gerçek zamanlı güncellemeler** — yeni ticket'lar SignalR sayesinde sayfa yenilenmeden anlık olarak tüm bağlı kullanıcılarda görünür
- **Arka plan iş kuyruğu** — email bildirimleri Hangfire ile asenkron olarak işlenir
- **Önbellekleme (cache)** — ticket listeleri performans için Redis'te tutulur; her yazma işleminde cache geçersiz kılınarak veri tazeliği garanti edilir
- **Abonelik faturalandırma** — tenant abonelik planları için Stripe entegrasyonu

### Teknoloji Yığını

**Backend**
- ASP.NET Core Web API (.NET 10) — Clean Architecture (Domain / Application / Infrastructure / API katmanları)
- Entity Framework Core + PostgreSQL ([Neon](https://neon.tech), serverless Postgres)
- Redis ([Upstash](https://upstash.com)) — önbellekleme
- Hangfire — arka plan işleri (email gönderimi)
- SignalR — gerçek zamanlı bildirimler
- Google Gemini API — AI ticket kategorilendirme & özetleme
- Stripe — abonelik faturalandırma
- JWT — kimlik doğrulama

**Frontend**
- React + TypeScript + Vite
- React Router
- Axios
- SignalR client

**Altyapı**
- Docker
- [Render](https://render.com) üzerinde deploy (Web Services)
- Neon üzerinde PostgreSQL, Upstash üzerinde Redis

### Mimari

Backend, Clean Architecture prensiplerini takip eder:

```
src/
├── NvnDesk.Domain          # Entity'ler, enum'lar — dış bağımlılık yok
├── NvnDesk.Application     # DTO'lar, interface'ler, iş kuralları
├── NvnDesk.Infrastructure  # EF Core, Redis, Hangfire, SignalR, dış servisler
└── NvnDesk.API             # Controller'lar, DI kurulumu, Program.cs
```

### Öne Çıkan Mühendislik Kararları

- **Cache invalidation stratejisi**: her yazma işleminde önbellekteki listeyi yerinde güncellemeye çalışmak yerine (hataya açık), `Create`/`Update` işlemlerinde ilgili cache key'i doğrudan siliniyor. Bir sonraki okuma isteği veritabanından taze veriyi çekip cache'i otomatik olarak yeniden dolduruyor — doğruluk için, bir sonraki okumanın biraz daha yavaş olmasını göze alıyoruz.
- **AI çağrıları ticket oluşturmayı bloklamaz**: AI servisi başarısız olur veya zaman aşımına uğrarsa, ticket yine de başarıyla oluşturulur; kategori/özet alanları boş bırakılır, tüm istek başarısız sayılmaz.
