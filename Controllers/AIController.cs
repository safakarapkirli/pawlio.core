using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Pawlio.Hubs;

namespace Pawlio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ApiController
    {
        public AIController(PostgreSqlDbContext context, IHubContext<MainHub> mainHub) : base(context, mainHub) { }

        [HttpGet]
        public async Task<string> Test()
        {
            return await Query(new { Query = "İhsan Genç isimli müşterinin ocak 2024 için toplam satış tutarı nedir?" });
        }

        [HttpPost]
        public async Task<string> Query(dynamic data)
        {
            string query = data.query;

            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-3.5-turbo", "YOUR_OPENAI_API_KEY")
                .Build();

            var path = Path.GetFullPath("AppData");
            var template = System.IO.File.ReadAllText(path + "/chatgpt-template.txt");

            object promt = new
            {
                model = "gpt-3.5-turbo",
                massages = new List<dynamic> 
                {
                    new { role = "system", content = template },
                    new { role = "user", content = query }
                }
            };

            var jsonPrompt = promt.ToJson();
            var response = await kernel.InvokePromptAsync<string>(jsonPrompt);
            return response ?? "NULL";
        }
    }


}
