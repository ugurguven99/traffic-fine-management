
using System.ComponentModel.DataAnnotations;

namespace Traffic.Models
{
    public class ApprovalHistory
    {
        public int Id { get; set; }

        public int TrafficFineId { get; set; }

        public TrafficFine TrafficFine { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string UserEmail { get; set; } = string.Empty;

        public ApprovalAction Action { get; set; }

        public FineStatus PreviousStatus { get; set; }

        public FineStatus NewStatus { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;
    }
}
