
//namespace Pawlio.Utils
//{
//    public class ProxyOpenAIHandler : HttpClientHandler
//    {
//        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            if (request.RequestUri != null && request.RequestUri.Host.Equals("api.openai.com", StringComparison.OrdinalIgnoreCase))
//            {
//                var url = $"http://vetapp.com.tr:1338{request.RequestUri!.PathAndQuery}"; // .Replace("v1/", "")
//                request.RequestUri = new Uri(url);
//            }

//            return base.SendAsync(request, cancellationToken);
//        }
//    }
//}
