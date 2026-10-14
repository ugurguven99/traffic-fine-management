
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Traffic.Data;
using Traffic.Models;

namespace Traffic.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager,Finance")]
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalVehicles = await _context.Vehicles.CountAsync(),

                TotalFines = await _context.TrafficFines.CountAsync(),

                PendingApprovalCount = await _context.TrafficFines
                    .CountAsync(f =>
                        f.Status == FineStatus.ManagerApproval ||
                        f.Status == FineStatus.FinanceApproval),

                CompletedFines = await _context.TrafficFines
                    .CountAsync(f => f.Status == FineStatus.Completed),

                RejectedFines = await _context.TrafficFines
                    .CountAsync(f => f.Status == FineStatus.Rejected),

                NewFines = await _context.TrafficFines
                    .CountAsync(f => f.Status == FineStatus.New),

                ManagerPendingCount = await _context.TrafficFines
                    .CountAsync(f =>
                        f.Status == FineStatus.ManagerApproval),

                FinancePendingCount = await _context.TrafficFines
                    .CountAsync(f =>
                        f.Status == FineStatus.FinanceApproval),

                TotalFineAmount = await _context.TrafficFines
                    .SumAsync(f => (decimal?)f.FineAmount) ?? 0m
            };

            return View(model);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ??
                            HttpContext.TraceIdentifier
            });
        }
    }
}
