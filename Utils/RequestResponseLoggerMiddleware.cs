using Pawlio.Log;
using Pawlio.Models;


namespace Pawlio.Core.Utils
{
    public class RequestResponseLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private static List<Log.Log> cache = new List<Log.Log>();

        const int maxCountDown = 60;
        static int countDown = maxCountDown;

        public static void Init()
        {
            _ = new Timer((e) =>
            {
                countDown--;
                if (countDown <= 0)
                {
                    countDown = maxCountDown;
                    lock (cache)
                    {
                        SaveLogs();
                    }
                }
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        public RequestResponseLoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                //if (!LogFilter.Write)
                //{
                //    await _next(httpContext); // Call the next middleware in the pipeline  
                //    return;
                //}

                if (httpContext.Request.Path.StartsWithSegments("/api/image") ||
                    httpContext.Request.Path.StartsWithSegments("/api/user/updatePhoto"))
                {
                    await _next(httpContext);
                    return;
                }

                if (!httpContext.Request.Headers.TryGetValue("UserId", out var userId)) userId = "0";
                if (!httpContext.Request.Headers.TryGetValue("FirmId", out var firmId)) firmId = "0";
                if (!httpContext.Request.Headers.TryGetValue("BranchId", out var branchId)) branchId = "0";
                if (!httpContext.Request.Headers.TryGetValue("FirmType", out var firmType)) firmType = "0";
                if (!httpContext.Request.Headers.TryGetValue("SerialNo", out var serialNo)) serialNo = "";
                if (!httpContext.Request.Headers.TryGetValue("BN", out var appBuildNumber)) appBuildNumber = "0";

                if (!httpContext.Request.Headers.TryGetValue("PN", out var packageName)) packageName = "0";
                byte appId = 0;
                if (packageName == "com.hizliis") appId = 1;
                if (packageName == "com.hizlivet") appId = 2;
                //int _userId = userId.ToInt();
                //if (LogFilter.UserId > 0 && LogFilter.UserId != _userId) // İstenilen UserId değilse geç
                //{
                //    await _next(httpContext);
                //    return;
                //}

                // Temporarily replace the HttpResponseStream, which is a write-only stream, with a MemoryStream to capture it's value in-flight.  
                var originalResponseBody = httpContext.Response.Body;
                using var newResponseBody = new MemoryStream();
                httpContext.Response.Body = newResponseBody;

                await _next(httpContext);

                newResponseBody.Seek(0, SeekOrigin.Begin);
                var responseBodyText = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
                newResponseBody.Seek(0, SeekOrigin.Begin);
                await newResponseBody.CopyToAsync(originalResponseBody);

                var requestBodyText = (await ReadBodyFromRequest(httpContext.Request));
                var clientIP = httpContext.Connection.RemoteIpAddress?.ToString() ?? "";

                lock (cache)
                {
                    cache.Add(new Log.Log
                    {
                        AppId = appId,
                        UserId = userId.ToString().ToInt(),
                        FirmId = firmId.ToString().ToInt(),
                        BranchId = branchId.ToString().ToInt(),
                        FirmType = (byte)firmType.ToString().ToInt(),
                        SerialNo = serialNo!,
                        AppBuildNumber = appBuildNumber.ToString().ToInt(),
                        Method = httpContext.Request.Method,
                        Path = httpContext.Request.Path,
                        QueryString = httpContext.Request.QueryString.ToString(),
                        RequestHeaders = FormatHeaders(httpContext.Request.Headers),
                        ClientIP = clientIP,
                        Host = httpContext.Request.Host.ToString(),
                        RequestBody = requestBodyText, //?.Left(1000) ?? "",
                        StatusCode = httpContext.Response.StatusCode,
                        ContentType = httpContext.Response.ContentType ?? "",
                        ResponseHeaders = FormatHeaders(httpContext.Response.Headers),
                        ResponseBody = responseBodyText?.Left(1000) ?? ""
                    });

                    // Geri sayımı sıfırla, eğer işlem gelmez liste dolmazsa geri sayım sonrası kayıt yapılacak
                    countDown = maxCountDown;

                    if (cache.Count >= 25)
                    {
                        SaveLogs();
                    }
                }
            }
            catch (Exception e)
            {
                await _next(httpContext);
                // 04.05.2023 Şafak
                // Log hata durumu, loga yazamaz ise başka yere yazsın şeklinde bir ekleme yapılabilir
            }
        }

        static void SaveLogs()
        {
            if (cache.Count == 0) return;

            var logs = new List<Log.Log>();
            logs.AddRange(cache);
            cache.Clear();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var db = new Log.PostgreSqlLogDbContext();
                    db.WebLogs.AddRange(logs);
                    db.SaveChangesAsync();
                }
                catch
                {
                    // 04.05.2023 Şafak
                    // Loga yazamaz ise başka yere yazsın şeklinde bir ekleme yapılabilir
                }
            });
        }

        private static string FormatHeaders(IHeaderDictionary headers) => string.Join(", ", headers.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value)}"));

        private static async Task<string> ReadBodyFromRequest(HttpRequest request)
        {
            // Ensure the request's body can be read multiple times (for the next middlewares in the pipeline).  
            request.EnableBuffering();
            request.Body.Position = 0;
            var requestBody = await new System.IO.StreamReader(request.Body).ReadToEndAsync();
            // Reset the request's body stream position for next middleware in the pipeline.  
            request.Body.Position = 0;
            return requestBody;
        }
    }
}
