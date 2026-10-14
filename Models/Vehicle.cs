
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Traffic.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Plaka zorunludur.")]
        [StringLength(15, MinimumLength = 5,
            ErrorMessage = "Plaka 5-15 karakter arasında olmalıdır.")]
        [Display(Name = "Plaka")]
        public string Plate { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lütfen araç tipi seçiniz.")]
        [Range(1, 4,
            ErrorMessage = "Geçerli bir araç tipi seçiniz.")]
        [Display(Name = "Araç Tipi")]
        public VehicleType? VehicleType { get; set; }

        [Required(ErrorMessage = "Marka zorunludur.")]
        [StringLength(50,
            ErrorMessage = "Marka en fazla 50 karakter olabilir.")]
        [Display(Name = "Marka")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model zorunludur.")]
        [StringLength(50,
            ErrorMessage = "Model en fazla 50 karakter olabilir.")]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;

        // Aynı araç kaydının eş zamanlı güncellenmesini kontrol eder.
        [Timestamp]
        [ValidateNever]
        public byte[] RowVersion { get; set; } = null!;
    }
}
