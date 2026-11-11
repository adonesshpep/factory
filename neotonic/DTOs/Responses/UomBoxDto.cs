using neotonic.Models;

namespace neotonic.DTOs.Responses
{
    public class UomBoxDto
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public long Number { get; set; }
        public UOMKinds Kind { get; set; }

        public int? MaterialId { get; set; }
        public string? MaterialNumber { get; set; }
        public string? MaterialNameEn { get; set; }
        public string? MaterialNameAr { get; set; }

        public int? ProductId { get; set; }
        public string? ProductNumber { get; set; }
        public string? ProductNameEn { get; set; }
        public string? ProductNameAr { get; set; }

        public decimal Quantity { get; set; }

        public int LocationId { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string LocationNameEn { get; set; } = string.Empty;
        public string LocationNameAr { get; set; } = string.Empty;
        public int? InventoryId { get; set; }
        public string? InventoryCode { get; set; }
        public InventoryZones? Zone { get; set; }

        public int? ShelfId { get; set; }
        public string? ShelfLabel { get; set; }

        public int? ParentUomId { get; set; }
        public int? ActiveCertificateId { get; set; }
        public string? ActiveCertificateNumber { get; set; }
        public AnalysisResults? ActiveCertificateResult { get; set; }
        public string? ActiveCertificateResultNameEn { get; set; }
        public string? ActiveCertificateResultNameAr { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierNumber { get; set; }
        public string? SupplierNameEn { get; set; }
        public string? SupplierNameAr { get; set; }
        public int? MemoRWInId { get; set; }
        public long? MemoRWInNumber { get; set; }
    }
}