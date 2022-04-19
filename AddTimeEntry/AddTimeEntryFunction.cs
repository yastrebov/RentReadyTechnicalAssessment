using System;
using System.Threading;
using System.Threading.Tasks;
using AddTimeEntryServices.Commands;
using AddTimeEntryServices.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AddTimeEntry
{
    public class AddTimeEntryFunction
    {
        private readonly ITimeEntryService _timeEntryService;

        public AddTimeEntryFunction(ITimeEntryService timeEntryService)
        {
            this._timeEntryService = timeEntryService;
        }

        [FunctionName("AddTimeEntry")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            Models.AddTimeEntry model,
            ILogger log,
            CancellationToken cancellationToken)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (model == null)
                return new BadRequestObjectResult("Model is invalid.");

            if (!DateTime.TryParse(model.StartOn, out var startOn))
                return new BadRequestObjectResult("Invalid param StartOn. Param should be date.");

            if (!DateTime.TryParse(model.EndOn, out var endOn))
                return new BadRequestObjectResult("Invalid param EndOn. Param should be date.");

            if (startOn >= endOn)
                return new BadRequestObjectResult("EndOn should be greater than StartOn");

            await _timeEntryService.AddTimeEntryAsync(new AddTimeEntryCommand
                {
                    StartOn = startOn,
                    EndOn = endOn
                },
                cancellationToken);

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
    }
}