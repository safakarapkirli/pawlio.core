using Microsoft.AspNetCore.SignalR;

namespace Pawlio.Hubs
{
    public class MainHub : Hub
    {
        //public static Dictionary<string, PosTerminal> PosTerminals = new Dictionary<string, PosTerminal>();

        public override async Task<Task> OnDisconnectedAsync(Exception? exception)
        {
            //var device = PosTerminals.Values.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            //if (device != null)
            //{
            //    string posTerminalId = $"{device.DeviceMark}/{device.SerialNo}";
            //    PosTerminals.Remove(posTerminalId);
            //    await SendDeviceList(new PostgreSqlDbContext(), device.FirmId, device.BranchId);
            //}
            return base.OnDisconnectedAsync(exception);
        }

        public async Task Start(string deviceId, int userId, int firmId, int branchId, string buildType, string serialNo)
        {
            // Firmanın tüm cihazları
            await Groups.AddToGroupAsync(Context.ConnectionId, firmId.ToString());
            // Kullanıcının tüm cihazları
            await Groups.AddToGroupAsync(Context.ConnectionId, "user-" + userId);

            // Pos cihazı ise
            //if (!string.IsNullOrEmpty(serialNo))
            //{
            //    string posTerminalId = $"{buildType.ToUpper()}/{serialNo}";

            //    var _context = new PostgreSqlDbContext();
            //    var posTerminal = await _context.PosTerminals
            //        .Include(p => p.User)
            //        .FirstOrDefaultAsync(d => d.SerialNo == serialNo && d.DeviceMark == buildType && !d.IsDeleted);

            //    if (posTerminal == null)
            //    {
            //        // Cihaz açıldı ama daha önce bir istemci tarafından silindi, oturum kapansın
            //        await Clients.Caller.SendAsync("logout");
            //        return;
            //    }

            //    posTerminal.ConnectionId = Context.ConnectionId;
            //    var user = posTerminal.User;
            //    if (user != null)
            //    {
            //        user.Password = null;
            //        //user.PNToken = null;
            //    }

            //    PosTerminals[posTerminalId] = posTerminal;
            //    await SendDeviceList(_context, firmId, branchId);
            //}
            //else // Telefon veya tablet girişi, 
            //{
            //}
        }

        //public async Task<string> PayStart(string posTerminalJson, string basketJson)
        //{
        //    var device = posTerminalJson.FromJson<PosTerminal>();
        //    string posTerminalId = $"{device.DeviceMark}/{device.SerialNo}";
        //    if (!PosTerminals.TryGetValue(posTerminalId, out var posTerminal)) return "NOTFOUND";

        //    await Clients.Client(posTerminal.ConnectionId).SendAsync("payStart", basketJson);
        //    return "OK";
        //}

        //public async Task<string> PayComplate(int firmId, bool success, string basketId)
        //{
        //    // Tüm cihazlara ödeme tamamlandı mesajı gönderiyorum
        //    // Sepet hangi cihazda ise o cihaz mesajı alıp sepeti boşaltacak
        //    await Clients.Group(firmId.ToString()).SendAsync("payComplate", success.ToString().ToLower(), basketId);
        //    return "OK";
        //}

        //public async Task<string> CancelStart(string posTerminalJson, string basketJson)
        //{
        //    var device = posTerminalJson.FromJson<PosTerminal>();
        //    string posTerminalId = $"{device.DeviceMark}/{device.SerialNo}";
        //    if (!PosTerminals.TryGetValue(posTerminalId, out var posTerminal)) return "NOTFOUND";

        //    await Clients.Client(posTerminal.ConnectionId).SendAsync("cancelStart", basketJson);
        //    return "OK";
        //}

        //public async Task<string> CancelComplate(int firmId, bool success, string basketId)
        //{
        //    // Tüm cihazlara iptal tamamlandı mesajı gönderiyorum
        //    // Sepet hangi cihazda ise o cihaz mesajı alıp sepeti boşaltacak
        //    await Clients.Group(firmId.ToString()).SendAsync("cancelComplate", success.ToString().ToLower(), basketId);
        //    return "OK";
        //}

        async Task SendDeviceList(PostgreSqlDbContext _context, int firmId, int branchId)
        {
            // POS login oldu, tüm ,stemcilerde POS cihazlarının listesini güncelle
            //var posTerminals = await PosController.GetDeviceList(_context, firmId, branchId);
            //var jsonStr = posTerminals.ToJson(formatting: true);
            //await Clients.Groups(firmId.ToString()).SendAsync("posListUpdate", jsonStr);
        }
    }
}
