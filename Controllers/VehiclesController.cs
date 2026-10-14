
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Traffic.Data;
using Traffic.Models;

namespace Traffic.Controllers
{
    [Authorize(Roles = "Admin,Manager,Finance")]
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .OrderBy(v => v.Plate)
                .ToListAsync();

            return View(vehicles);
        }

        // GET: Vehicles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var vehicle = await _context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id.Value);

            if (vehicle == null)
                return NotFound();

            return View(vehicle);
        }

        // GET: Vehicles/Create
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Plate,VehicleType,Brand,Model")] Vehicle vehicle)
        {
            NormalizeVehicle(vehicle);
            ValidateVehicle(vehicle);

            if (!ModelState.IsValid)
                return View(vehicle);

            bool plateExists = await _context.Vehicles
                .AnyAsync(v => v.Plate == vehicle.Plate);

            if (plateExists)
            {
                ModelState.AddModelError(
                    nameof(Vehicle.Plate),
                    "Bu plakaya sahip bir araç zaten kayýtlý.");

                return View(vehicle);
            }

            try
            {
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Araç baþarýyla eklendi.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
                when (IsUniqueViolation(ex))
            {
                _context.Entry(vehicle).State =
                    EntityState.Detached;

                ModelState.AddModelError(
                    nameof(Vehicle.Plate),
                    "Bu plakaya sahip bir araç zaten kayýtlý.");

                return View(vehicle);
            }
        }

        // GET: Vehicles/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var vehicle = await _context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id.Value);

            if (vehicle == null)
                return NotFound();

            return View(vehicle);
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Plate,VehicleType,Brand,Model")] Vehicle vehicle,
            byte[]? rowVersion)
        {
            if (id != vehicle.Id)
                return NotFound();

            // Formdan orijinal sürüm gelmelidir.
            if (rowVersion == null || rowVersion.Length == 0)
                return BadRequest("Araç sürüm bilgisi eksik.");

            // Hata durumunda Edit ekranýnýn tekrar
            // açýlabilmesi için form sürümünü koru.
            vehicle.RowVersion = rowVersion;

            NormalizeVehicle(vehicle);
            ValidateVehicle(vehicle);

            if (!ModelState.IsValid)
                return View(vehicle);

            bool plateExists = await _context.Vehicles
                .AnyAsync(v =>
                    v.Plate == vehicle.Plate &&
                    v.Id != vehicle.Id);

            if (plateExists)
            {
                ModelState.AddModelError(
                    nameof(Vehicle.Plate),
                    "Bu plaka baþka bir araca ait.");

                return View(vehicle);
            }

            var existingVehicle = await _context.Vehicles
                .FindAsync(id);

            if (existingVehicle == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Bu araç baþka bir kullanýcý tarafýndan silinmiþ.");

                return View(vehicle);
            }

            // Kritik nokta:
            // EF Core, veritabanýndaki RowVersion ile
            // form açýldýðýnda alýnan sürümü karþýlaþtýrýr.
            _context.Entry(existingVehicle)
                .Property(v => v.RowVersion)
                .OriginalValue = rowVersion;

            existingVehicle.Plate = vehicle.Plate;
            existingVehicle.VehicleType = vehicle.VehicleType;
            existingVehicle.Brand = vehicle.Brand;
            existingVehicle.Model = vehicle.Model;

            try
            {
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Araç baþarýyla güncellendi.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.Entry(existingVehicle).State =
                    EntityState.Detached;

                ModelState.AddModelError(
                    string.Empty,
                    "Bu araç baþka bir iþlem tarafýndan güncellendi " +
                    "veya silindi. Deðiþiklikleriniz kaydedilmedi. " +
                    "Lütfen araç listesinden güncel kaydý yeniden açýn.");

                return View(vehicle);
            }
            catch (DbUpdateException ex)
                when (IsUniqueViolation(ex))
            {
                _context.Entry(existingVehicle).State =
                    EntityState.Detached;

                ModelState.AddModelError(
                    nameof(Vehicle.Plate),
                    "Bu plaka baþka bir araca ait.");

                return View(vehicle);
            }
        }

        // GET: Vehicles/Delete/5
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var vehicle = await _context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id.Value);

            if (vehicle == null)
                return NotFound();

            return View(vehicle);
        }

        // POST: Vehicles/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            byte[]? rowVersion)
        {
            if (rowVersion == null || rowVersion.Length == 0)
                return BadRequest("Araç sürüm bilgisi eksik.");

            var vehicle = await _context.Vehicles
                .FindAsync(id);

            if (vehicle == null)
            {
                TempData["ErrorMessage"] =
                    "Bu araç baþka bir iþlem tarafýndan silinmiþ.";

                return RedirectToAction(nameof(Index));
            }

            // Silme sýrasýnda da orijinal sürüm kullanýlýr.
            _context.Entry(vehicle)
                .Property(v => v.RowVersion)
                .OriginalValue = rowVersion;

            // Önce iliþkili trafik cezasý var mý kontrol et.
            bool hasFines = await _context.TrafficFines
                .AnyAsync(f => f.VehicleId == id);

            if (hasFines)
            {
                TempData["ErrorMessage"] =
                    "Bu araca ait trafik cezasý bulunduðu için " +
                    "araç silinemez.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Vehicles.Remove(vehicle);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] =
                    "Araç baþarýyla silindi.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["ErrorMessage"] =
                    "Bu araç baþka bir iþlem tarafýndan " +
                    "güncellendi veya silindi. " +
                    "Güncel kaydý kontrol edip tekrar deneyin.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
                when (IsForeignKeyViolation(ex))
            {
                TempData["ErrorMessage"] =
                    "Bu araca baðlý trafik cezasý bulunduðu için " +
                    "araç silinemedi.";

                return RedirectToAction(nameof(Index));
            }
        }

        // Plaka ve diðer metinleri standartlaþtýrýr.
        private static void NormalizeVehicle(Vehicle vehicle)
        {
            vehicle.Plate = vehicle.Plate?
                .Trim()
                .ToUpperInvariant() ?? string.Empty;

            vehicle.Brand = vehicle.Brand?.Trim()
                ?? string.Empty;

            vehicle.Model = vehicle.Model?.Trim()
                ?? string.Empty;
        }

        // Normalize edilen veriyi yeniden doðrular.
        private void ValidateVehicle(Vehicle vehicle)
        {
            ModelState.Clear();
            TryValidateModel(vehicle);
        }

        // SQL Server unique index hata kodlarý
        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                   (sqlEx.Number == 2601 ||
                    sqlEx.Number == 2627);
        }

        // SQL Server foreign key hata kodu
        private static bool IsForeignKeyViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlEx &&
                   sqlEx.Number == 547;
        }
    }
}
