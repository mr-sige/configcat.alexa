using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Google.Apis.Dialogflow.v2.Data;

namespace ConfigCat.HealthGoogle
{
    public static class HealthGoogle
    {
        [FunctionName("HealthGoogle")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string json = await req.ReadAsStringAsync();
            var dialogRequest = JsonConvert.DeserializeObject<GoogleCloudDialogflowV2WebhookRequest>(json);
            
            var response = new GoogleCloudDialogflowV2WebhookResponse();

            response.FulfillmentText = "ConfigCat rules!";
            return new OkObjectResult(response);
        }
    }
}
