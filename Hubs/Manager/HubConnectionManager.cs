using Pawlio.Models;

namespace Pawlio.Hubs.Manager
{
    public delegate void FirmHubCallBack(int index, FirmHub hub);

    public static class HubConnectionManager
    {
        public static Dictionary<int, Dictionary<string, FirmHub>> firmHubs = new Dictionary<int, Dictionary<string, FirmHub>>();

        public static void Start(string deviceId, int userId, int firmId, int branchId, string connectionId)
        {
            var firmHub = new FirmHub { DeviceId = deviceId, UserId = userId, FirmId = firmId, BranchId = branchId, ConnectionId = connectionId };

            if (!firmHubs.TryGetValue(firmId, out var hubs))
            {
                // Firma ilk defa alınıyor ise
                hubs = new Dictionary<string, FirmHub>();
                firmHubs[firmId] = hubs;
            }

            if (!hubs.TryGetValue(deviceId, out var hub))
            {
                hubs.Add(deviceId, firmHub);
            }
            else
            {
                hubs[deviceId] = firmHub;
            }
        }

        public static void Stop(string deviceId, int userId, int firmId, int branchId)
        {
            if (firmHubs.TryGetValue(firmId, out var firmHub))
                firmHub.Remove(deviceId);
        }

        public static List<FirmHub> FirmClients(int firmId)
        {
            if (!firmHubs.TryGetValue(firmId, out var hubs)) return new List<FirmHub>();
            return hubs.Values.ToList();
        }

        public static void CallOnFirm(int firmId, Action<int, FirmHub> callBack)
        {
            var clients = FirmClients(firmId);
            for (var i = 0; i < clients.Count; i++)
                callBack(i, clients[i]);
        }
    }
}
