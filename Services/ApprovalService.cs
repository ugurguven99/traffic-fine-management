
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Traffic.Data;
using Traffic.Models;

namespace Traffic.Services
{
    public class ApprovalService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ApprovalService(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<(bool Success, string Message)> ExecuteAsync(
            int fineId,
            ApprovalAction action,
            ClaimsPrincipal principal,
            string? description = null)
        {
            // 1. İşlemi yapan kullanıcıyı bul.
            var user = await _userManager.GetUserAsync(principal);

            if (user == null)
            {
                return (false, "Bu işlem için giriş yapmalısınız.");
            }

            // 2. Trafik cezasını veritabanından getir.
            var fine = await _context.TrafficFines
                .FirstOrDefaultAsync(f => f.Id == fineId);

            if (fine == null)
            {
                return (false, "Trafik cezası bulunamadı.");
            }

            // 3. Mevcut durumu sakla.
            FineStatus previousStatus = fine.Status;
            FineStatus nextStatus;

            // 4. Açıklamayı düzenle.
            description = description?.Trim();

            if (description?.Length > 500)
            {
                return (false,
                    "Açıklama en fazla 500 karakter olabilir.");
            }

            // 5. İşleme göre durum ve yetki kontrolü yap.
            switch (action)
            {

                case ApprovalAction.Submit:

                    bool isAdmin = await _userManager
                        .IsInRoleAsync(user, "Admin");

                    if (!isAdmin)
                    {
                        return (false,
                            "Trafik cezasını onaya göndermek için " +
                            "Admin yetkisi gereklidir.");
                    }

                    if (fine.Status != FineStatus.New)
                    {
                        return (false,
                            "Yalnızca Yeni durumundaki cezalar " +
                            "onaya gönderilebilir.");
                    }

                    nextStatus = FineStatus.ManagerApproval;

                    description ??=
                        "Trafik cezası yönetici onayına gönderildi.";

                    break;


                case ApprovalAction.Approve:

                    if (fine.Status == FineStatus.ManagerApproval)
                    {
                        bool isManager = await _userManager
                            .IsInRoleAsync(user, "Manager");

                        if (!isManager)
                        {
                            return (false,
                                "Bu işlem için Manager yetkisi gereklidir.");
                        }

                        nextStatus = FineStatus.FinanceApproval;
                    }
                    else if (fine.Status == FineStatus.FinanceApproval)
                    {
                        bool isFinance = await _userManager
                            .IsInRoleAsync(user, "Finance");

                        if (!isFinance)
                        {
                            return (false,
                                "Bu işlem için Finance yetkisi gereklidir.");
                        }

                        nextStatus = FineStatus.Completed;
                    }
                    else
                    {
                        return (false,
                            "Ceza mevcut durumunda onaylanamaz.");
                    }

                    description ??= "Trafik cezası onaylandı.";
                    break;

                case ApprovalAction.Reject:

                    if (string.IsNullOrWhiteSpace(description))
                    {
                        return (false,
                            "Ret nedeni belirtmek zorunludur.");
                    }

                    if (fine.Status == FineStatus.ManagerApproval)
                    {
                        bool isManager = await _userManager
                            .IsInRoleAsync(user, "Manager");

                        if (!isManager)
                        {
                            return (false,
                                "Bu işlem için Manager yetkisi gereklidir.");
                        }
                    }
                    else if (fine.Status == FineStatus.FinanceApproval)
                    {
                        bool isFinance = await _userManager
                            .IsInRoleAsync(user, "Finance");

                        if (!isFinance)
                        {
                            return (false,
                                "Bu işlem için Finance yetkisi gereklidir.");
                        }
                    }
                    else
                    {
                        return (false,
                            "Ceza mevcut durumunda reddedilemez.");
                    }

                    nextStatus = FineStatus.Rejected;
                    break;

                default:
                    return (false, "Geçersiz onay işlemi.");
            }

            // 6. Trafik cezasının durumunu değiştir.
            fine.Status = nextStatus;

            // 7. Onay geçmişine yeni kayıt ekle.
            var history = new ApprovalHistory
            {
                TrafficFineId = fine.Id,
                UserId = user.Id,
                UserEmail = user.Email ?? user.UserName ?? user.Id,
                Action = action,
                PreviousStatus = previousStatus,
                NewStatus = nextStatus,
                Description = description,
                ActionDate = DateTime.UtcNow
            };

            _context.ApprovalHistories.Add(history);

            // 8. Durum ve geçmişi birlikte kaydet.
            try
            {
                await _context.SaveChangesAsync();

                return (true,
                    "İşlem başarıyla gerçekleştirildi.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false,
                    "Bu trafik cezası başka bir kullanıcı tarafından " +
                    "güncellendi. Sayfayı yenileyip tekrar deneyin.");
            }
        }
    }
}
