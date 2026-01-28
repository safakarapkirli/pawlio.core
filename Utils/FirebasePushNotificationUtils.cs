using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Pawlio.Models;
using Microsoft.Extensions.Configuration;

public static class FirebasePushNotificationUtils
{
    static Dictionary<string, FirebaseApp> apps = new Dictionary<string, FirebaseApp>();

    public static void Init(IConfiguration configuration)
    {
        var procioJson = JsonConvert.SerializeObject(configuration.GetSection("Firebase:Procio").Get<Dictionary<string, object>>());
        var pawlioJson = JsonConvert.SerializeObject(configuration.GetSection("Firebase:Pawlio").Get<Dictionary<string, object>>());

        var procio = FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromJson(procioJson) }, "hizliis");
        var pawlio = FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.FromJson(pawlioJson) }, "pawlio");

        apps.Add("com.procio.app", procio);
        apps.Add("com.pawlio.app", pawlio);
    }

    public static void PushMessage(List<Device> devices, string title, string body, Dictionary<string, string> data)
    {
        foreach (var d in devices)
        {
            try
            {
                var app = apps[d.PackageName];
                if (app == null) continue;

                FirebaseMessaging.GetMessaging(app).SendAsync(
                    new Message
                    {
                        Token = d.PNToken,
                        Notification = new Notification { Body = body, Title = title },
                        Data = data,
                    });
            }
            catch { }
        }
    }
}