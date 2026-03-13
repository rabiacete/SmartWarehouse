namespace WarehouseAPI.Entities;

// ============================================================
// BaseEntity — Tüm entity'lerin türediği temel sınıf
// ============================================================
// [KURAL-MT] Multi-Tenant: CompanyId her entity'de zorunlu
//            → Farklı şirketlerin verileri aynı tabloda tutulur
//            → Tüm sorgularda WHERE CompanyId = X filtresi uygulanır
//
// [KURAL-2]  Soft Delete: IsDeleted alanı fiziksel silme yerine
//            mantıksal silme sağlar
//            → IsDeleted=true → kayıt "silinmiş" kabul edilir
//            → Sorgularda WHERE IsDeleted=false ile filtrelenir
//            → Veri kaybı yaşanmaz, audit trail korunur
// ============================================================
public abstract class BaseEntity
{
    public int Id { get; set; }

    // [KURAL-MT] Multi-tenant zorunlu alan — her sorgu bu değere göre filtrelenir
    public string CompanyId { get; set; } = string.Empty;

    // [KURAL-2] Soft delete — true ise kayıt silinmiş sayılır, fiziksel olarak silinmez
    public bool IsDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
