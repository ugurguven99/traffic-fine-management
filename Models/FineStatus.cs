
using System.ComponentModel.DataAnnotations;

namespace Traffic.Models
{
    public enum FineStatus
    {
        [Display(Name = "Yeni")]
        New = 0,

        [Display(Name = "Yönetici Onayı")]
        ManagerApproval = 1,

        [Display(Name = "Finans Onayı")]
        FinanceApproval = 2,

        [Display(Name = "Tamamlandı")]
        Completed = 3,

        [Display(Name = "Reddedildi")]
        Rejected = 4
    }
}
