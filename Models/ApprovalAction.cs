
using System.ComponentModel.DataAnnotations;

namespace Traffic.Models
{
    public enum ApprovalAction
    {
        [Display(Name = "Onaya Gönderildi")]
        Submit = 1,

        [Display(Name = "Onaylandı")]
        Approve = 2,

        [Display(Name = "Reddedildi")]
        Reject = 3
    }
}
