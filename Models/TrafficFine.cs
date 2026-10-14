
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Traffic.Models
{
    public class TrafficFine
    {
        public int Id { get; set; }

        // Ceza hangi araca ait?
        [Required(ErrorMessage = "Araç seçimi zorunludur.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Lütfen bir araç seçiniz.")]
        [Display(Name = "Araç")]
        public int VehicleId { get; set; }

        // Araç ile ilişkili navigation property
        [ForeignKey(nameof(VehicleId))]
        [ValidateNever]
        public Vehicle Vehicle { get; set; } = null!;

        // Ceza tutarı
        [Required(ErrorMessage = "Ceza tutarı zorunludur.")]
        [Range(typeof(decimal), "0.01", "999999999.99",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "Ceza tutarı 0'dan büyük olmalıdır.")]
        [Display(Name = "Ceza Tutarı")]
        public decimal FineAmount { get; set; }

        // Ceza tarihi
        [Required(ErrorMessage = "Ceza tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ceza Tarihi")]
        public DateTime FineDate { get; set; }

        // Ceza açıklaması
        [StringLength(500,
            ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        // Trafik cezasının mevcut onay durumu
        [Display(Name = "Durum")]
        public FineStatus Status { get; set; } = FineStatus.New;

        // Kaydın oluşturulduğu tarih (UTC)
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Trafik cezasının onay/ret işlem geçmişi
        public ICollection<ApprovalHistory> ApprovalHistories
        { get; set; } = new List<ApprovalHistory>();

        // Eş zamanlı güncelleme kontrolü

        [Timestamp]
        [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
        public byte[] RowVersion { get; set; } = null!;

    }
}
