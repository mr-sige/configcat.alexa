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

            var isValid = await ValidateRequestAsync(req, skillRequest);
            if (!isValid)
            {
                return new BadRequestResult();
            }

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
                    }
                    else
                    {
                        response = ResponseBuilder.Tell("There is something wrong with the website.");
                    }
                }
                else if (intentRequest.Intent.Name == "AMAZON.HelpIntent")
                {
                    response = ResponseBuilder.Tell("You can ask config cat how he's feeling.");
                    response.Response.ShouldEndSession = false;
                }
                else if (intentRequest.Intent.Name == "AMAZON.CancelIntent" || intentRequest.Intent.Name == "AMAZON.StopIntent")
                {
                    response = ResponseBuilder.Tell("Goodbye!");
                    response.Response.ShouldEndSession = true;
                }
            }
            else if (requestType == typeof(SessionEndedRequest))
            {
                response = ResponseBuilder.Empty();
                response.Response.ShouldEndSession = true;
            }

            return new OkObjectResult(response);
        }

        private static async Task<bool> ValidateRequestAsync(HttpRequest request, SkillRequest skillRequest)
        {
            request.Headers.TryGetValue("SignatureCertChainUrl", out var signatureChainUrl);
            if (string.IsNullOrWhiteSpace(signatureChainUrl))
            {
                return false;
            }

            Uri certUrl;
            try
            {
                certUrl = new Uri(signatureChainUrl);
            }
            catch
            {
                return false;
            }

            request.Headers.TryGetValue("Signature", out var signature);
            if (string.IsNullOrWhiteSpace(signature))
            {
                return false;
            }

            request.Body.Position = 0;
            var body = await request.ReadAsStringAsync();
            request.Body.Position = 0;

            if (string.IsNullOrWhiteSpace(body))
            {
                return false;
            }

            bool isTimestampValid = RequestVerification.RequestTimestampWithinTolerance(skillRequest);
            bool valid = await RequestVerification.Verify(signature, certUrl, body);

            if (!valid || !isTimestampValid)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
