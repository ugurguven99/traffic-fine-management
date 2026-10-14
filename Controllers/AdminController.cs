
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Traffic.Models;

namespace Traffic.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(
            UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: Admin
        // Kullanıcıları ve rollerini listeler.
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .AsNoTracking()
                .OrderBy(u => u.Email)
                .ToListAsync();

            var model = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager
                    .GetRolesAsync(user);

                model.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    CurrentRole = roles.Count > 0
                        ? string.Join(", ", roles)
                        : "Rol Yok",
                    IsAdmin = roles.Contains("Admin")
                });
            }

            return View(model);
        }

        // POST: Admin/AssignRole
        // Kullanıcıya Manager veya Finance rolü atar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(
            string userId,
            string selectedRole)
        {
            // Sadece izin verilen roller atanabilir.
            if (selectedRole != "Manager" &&
                selectedRole != "Finance")
            {
                TempData["ErrorMessage"] =
                    "Geçersiz rol seçimi.";

                return RedirectToAction(nameof(Index));
            }

            // Kullanıcıyı bul.
            var user = await _userManager
                .FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcının mevcut rollerini getir.
            var currentRoles = await _userManager
                .GetRolesAsync(user);

            // Admin rolünün değiştirilmesini engelle.
            if (currentRoles.Contains("Admin"))
            {
                TempData["ErrorMessage"] =
                    "Admin kullanıcısının rolü bu ekrandan değiştirilemez.";

                return RedirectToAction(nameof(Index));
            }

            // Önceden atanmış Manager/Finance rollerini bul.
            var businessRoles = currentRoles
                .Where(r => r == "Manager" || r == "Finance")
                .ToList();

            // Kullanıcı zaten bu role sahipse işlem yapma.
            if (businessRoles.Count == 1 &&
                businessRoles[0] == selectedRole)
            {
                TempData["SuccessMessage"] =
                    "Kullanıcı zaten seçilen role sahip.";

                return RedirectToAction(nameof(Index));
            }

            // Eski iş rollerini kaldır.
            if (businessRoles.Count > 0)
            {
                var removeResult = await _userManager
                    .RemoveFromRolesAsync(user, businessRoles);

                if (!removeResult.Succeeded)
                {
                    TempData["ErrorMessage"] =
                        "Eski roller kaldırılamadı.";

                    return RedirectToAction(nameof(Index));
                }
            }

            // Yeni rolü ata.
            var addResult = await _userManager
                .AddToRoleAsync(user, selectedRole);

            if (!addResult.Succeeded)
            {
                // Hata oluşursa eski rolleri geri yüklemeyi dene.
                if (businessRoles.Count > 0)
                {
                    var restoreResult = await _userManager
                        .AddToRolesAsync(user, businessRoles);

                    TempData["ErrorMessage"] =
                        restoreResult.Succeeded
                            ? "Yeni rol atanamadı. Eski roller geri yüklendi."
                            : "Rol ataması başarısız. Eski roller " +
                              "geri yüklenemedi; hesabı kontrol edin.";
                }
                else
                {
                    TempData["ErrorMessage"] =
                        "Rol ataması başarısız oldu.";
                }

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Kullanıcı rolü başarıyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }
    }
}
