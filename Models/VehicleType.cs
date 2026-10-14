
using System.ComponentModel.DataAnnotations;

namespace Traffic.Models
{
    public enum VehicleType
    {
        [Display(Name = "Binek")]
        PassengerCar = 1,

        [Display(Name = "Çekici")]
        Tractor = 2,

        [Display(Name = "Dorse")]
        Trailer = 3,

        [Display(Name = "Kiralık Araç")]
        RentalCar = 4
    }
}
