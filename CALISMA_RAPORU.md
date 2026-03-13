# ÇALIŞMA RAPORU — Akıllı Depo Yönetim Sistemi

## 1. Proje Özeti

Bu proje, çok kiracılı (multi-tenant) bir **Akıllı Depo Yönetim Sistemi** geliştirme case çalışmasıdır. Ürün yönetimi, depo yönetimi ve stok hareketi (giriş/çıkış) işlemlerini kapsayan tam yığın (full-stack) bir web uygulaması geliştirilmiştir.

---

## 2. Kullanılan Teknolojiler

### Backend
| Teknoloji | Sürüm | Açıklama |
|-----------|-------|----------|
| .NET | 9.0 | Web API framework |
| ASP.NET Core | 9.0 | HTTP pipeline ve routing |
| Entity Framework Core | 9.0.3 | ORM — veritabanı erişimi |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.3 | SQL Server provider |
| Swashbuckle.AspNetCore | 6.9.0 | Swagger/OpenAPI dokümantasyonu |
| MS SQL Server | 2019 (Docker) | Veritabanı |

### Frontend
| Teknoloji | Sürüm | Açıklama |
|-----------|-------|----------|
| React | 18 | UI framework |
| TypeScript | 5.x | Tip güvenli JavaScript |
| Vite | 7.x | Build tool ve geliştirme sunucusu |
| Material-UI (MUI) | 6.x | UI bileşen kütüphanesi |
| Axios | 1.x | HTTP istemci |

### Altyapı
| Teknoloji | Açıklama |
|-----------|----------|
| Docker | SQL Server container çalıştırmak için |

---

## 3. Mimari Kararlar

### 3.1 Katmanlı Mimari

```
Controller → Manager → Repository → DbContext → SQL Server
```

- **Controller**: HTTP endpoint tanımları, request/response dönüşümleri
- **Manager**: İş mantığı, validasyon (stok yeterliliği, CompanyId kontrolü)
- **Repository**: EF Core ile veritabanı erişimi, sorgulama
- **Entities**: Domain modelleri

Bu mimari sayede her katman tek bir sorumluluğa sahip olmuş (Single Responsibility Principle) ve bağımlılıklar interface üzerinden yönetilmiştir (Dependency Inversion Principle).

### 3.2 Multi-Tenant Tasarım

Her entity'de `CompanyId` alanı bulunmaktadır. Tüm repository sorgularında bu alan ile filtreleme yapılmaktadır. CompanyId sağlanmadığında `400 BadRequest`, uyuşmadığında `403 Forbidden` döndürülmektedir.

### 3.3 HTTP Metod Kısıtlaması

Case'in kuralına uygun olarak:
- **GET**: Listeleme ve tekil kayıt okuma
- **POST**: Oluşturma, güncelleme ve silme işlemleri

PUT ve DELETE metodları hiç kullanılmamıştır.

### 3.4 Soft Delete

Kayıtlar fiziksel olarak silinmez. `IsDeleted = true` ile işaretlenir. Tüm sorgularda `WHERE IsDeleted = false` filtresi uygulanır.

### 3.5 DTO Katmanı

Entity'ler doğrudan API'ye expose edilmez. Her işlem için ayrı DTO sınıfları tanımlanmıştır:
- `CreateProductDto`, `UpdateProductDto`, `DeleteProductDto`
- `ListProductsDto` (sayfalama ve filtreleme parametreleri)
- `ProductResponseDto` (mevcut stok dahil)

### 3.6 Server-Side Pagination

Tüm liste endpoint'leri `page` ve `pageSize` parametrelerini alır. Veritabanında `Skip().Take()` ile sunucu tarafında sayfalama uygulanır. Frontend için `totalCount`, `totalPages` bilgileri her yanıtta döndürülür.

---

## 4. Veritabanı Tasarımı

### Tablolar

```sql
Products (Id, ProductName, SKU, Category, Unit, Description, MinStockLevel,
          CompanyId, IsDeleted, CreatedAt, UpdatedAt)

Warehouses (Id, Name, Location, Capacity, Description,
            CompanyId, IsDeleted, CreatedAt, UpdatedAt)

StockTransactions (Id, ProductId, WarehouseId, TransactionType, Quantity, Note,
                   CompanyId, IsDeleted, CreatedAt, UpdatedAt)
```

### İlişkiler
- `StockTransactions.ProductId` → `Products.Id` (FK, NO ACTION)
- `StockTransactions.WarehouseId` → `Warehouses.Id` (FK, NO ACTION)

İlişkilerde `ON DELETE NO ACTION` tercih edilmiştir — ürün veya depo silindiğinde bağlı hareketlerin etkilenmemesi için.

### BaseEntity

Tüm entity'ler ortak `BaseEntity` sınıfından türemektedir:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public string CompanyId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

---

## 5. API Endpoint'leri

### Ürünler
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/product/by-company/{companyId}` | Sayfalı ürün listesi |
| GET | `/api/product/{id}` | Tekil ürün |
| POST | `/api/product/create` | Yeni ürün oluştur |
| POST | `/api/product/update` | Ürün güncelle |
| POST | `/api/product/delete` | Ürün soft-delete |

### Depolar
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/warehouse/by-company/{companyId}` | Sayfalı depo listesi |
| POST | `/api/warehouse/create` | Yeni depo oluştur |
| POST | `/api/warehouse/update` | Depo güncelle |
| POST | `/api/warehouse/delete` | Depo soft-delete |

### Stok Hareketleri
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/stock-transaction/by-company/{companyId}` | Sayfalı hareket listesi |
| POST | `/api/stock-transaction/create` | Giriş/çıkış hareketi |

### Dashboard
| Method | Endpoint | Açıklama |
|--------|----------|----------|
| GET | `/api/dashboard/summary/{companyId}` | Özet istatistikler |

---

## 6. Frontend Özellikleri

- **Tek sayfa uygulaması**: React + MUI Tabs ile 3 sekme (Ürünler, Depolar, Stok Hareketleri)
- **Dashboard kartları**: Toplam ürün, depo, günlük hareket, düşük stok uyarısı, toplam giriş/çıkış
- **Server-side pagination**: Tüm tablolarda sayfa değişikliğinde backend'e istek atılır
- **Arama ve filtreleme**: Ürün adı/SKU arama, kategori filtresi, stok hareketi tür filtresi
- **CRUD işlemleri**: MUI Dialog modalları ile ekleme ve düzenleme
- **Silme onayı**: Tehlikeli işlem öncesi onay diyalogu
- **Hata yönetimi**: API hatalarında kullanıcıya anlaşılır mesaj gösterimi
- **Düşük stok göstergesi**: MinStockLevel altına düşen ürünler kırmızı ile işaretlenir
- **Dark mode**: Tam karanlık tema, modern gradient tasarım

---

## 7. Karşılaşılan Zorluklar ve Çözümler

### Zorluk 1: macOS'ta NuGet SSL Sertifika Hatası
**Sorun**: `dotnet add package` komutları SSL sertifika doğrulama hatası veriyordu.  
**Çözüm**: `TMPDIR=/tmp` ortam değişkeni ile geçici dizin sorunu çözüldü.

### Zorluk 2: Docker SQL Server ARM64/AMD64 Platform Uyumsuzluğu
**Sorun**: Apple Silicon (M-serisi) Mac üzerinde SQL Server 2022 görüntüsü SSL handshake hatası verdi.  
**Çözüm**: SQL Server 2019 görüntüsü kullanıldı, bağlantı dizgisine `Encrypt=False` eklendi.

### Zorluk 3: MUI v6 Grid API Değişikliği
**Sorun**: MUI v5'teki `<Grid item xs={6}>` sözdizimi v6'da kaldırıldı.  
**Çözüm**: `Grid item` yerine `Stack` ve `Box` bileşenleri ile düzen oluşturuldu.

### Zorluk 4: dotnet-ef PATH Sorunu
**Sorun**: `dotnet ef` komutu kurulumdan sonra bulunamıyordu.  
**Çözüm**: `export PATH="$PATH:/Users/rabia/.dotnet/tools"` ile oturuma eklendi.

---

## 8. AI Kullanımı

Bu proje, **Antigravity (Google DeepMind)** yapay zeka asistanı ile pair programming yöntemiyle geliştirilmiştir.

### AI'ın Katkıda Bulunduğu Alanlar
- Entity, DTO ve Controller sınıflarının iskelet kodunun oluşturulması
- EF Core model konfigürasyonları (Fluent API)
- Repository ve Manager katmanı implementasyonları
- React bileşenlerinin (ProductsTab, WarehousesTab, TransactionsTab vb.) geliştirilmesi
- Ortam sorunlarının (SSL, Docker, PATH) teşhis ve çözümü

### Geliştirici Katkıları
- Tüm mimari kararların alınması ve onaylanması
- Endpoint tasarımı ve iş kurallarının belirlenmesi
- Test senaryolarının çalıştırılması ve doğrulanması
- Geliştirme ortamının kurulumu ve yönetimi
- Hata durumlarında debugging

> **Not**: Üretilen her kod satırı geliştiricinin gözetiminde incelenmiş ve onaylanmıştır. AI, bir geliştirici aracı olarak kullanılmış; kararlar ve sorumluluk geliştiriciye aittir.

---

## 9. Proje Yapısı

```
developer-case/
├── WarehouseAPI/
│   └── WarehouseAPI/
│       ├── Controllers/
│       │   ├── ProductController.cs
│       │   ├── WarehouseController.cs
│       │   ├── StockTransactionController.cs
│       │   └── DashboardController.cs
│       ├── Data/
│       │   └── AppDbContext.cs
│       ├── DTOs/
│       │   ├── Product/ProductDtos.cs
│       │   ├── Warehouse/WarehouseDtos.cs
│       │   ├── StockTransaction/StockTransactionDtos.cs
│       │   └── Dashboard/DashboardDtos.cs
│       ├── Entities/
│       │   ├── BaseEntity.cs
│       │   ├── Product.cs
│       │   ├── Warehouse.cs
│       │   └── StockTransaction.cs
│       ├── Managers/
│       │   ├── Interfaces/
│       │   └── (Implementations)
│       ├── Migrations/
│       ├── Repositories/
│       │   ├── Interfaces/
│       │   └── (Implementations)
│       ├── Program.cs
│       └── appsettings.json
└── WarehouseUI/
    └── src/
        ├── api/
        │   ├── apiClient.ts
        │   ├── services.ts
        │   └── types.ts
        ├── components/
        │   ├── SummaryCards.tsx
        │   ├── ProductsTab.tsx
        │   ├── ProductModal.tsx
        │   ├── WarehousesTab.tsx
        │   ├── WarehouseModal.tsx
        │   ├── TransactionsTab.tsx
        │   └── ConfirmDialog.tsx
        └── App.tsx
```

---

## 10. Projeyi Çalıştırma

### Gereksinimler
- .NET 9.0 SDK
- Node.js 18+
- Docker Desktop

### Backend
```bash
# SQL Server başlat
docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=Warehouse123! \
  -p 1433:1433 --name sqlserver -d \
  mcr.microsoft.com/mssql/server:2019-latest

# Migration uygula
cd WarehouseAPI/WarehouseAPI
export PATH="$PATH:$HOME/.dotnet/tools"
TMPDIR=/tmp dotnet ef database update

# API'yi başlat
TMPDIR=/tmp dotnet run
```

### Frontend
```bash
cd WarehouseUI
npm install
npm run dev
```

Uygulama: **http://localhost:5173**  
API: **http://localhost:5058**  
Swagger: **http://localhost:5058/swagger**
