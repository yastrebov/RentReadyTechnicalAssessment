using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AddTimeEntryServices.Commands;
using AddTimeEntryServices.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AddTimeEntry.Tests;

[TestClass]
public class AddTimeEntryTests
{
    private readonly ILogger _logger;
    private readonly AddTimeEntryFunction _addTimeEntryFunction;
    private readonly Mock<ITimeEntryService> _timeEntryServiceMock;

    public AddTimeEntryTests()
    {
        _logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
        
        _timeEntryServiceMock = new Mock<ITimeEntryService>();
        _timeEntryServiceMock.Setup(ts =>
            ts.AddTimeEntryAsync(It.IsAny<AddTimeEntryCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Guid>{Guid.NewGuid()});
        
        _addTimeEntryFunction = new AddTimeEntryFunction(_timeEntryServiceMock.Object);
    }
    
    [TestMethod]
    public async Task ValidationInputModelEmptyTest()
    {
        var model = new Models.AddTimeEntry();

        var result = await _addTimeEntryFunction.Run(model, _logger, CancellationToken.None);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));   
    }

    [TestMethod]
    public async Task ValidationStartValueTest()
    {
        var start = "";
        var end = "2022-04-20T18:25:43.511Z";
        
        var model = new Models.AddTimeEntry
        {
            StartOn = start,
            EndOn = end
        };

        var result = await _addTimeEntryFunction.Run(model, _logger, CancellationToken.None);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));   
    }
    
    [TestMethod]
    public async Task ValidationEndValueTest()
    {
        var start = "2022-04-15T18:25:43.511Z";
        var end = "";
        
        var model = new Models.AddTimeEntry
        {
            StartOn = start,
            EndOn = end
        };

        var result = await _addTimeEntryFunction.Run(model, _logger, CancellationToken.None);
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));   
    }
    
    [TestMethod]
    public async Task ValidationCorrectValuesTest()
    {
        var start = "2022-04-15T18:25:43.511Z";
        var end = "2022-04-20T18:25:43.511Z";
        
        var model = new Models.AddTimeEntry
        {
            StartOn = start,
            EndOn = end
        };

        var result = await _addTimeEntryFunction.Run(model, _logger, CancellationToken.None);
        _timeEntryServiceMock.Verify(ts =>
                ts.AddTimeEntryAsync(
                    It.Is<AddTimeEntryCommand>(c =>
                        c.StartOn == DateTime.Parse(start) 
                        && c.EndOn == DateTime.Parse(end)),
                    It.IsAny<CancellationToken>()),
            Times.Once); 
    }
}