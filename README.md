# 📦 Smart Warehouse — Akıllı Depo Yönetim Sistemi

![Smart Warehouse Overview](WarehouseUI/public/vite.svg) (*Proje görsel veya logoları için yer tutucu*)

Bu proje, çok kiracılı (**Multi-Tenant**) mimari ile tasarlanmış tam yığın (Full-Stack) bir **Akıllı Depo Yönetim Sistemi** uygulamasıdır. Ürünleri sisteme tanımlama, depolara giriş/çıkış (stok hareketi) yapma ve anlık istatistikleri takip edebilme özelliklerine sahiptir.

Endüstri standartlarında **Katmanlı Mimari (N-Tier)** kullanılarak .NET 9.0 (C#) ve React 18 (TypeScript) ile sıfırdan geliştirilmiştir.

---

## 🚀 Öne Çıkan Özellikler

*   🔒 **Multi-Tenant Güvenlik:** Aynı veritabanı içerisinde birden fazla şirketin (Company) verisi %100 izole edilerek yönetilir. Bir şirketin, başka bir şirketin verilerine (Okuma/Yazma) erişmesi imkansızdır.
*   🗑️ **Soft Delete Stratejisi:** Hiçbir veri veritabanından fiziksel olarak silinmez. Tüm yapılar `IsDeleted` bayraklarıyla yönetilir, böylece veri geçmişi (Data Integrity) daima korunur.
*   📄 **Server-Side Pagination:** Milyonlarca satır veri bile olsa, API tarafında veritabanından sadece o sayfanın ihtiyacı kadar veri (`Skip` ve `Take` ile) çekilir. Arayüz asla donmaz.
*   🎨 **Modern Kullanıcı Arayüzü:** React ve Material-UI (MUI) kullanılarak "Tek Sayfa (SPA)" ve karanlık tema (Dark Mode) destekli dinamik bir kontrol paneli sunulur.

---

## 🛠️ Kullanılan Teknolojiler

### Backend (.NET Core)
*   **Web API:** .NET 9.0 (C#)
*   **ORM:** Entity Framework Core 9.0 (Code-First)
*   **Veritabanı:** MS SQL Server 2019 (Docker tabanlı)
*   **Mimari:** Controller → Manager → Repository Pattern (Bağımlılık Enjeksiyonu ve DTO yapısı kullanılarak)

### Frontend (React)
*   **Çatı (Framework):** React 18, Vite
*   **Dil:** TypeScript (Tip güvenliği için)
*   **Tasarım (UI):** Material-UI (MUI) v6
*   **Http İstemcisi:** Axios

---

## ⚙️ Kurulum ve Çalıştırma

Projeyi kendi bilgisayarınızda saniyeler içinde ayağa kaldırmak için aşağıdaki adımları izleyebilirsiniz.

### 1. Veritabanının Hazırlanması (SQL Server)
Eğer bilgisayarınızda SQL Server kurulu değilse, **Docker** aracılığıyla geçici bir veritabanı sunucusunu aşağıdaki tek satırlık kodla anında oluşturabilirsiniz:
```bash
docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=Warehouse123! \
  -p 1433:1433 --name sqlserver -d \
  mcr.microsoft.com/mssql/server:2019-latest
```
*(Eğer halihazırda SQL Server kullanıyorsanız bu adımı atlayıp, `appsettings.json` içindeki `DefaultConnection` adımını kendi sunucunuza göre değiştirebilirsiniz).*

### 2. Backend (API) Başlatılması
Gerekli paketlerin indirilmesi, veritabanı tablolarının kurulması ve API'nin ayağa kaldırılması için terminalde proje kök dizininde şu adımları izleyin:

```bash
cd WarehouseAPI/WarehouseAPI
export PATH="$PATH:$HOME/.dotnet/tools"

# Tabloları SQL Server'a otomatik kurar:
dotnet ef database update

# API'yi yayına alır:
dotnet run
```
API başarılı bir şekilde çalıştıktan sonra `http://localhost:5058/swagger` adresi üzerinden API dokümanlarına erişebilirsiniz.

### 3. Frontend (Kullanıcı Arayüzü) Başlatılması
Yeni bir terminal sekmesi açarak React projesini ayağa kaldırabilirsiniz:

```bash
cd WarehouseUI
npm install
npm run dev
```
Uygulamayı tarayıcınızdan **`http://localhost:5173`** adresine giderek hemen kullanmaya başlayabilirsiniz.

---

## 📝 Daha Fazla Detay İçin

Uygulamanın mimari kararları, tasarım seçimleri, yapay zekanın (AI) projedeki yeri ve karşılaşılan çevresel hataların nasıl çözüldüğüne dair teknik tüm okumalar için ana dizinde bulunan **[CALISMA_RAPORU.md](./CALISMA_RAPORU.md)** dosyasını inceleyebilirsiniz.

---
*Bu proje, yetenek değerleme (Developer Case) mülakatı kapsamında tasarlanmış ve geliştirilmiştir.*
