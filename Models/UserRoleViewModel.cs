
using System.ComponentModel.DataAnnotations;

namespace Traffic.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string CurrentRole { get; set; } = "Rol Yok";

        [Required(ErrorMessage = "Lütfen bir rol seçiniz.")]
        public string SelectedRole { get; set; } = string.Empty;

        public bool IsAdmin { get; set; }
    }
}
