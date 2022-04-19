using System;
using System.IO;
using System.Threading.Tasks;
using AddTimeEntryServices.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AddTimeEntry
{
    public class AddTimeEntryFunction
    {
        private readonly IAddTimeEntryService _addTimeEntryService;

        public AddTimeEntryFunction(IAddTimeEntryService addTimeEntryService)
        {
            this._addTimeEntryService = addTimeEntryService;
        }
        
        [FunctionName("AddTimeEntry")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] AddTimeEntry req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            // string name = req.Query["name"];
            //
            // string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            // dynamic data = JsonConvert.DeserializeObject(requestBody);
            // name = name ?? data?.name;
            //
            // string responseMessage = string.IsNullOrEmpty(name)
            //     ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
            //     : $"Hello, {name}. This HTTP triggered function executed successfully.";
            //
            // return new OkObjectResult(responseMessage);
            
            return new OkResult();
        }
        
        public class AddTimeEntry
        {
            [JsonProperty(Required = Required.Always)]
            public DateTime StartOn { get; set; }
            
            [JsonProperty(Required = Required.Always)]
            public DateTime EndOn { get; set; }
        }
    }
}
