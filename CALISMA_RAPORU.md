# ÇALIŞMA RAPORU — Akıllı Depo Yönetim Sistemi

Bu proje, çok kiracılı (multi-tenant) bir **Akıllı Depo Yönetim Sistemi** geliştirme case çalışmasıdır. Ürün yönetimi, depo yönetimi ve stok hareketi (giriş/çıkış) işlemlerini kapsayan tam yığın (full-stack) bir web uygulaması geliştirilmiştir.

---

## 1. Kullanılan Teknolojiler

**Backend**
* **.NET 9.0:** Web API framework
* **ASP.NET Core 9.0:** HTTP pipeline ve routing
* **Entity Framework Core 9.0.3:** ORM — veritabanı erişimi
* **MS SQL Server:** Veritabanı
* **Swashbuckle:** Swagger / OpenAPI dokümantasyonu

**Frontend**
* **React 18:** UI framework
* **TypeScript 5.x:** Tip güvenli JavaScript
* **Vite 7.x:** Build tool ve geliştirme sunucusu
* **Material-UI (MUI) 6.x:** UI bileşen kütüphanesi
* **Axios 1.x:** HTTP istemci

---

## 2. Mimari Kararlar

**Katmanlı Mimari Dizaynı**
```text
Controller → Manager → Repository → DbContext → SQL Server
```
* **Controller:** HTTP endpoint tanımları, request/response dönüşümleri.
* **Manager:** İş mantığı, validasyon (stok yeterliliği, CompanyId kontrolü).
* **Repository:** EF Core ile veritabanı erişimi, sorgulama.
* **Entities:** Domain modelleri.

Bu mimari ile her katman tek bir sorumluluğa sahip olmuş (Single Responsibility Principle) ve bağımlılıklar arayüzler (Interface) kullanılarak yönetilmiştir (Dependency Inversion Principle).

**Multi-Tenant Tasarım**
Her entity'de `CompanyId` alanı bulunmaktadır. Tüm repository sorgularında bu alan ile filtreleme yapılmaktadır. İstemciden CompanyId sağlanmadığında `400 BadRequest`, şirket bazlı uyumsuzluk olduğunda ise `403 Forbidden` yanıtı döndürülmektedir.

**HTTP Metod Kısıtlaması**
Case dokümanındaki kurallar gereği RESTful standartları bir miktar sıkılaştırılarak:
* **GET:** Listeleme ve tekil kayıt okuma
* **POST:** Oluşturma, güncelleme ve silme işlemleri için kullanılmıştır.
* **PUT ve DELETE:** Metodları üretim ortamı kısıtlamaları baz alınarak projede hiç kullanılmamıştır.

**Soft Delete Stratejisi**
Kayıtlar veritabanından fiziksel olarak silinmez. Tüm domain nesnelerinde `IsDeleted = true` bayrağı ile işaretlenir. Sistemdeki tüm okuma sorgularında `WHERE IsDeleted = false` filtresi (Query Filter) istisnasız uygulanır.

**DTO (Data Transfer Object) Katmanı**
Veritabanı varlıkları (Entity) doğrudan dışarı açılmaz. Her operasyon için spesifik DTO sınıfları oluşturulmuştur:
* Parametre yönetimi için: `CreateProductDto`, `UpdateProductDto`, `DeleteProductDto`
* Sayfalama işlemleri için: `ListProductsDto`
* Yanıt formatları için: `ProductResponseDto`

**Server-Side Pagination**
Tüm listeleme endpoint'leri `page` ve `pageSize` parametrelerini kabul eder. Veritabanı seviyesinde `Skip().Take()` uygulanarak bellek performansı korunur. İstemci tarafına sonuçlar `totalCount` ve `totalPages` metadataları ile birlikte iletilir.

---

## 3. Veritabanı Tasarımı

**Tablolar**
* `Products`: Id, ProductName, SKU, Category, Unit, Description, MinStockLevel, CompanyId, IsDeleted.
* `Warehouses`: Id, Name, Location, Capacity, Description, CompanyId, IsDeleted.
* `StockTransactions`: Id, ProductId, WarehouseId, TransactionType, Quantity, Note, CompanyId, IsDeleted.

**İlişki Modeli**
* `StockTransactions.ProductId` -> `Products.Id` (Foreign Key)
* `StockTransactions.WarehouseId` -> `Warehouses.Id` (Foreign Key)

> **Not:** İlişkilerde `ON DELETE NO ACTION` kuralı uygulanmıştır. Proje tamamen Soft Delete üzerine inşa edildiği için fiziksel silinmeler sonucu kaskad veri kaybının veya ilişkisel bütünlük hatalarının önüne geçilmiştir.

---

## 4. API Endpoint'leri

**Ürünler (Products)**
* `GET   /api/product/by-company/{companyId}`
* `GET   /api/product/{id}`
* `POST  /api/product/create`
* `POST  /api/product/update`
* `POST  /api/product/delete`

**Depolar (Warehouses)**
* `GET   /api/warehouse/by-company/{companyId}`
* `POST  /api/warehouse/create`
* `POST  /api/warehouse/update`
* `POST  /api/warehouse/delete`

**Stok Hareketleri (Stock Transactions)**
* `GET   /api/stock-transaction/by-company/{companyId}`
* `POST  /api/stock-transaction/create`

**Kontrol Paneli (Dashboard)**
* `GET   /api/dashboard/summary/{companyId}`

---

## 5. Frontend Özellikleri ve UI/UX Kararları

Uygulamanın görsel yüzü tek sayfa (SPA) üzerinde çalışacak şekilde, React ve Material-UI bileşenleri ile kurgulanmıştır.

* **Modüler Yapı:** Ana sayfa üç ana sekmeye (Ürünler, Depolar, Stok Hareketleri) ayrılmıştır.
* **Kontrol Paneli:** İşletmenin anlık özetini sunan, toplam kayıt, düşük stok uyarısı ve hareket grafiklerini barındıran Dashboard istatistik kartları eklenmiştir.
* **Server-Side Pagination:** Tüm tablolarda entegrasyonu tamamlanmıştır; ağ trafigini yormadan sadece talep edilen sayfa verisi API'den getirilir.
* **Gerçek Zamanlı Filtreler:** SKU, Ürün Adı, Kategori gibi başlıklarda anlık backend filtrelenmesi ve aranması kurgulanmıştır.
* **Veri Güvenliği ve UX:** Tehlikeli işlemler (Silme) her zaman uyarı diyalogları ile (MUI Dialog) güvence altına alınmıştır.
* **Görsel Zenginlik:** Kurumsal ancak donuk olmayan, "Dark Modern" temalı özel bir renk paleti CSS aracılığıyla projeye yedirilmiştir. Kritik iş kuralları (Örn: Minimum Stok Seviyesinin altına düşen ürünler) spesifik renklerle işaretlenmiştir.

---

## 6. Karşılaşılan Zorluklar ve Çözümler

**Zorluk 1:** macOS ortamında .NET paket yönetiminde "SSL Sertifika Doğrulama" engeline takılması.
**Çözüm:** Nuget yöneticisi için geçici ortamlarda sertifika kısıtlamasını atlamak üzere `TMPDIR=/tmp` argümanı çalışma oturumlarına tanımlandı.

**Zorluk 2:** ARM tabanlı (Apple Silicon) işlemcilerde MSSQL çalıştırılırken TCP/SSL Handshake hataları alınması.
**Çözüm:** Bağlantı protokollerindeki şifreleme uyuşmazlığını aşmak amacıyla SQL Server bağlantı dizgisine `Encrypt=False` zorunluluğu eklenerek güvenli el sıkışma problemi bypass edildi.

**Zorluk 3:** MUI v5 ile v6 geçişlerindeki API farklılıkları.
**Çözüm:** Geliştirme esnasında güncel stabil sürüm olan Material-UI v6 mimarisine uyum sağlanmış ve Flexbox yaklaşımını baz alan `Stack` ve `Box` entegrasyonuna geçiş yapılmıştır.

---

## 7. AI Kullanım Raporu

Proje kodlaması esnasında yapay zeka asistanı aktif bir çift-programlama (pair-programming) aracı olarak değerlendirilmiştir.

* **Destek Alınan Noktalar:** Katmanlı mimari için tekrarlayan iş yapıları (Boilerplate entity, DTO, Repository sınıfları iskeletlerinin oluşturulması), paket kurumu hatalarının araştırılması ve Docker spesifik işletim sistemi altyapı hata teşhisleridir.
* **Geliştirici İnsiyatifi:** Geliştirme alanındaki tüm mimari kararlar (Multi-tenant stratejisi, EF Core EntityState kuralları kurgusu, API endpoint metot kısıtlamaları, Soft-delete yaklaşımları ve Frontend bileşen dağılım yapısı) tamamen geliştiricinin analizinden geçmiş ve ilgili kurallar katı bir şekilde projelendirilmiştir.

> **Önemli Not:** Üretilen tüm mantıksal yapı, algoritma ve iş kuralı uygulamaları doğrudan geliştirici insiyatifi ile projeye entegre edilmiş olup, projede yardımcı araçlar bir karar mercii olarak değil, daktilo vazifesinde kullanılmıştır. Sorumluluk ve kararlar geliştiriciye aittir.

---

## 8. Projeyi Çalıştırma Yönergeleri

**Gereksinimler:**
* .NET 9.0 SDK
* Node.js 18+
* Docker Desktop veya kurulu MS SQL Server (Veritabanı işlemleri için)

**Adım 1: Veritabanı Hazırlığı (Backend)**

*Seçenek A: Bilgisayarınızda MS SQL Server yüklüyse (Önerilen)*
Ekstra bir servise ihtiyaç yoktur. `WarehouseAPI/WarehouseAPI/appsettings.json` dosyasındaki `DefaultConnection` kısmını kendi sunucu, kullanıcı adı ve şifre verilerinizle değiştirmeniz yeterlidir.

*Seçenek B: Bilgisayarınızda SQL Server yoksa (Docker Kurulumu)*
Bilgisayarında SQL Server yüklü olmayan değerlendiriciler, terminal / komut satırında aşağıdaki tekil komut ile projeye tam uyumlu bir veritabanı yansımasını ayağa kaldırabilirler:
```bash
docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=Warehouse123! \
  -p 1433:1433 --name sqlserver -d \
  mcr.microsoft.com/mssql/server:2019-latest
```

**Adım 2: API Servisini Başlatma (Backend)**
Veritabanı adreslemesi yapıldıktan sonra sırasıyla; veritabanı tablolarının kurulması ve API'ın ayağa kaldırılması işlemi uygulanır. İşlemler için kök klasörü olarak `WarehouseAPI/WarehouseAPI` yolunu baz alınız.
```bash
cd WarehouseAPI/WarehouseAPI
export PATH="$PATH:$HOME/.dotnet/tools"

# 1) Entity Framework migration ile veritabanı ve tabloları sıfırdan kurar:
TMPDIR=/tmp dotnet ef database update

# 2) .NET projesini yayına alır:
TMPDIR=/tmp dotnet run
```
*(Backend çalıştığında konsolda gösterilen `http://localhost:5058/swagger` adresi üzerinden projenin canlı dokümanı incelenebilir)*

**Adım 3: İstemci Arayüzünü Başlatma (Frontend)**
Terminalde yeni bir sekme açılarak React SPA istemcisi barındırılacaktır.
```bash
cd WarehouseUI
npm install
npm run dev
```

Uygulamamız **http://localhost:5173** adresi üzerinden başarıyla çalışmaya başlayacaktır.
