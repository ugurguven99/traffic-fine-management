
namespace Traffic.Models
{
    public class DashboardViewModel
    {
        public int TotalVehicles { get; set; }

        public int TotalFines { get; set; }

        public int PendingApprovalCount { get; set; }

        public int CompletedFines { get; set; }

        public int RejectedFines { get; set; }

        public int NewFines { get; set; }

        public int ManagerPendingCount { get; set; }

        public int FinancePendingCount { get; set; }

        public decimal TotalFineAmount { get; set; }
    }
}
