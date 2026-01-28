using Microsoft.AspNetCore.SignalR;

namespace Pawlio.Hubs
{
    public class PosHub : Hub
    {
        //public static Dictionary<string, PosTerminal> PosTerminals = new Dictionary<string, PosTerminal>();

        public async Task<string> Start(string buildType, string serialNo)
        {
            string deviceId = $"{buildType}/{serialNo}";
            //if (PosTerminals.TryGetValue(deviceId, out var device)) return "NODEVICE";

            //var _context = new PostgreSqlDbContext();
            //var device = await _context.PosTerminals
            //    .Include(p => p.User)
            //    .FirstOrDefaultAsync(d => d.SerialNo == serialNo && d.DeviceMark == buildType && !d.IsDeleted);

            //if (device == null)
            //{
            //    device = new PosTerminal
            //    {
            //        Flavor = Flavor.None,
            //        Name = buildType,
            //        SerialNo = serialNo,
            //        DeviceMark = buildType,
            //        ConnectionId = Context.ConnectionId,
            //    };

            //    PosTerminals[deviceId] = device;
            //    return "LOGIN";
            //}

            //device.ConnectionId = Context.ConnectionId;
            //await _context.SaveChangesAsync();

            //var user = device.User;
            //if (user != null)
            //{
            //    user.Password = null;
            //    //user.PNToken = null;
            //}

            //PosTerminals[deviceId] = device;
            //var deviceJson = device.ToJson();
            //await Clients.Client(Context.ConnectionId).SendAsync("posUpdate", deviceJson);
            return "";
        }
    }
}
