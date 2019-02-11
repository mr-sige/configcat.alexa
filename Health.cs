using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET;
using System.Net.Http;

namespace ConfigCat.Function
{
    public static class Health
    {
        private static HttpClient httpClient = new HttpClient();

        [FunctionName("Health")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string json = await req.ReadAsStringAsync();
            var skillRequest = JsonConvert.DeserializeObject<SkillRequest>(json);

            var requestType = skillRequest.GetRequestType();

            SkillResponse response = null;

            if (requestType == typeof(LaunchRequest))
            {
                response = ResponseBuilder.Tell("Welcome to ConfigCat health checker!");
                response.Response.ShouldEndSession = false;
            }

            else if (requestType == typeof(IntentRequest))
            {
                var intentRequest = skillRequest.Request as IntentRequest;

                if (intentRequest.Intent.Name == "Health")
                {
                    var getResponse = await httpClient.GetAsync("https://configcat.com");
                    if (getResponse.IsSuccessStatusCode)
                    {
                        response = ResponseBuilder.Tell("All systems are green!");
                    } else {
                        response = ResponseBuilder.Tell("There is something wrong with the website.");
                    }
                }
            }

            return new OkObjectResult(response);
        }
    }
}
