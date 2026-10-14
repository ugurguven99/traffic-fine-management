
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Traffic.Data;
using Traffic.Models;
using Traffic.Services;

namespace Traffic.Controllers
{
    [Authorize(Roles = "Admin,Manager,Finance")]

    public class TrafficFinesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ApprovalService _approvalService;

        public TrafficFinesController(
            ApplicationDbContext context,
            ApprovalService approvalService)
        {
            _context = context;
            _approvalService = approvalService;
        }

        // GET: TrafficFines
        public async Task<IActionResult> Index()
        {
            var fines = await _context.TrafficFines
                .Include(f => f.Vehicle)
                .AsNoTracking()
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(fines);
        }

        // GET: TrafficFines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var fine = await _context.TrafficFines
                .Include(f => f.Vehicle)
                .Include(f => f.ApprovalHistories)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id.Value);

            if (fine == null)
                return NotFound();

            return View(fine);
        }

        // GET: TrafficFines/Create

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await LoadVehiclesAsync();

            return View(new TrafficFine
            {
                FineDate = DateTime.Today
            });
        }


        // POST: TrafficFines/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("VehicleId,FineAmount,FineDate,Description")]
            TrafficFine input)
        {
            await ValidateFineAsync(input);

            if (!ModelState.IsValid)
            {
                await LoadVehiclesAsync(input.VehicleId);
                return View(input);
            }

            // Durum kullanýcýdan alýnmýyor.
            input.Status = FineStatus.New;
            input.CreatedAt = DateTime.UtcNow;

            _context.TrafficFines.Add(input);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Trafik cezasý baþarýyla oluþturuldu.";

            return RedirectToAction(nameof(Index));
        }

        // GET: TrafficFines/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var fine = await _context.TrafficFines
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id.Value);

            if (fine == null)
                return NotFound();

            if (fine.Status != FineStatus.New)
            {
                TempData["ErrorMessage"] =
                    "Yalnýzca Yeni durumundaki cezalar düzenlenebilir.";

                return RedirectToAction(nameof(Details),
                    new { id = fine.Id });
            }

            await LoadVehiclesAsync(fine.VehicleId);

            return View(fine);
        }

        // POST: TrafficFines/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,VehicleId,FineAmount,FineDate,Description,RowVersion")]
    TrafficFine input)
        {
            if (id != input.Id)
                return NotFound();

            // Formdan orijinal RowVersion gelmiþ mi?
            if (input.RowVersion == null ||
                input.RowVersion.Length == 0)
            {
                TempData["ErrorMessage"] =
                    "Kayýt sürümü bilgisi eksik. Sayfayý yenileyip tekrar deneyin.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Veritabanýndaki güncel trafik cezasýný bul.
            var fine = await _context.TrafficFines
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fine == null)
                return NotFound();

            // Sadece Yeni durumundaki cezalar düzenlenebilir.
            if (fine.Status != FineStatus.New)
            {
                TempData["ErrorMessage"] =
                    "Bu trafik cezasý artýk düzenlenemez. " +
                    "Onay süreci baþlamýþ olabilir.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Formun açýldýðý sürümle veritabanýndaki
            // mevcut sürüm ayný mý?
            if (!fine.RowVersion.SequenceEqual(input.RowVersion))
            {
                TempData["ErrorMessage"] =
                    "Bu trafik cezasý siz formu açtýktan sonra " +
                    "baþka bir kullanýcý tarafýndan deðiþtirilmiþ. " +
                    "Lütfen güncel bilgileri tekrar kontrol edin.";

                return RedirectToAction(nameof(Details), new { id });
            }

            await ValidateFineAsync(input);

            if (!ModelState.IsValid)
            {
                await LoadVehiclesAsync(input.VehicleId);
                return View(input);
            }

            // EF Core'a formun açýldýðý andaki sürümü bildir.
            _context.Entry(fine)
                .Property(f => f.RowVersion)
                .OriginalValue = input.RowVersion;

            // Sadece izin verilen alanlarý güncelle.
            fine.VehicleId = input.VehicleId;
            fine.FineAmount = input.FineAmount;
            fine.FineDate = input.FineDate;
            fine.Description = input.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] =
                    "Kaydetme sýrasýnda kayýt baþka bir iþlem " +
                    "tarafýndan güncellendi. Lütfen tekrar deneyin.";

                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["SuccessMessage"] =
                "Trafik cezasý baþarýyla güncellendi.";

            return RedirectToAction(nameof(Index));
        }


        // GET: TrafficFines/Delete/5

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var fine = await _context.TrafficFines
                .Include(f => f.Vehicle)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id.Value);

            if (fine == null)
                return NotFound();

            if (fine.Status != FineStatus.New)
            {
                TempData["ErrorMessage"] =
                    "Yalnýzca Yeni durumundaki cezalar silinebilir.";

                return RedirectToAction(nameof(Details),
                    new { id = fine.Id });
            }

            bool hasHistory = await _context.ApprovalHistories
                .AnyAsync(h => h.TrafficFineId == fine.Id);

            if (hasHistory)
            {
                TempData["ErrorMessage"] =
                    "Ýþlem geçmiþi bulunan trafik cezasý silinemez.";

                return RedirectToAction(nameof(Details),
                    new { id = fine.Id });
            }

            return View(fine);
        }


        // POST: TrafficFines/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            string? rowVersion)
        {
            // Silme ekranýndan gelen sürüm bilgisi zorunlu.
            if (string.IsNullOrWhiteSpace(rowVersion))
            {
                TempData["ErrorMessage"] =
                    "Kayýt sürümü bulunamadý. Silme ekranýný yeniden açýn.";

                return RedirectToAction(nameof(Index));
            }

            byte[] originalRowVersion;

            try
            {
                originalRowVersion = Convert.FromBase64String(rowVersion);
            }
            catch (FormatException)
            {
                TempData["ErrorMessage"] =
                    "Geçersiz kayýt sürümü bilgisi.";

                return RedirectToAction(nameof(Index));
            }

            if (originalRowVersion.Length != 8)
            {
                TempData["ErrorMessage"] =
                    "Geçersiz kayýt sürümü bilgisi.";

                return RedirectToAction(nameof(Index));
            }

            var fine = await _context.TrafficFines
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fine == null)
                return NotFound();

            // Onay süreci baþlamýþ ceza silinemez.
            if (fine.Status != FineStatus.New)
            {
                TempData["ErrorMessage"] =
                    "Onay süreci baþlamýþ trafik cezasý silinemez.";

                return RedirectToAction(nameof(Details),
                    new { id });
            }

            // Kullanýcýnýn gördüðü sürüm hâlâ güncel mi?
            if (!fine.RowVersion.SequenceEqual(originalRowVersion))
            {
                TempData["ErrorMessage"] =
                    "Bu ceza, silme ekraný açýldýktan sonra deðiþtirildi. " +
                    "Lütfen güncel bilgileri kontrol edin.";

                return RedirectToAction(nameof(Details),
                    new { id });
            }

            // Denetim geçmiþi olan kayýtlarý koru.
            bool hasHistory = await _context.ApprovalHistories
                .AnyAsync(h => h.TrafficFineId == id);

            if (hasHistory)
            {
                TempData["ErrorMessage"] =
                    "Onay geçmiþi bulunan trafik cezasý silinemez.";

                return RedirectToAction(nameof(Details),
                    new { id });
            }

            // Silme iþlemi sýrasýnda da ayný sürümü kontrol et.
            _context.Entry(fine)
                .Property(f => f.RowVersion)
                .OriginalValue = originalRowVersion;

            _context.TrafficFines.Remove(fine);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] =
                    "Ceza silinirken baþka bir iþlem tarafýndan deðiþtirildi. " +
                    "Lütfen listeyi yenileyin.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // Örneðin silme sýrasýnda yeni bir onay geçmiþi oluþmuþsa
                // veritabanýndaki Restrict iliþkisi silmeyi engeller.
                TempData["ErrorMessage"] =
                    "Ceza veritabaný iliþkileri nedeniyle silinemedi. " +
                    "Kaydýn onay durumunu ve geçmiþini kontrol edin.";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Trafik cezasý baþarýyla silindi.";

            return RedirectToAction(nameof(Index));
        }


        // POST: TrafficFines/Submit/5

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var result = await _approvalService.ExecuteAsync(
                id,
                ApprovalAction.Submit,
                User,
                "Trafik cezasý yönetici onayýna gönderildi.");

            return ApprovalResult(id, result);
        }

        
        // POST: TrafficFines/Approve/5
        [HttpPost]
        [Authorize(Roles = "Manager,Finance")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _approvalService.ExecuteAsync(
                id,
                ApprovalAction.Approve,
                User,
                "Trafik cezasý onaylandý.");

            return ApprovalResult(id, result);
        }

        // POST: TrafficFines/Reject/5
        [HttpPost]
        [Authorize(Roles = "Manager,Finance")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(
            int id,
            string? description)
        {
            var result = await _approvalService.ExecuteAsync(
                id,
                ApprovalAction.Reject,
                User,
                description);

            return ApprovalResult(id, result);
        }

        // Onay servisi sonucunu ekrana taþýr.
        private IActionResult ApprovalResult(
            int id,
            (bool Success, string Message) result)
        {
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // Araç dropdown'ýný hazýrlar.
        private async Task LoadVehiclesAsync(
            int? selectedId = null)
        {
            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .OrderBy(v => v.Plate)
                .ToListAsync();

            ViewBag.VehicleId = new SelectList(
                vehicles,
                "Id",
                "Plate",
                selectedId);
        }

        // Trafik cezasýna ait ek doðrulamalar.
        private async Task ValidateFineAsync(
            TrafficFine fine)
        {
            if (fine.FineDate == default)
            {
                ModelState.AddModelError(
                    nameof(TrafficFine.FineDate),
                    "Lütfen ceza tarihini seçiniz.");
            }
            else if (fine.FineDate.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(TrafficFine.FineDate),
                    "Ceza tarihi gelecekte olamaz.");
            }

            bool vehicleExists = await _context.Vehicles
                .AnyAsync(v => v.Id == fine.VehicleId);

            if (!vehicleExists)
            {
                ModelState.AddModelError(
                    nameof(TrafficFine.VehicleId),
                    "Lütfen geçerli bir araç seçiniz.");
            }
        }
    }
}
