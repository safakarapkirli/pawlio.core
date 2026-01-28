using System.Collections.Generic;

namespace Pawlio.Hubs
{
    public class FirmHub
    {
        public string DeviceId { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int FirmId { get; set; }
        public int BranchId { get; set; }
        public string ConnectionId { get; set; } = string.Empty;
    }
}
