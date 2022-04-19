using System;
using System.Collections.Generic;
using System.Threading;
using AddTimeEntryServices.Commands;
using AddTimeEntryServices.Services;
using DAL.Entities;
using DAL.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Services.Tests;

[TestClass]
public class TimeEntryServiceTests
{
    private readonly ILogger<TimeEntryService> _logger;
    private readonly ITimeEntryService _timeEntryService;
    private readonly Mock<IDataverseRepository> _dataverseRepositoryMock;
    
    public TimeEntryServiceTests()
    {
        _logger = NullLoggerFactory.Instance.CreateLogger<TimeEntryService>();
        
        _dataverseRepositoryMock = new Mock<IDataverseRepository>();
        
        _timeEntryService = new TimeEntryService(_dataverseRepositoryMock.Object, _logger);
    }

    private void DataverseRepositoryMockSetUp(List<TimeEntryEntity> timeEntryEntities)
    {
        _dataverseRepositoryMock.Setup(dr =>
                dr.GetTimeEntryEntitiesByDatesAsync(It.IsAny<TimeEntryModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeEntryEntities);
        
        _dataverseRepositoryMock.Setup(dr =>
                dr.AddTimeEntryEntityAsync(It.IsAny<TimeEntryModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid);
    }
    
    
    [TestMethod]
    public void DateTimeConvertedToUtc()
    {
        var start = DateTime.Now;
        var end = DateTime.Now.AddDays(1);

        var command = new AddTimeEntryCommand
        {
            StartOn = start,
            EndOn = end
        };

        DataverseRepositoryMockSetUp(new List<TimeEntryEntity>());

        var result = _timeEntryService.AddTimeEntryAsync(command, CancellationToken.None);
        _dataverseRepositoryMock.Verify(ts =>
                ts.GetTimeEntryEntitiesByDatesAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == start.ToUniversalTime().Date.Day
                        && c.Start.Month == start.ToUniversalTime().Date.Month
                        && c.Start.Year == start.ToUniversalTime().Date.Year
                        && c.End.Day == end.ToUniversalTime().Date.Day
                        && c.End.Month == end.ToUniversalTime().Date.Month
                        && c.End.Year == end.ToUniversalTime().Date.Year
                        ),
                    It.IsAny<CancellationToken>()),
            Times.Once); 
    }
    
    [TestMethod]
    public void AddAllDatesIfTableIsEmpty()
    {
        var start = DateTime.Now.ToUniversalTime().Date;
        var end = DateTime.Now.AddDays(1).ToUniversalTime().Date;

        var command = new AddTimeEntryCommand
        {
            StartOn = start,
            EndOn = end
        };

        DataverseRepositoryMockSetUp(new List<TimeEntryEntity>());

        var result = _timeEntryService.AddTimeEntryAsync(command, CancellationToken.None);
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == start.Day
                        && c.Start.Month == start.Month
                        && c.Start.Year == start.Year
                        && c.End.Day == start.Day
                        && c.End.Month == start.Month
                        && c.End.Year == start.Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Once); 
        
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == end.Day
                        && c.Start.Month == end.Month
                        && c.Start.Year == end.Year
                        && c.End.Day == end.Day
                        && c.End.Month == end.Month
                        && c.End.Year == end.Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [TestMethod]
    public void AddPartOfDatesIfExistFirstDate()
    {
        var start = DateTime.Now.ToUniversalTime().Date;
        var end = DateTime.Now.AddDays(2).ToUniversalTime().Date;

        var command = new AddTimeEntryCommand
        {
            StartOn = start,
            EndOn = end
        };

        DataverseRepositoryMockSetUp(new List<TimeEntryEntity>
        {
            new TimeEntryEntity
            {
                Id = Guid.NewGuid(),
                Start =  start,
                End = start
            }
        });

        var result = _timeEntryService.AddTimeEntryAsync(command, CancellationToken.None);
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == start.Day
                        && c.Start.Month == start.Month
                        && c.Start.Year == start.Year
                        && c.End.Day == start.Day
                        && c.End.Month == start.Month
                        && c.End.Year == start.Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Never); 
        
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == start.AddDays(1).Day
                        && c.Start.Month == start.AddDays(1).Month
                        && c.Start.Year == start.AddDays(1).Year
                        && c.End.Day == start.AddDays(1).Day
                        && c.End.Month == start.AddDays(1).Month
                        && c.End.Year == start.AddDays(1).Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Once); 
        
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == end.Day
                        && c.Start.Month == end.Month
                        && c.Start.Year == end.Year
                        && c.End.Day == end.Day
                        && c.End.Month == end.Month
                        && c.End.Year == end.Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [TestMethod]
    public void AddPartOfDatesIfExistLastDate()
    {
        var start = DateTime.Now.ToUniversalTime().Date;
        var end = DateTime.Now.AddDays(2).ToUniversalTime().Date;

        var command = new AddTimeEntryCommand
        {
            StartOn = start,
            EndOn = end
        };

        DataverseRepositoryMockSetUp(new List<TimeEntryEntity>
        {
            new TimeEntryEntity
            {
                Id = Guid.NewGuid(),
                Start =  end,
                End = end
            }
        });

        var result = _timeEntryService.AddTimeEntryAsync(command, CancellationToken.None);
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == start.Day
                        && c.Start.Month == start.Month
                        && c.Start.Year == start.Year
                        && c.End.Day == start.Day
                        && c.End.Month == start.Month
                        && c.End.Year == start.Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Once); 
        
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == start.AddDays(1).Day
                        && c.Start.Month == start.AddDays(1).Month
                        && c.Start.Year == start.AddDays(1).Year
                        && c.End.Day == start.AddDays(1).Day
                        && c.End.Month == start.AddDays(1).Month
                        && c.End.Year == start.AddDays(1).Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Once); 
        
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == end.Day
                        && c.Start.Month == end.Month
                        && c.Start.Year == end.Year
                        && c.End.Day == end.Day
                        && c.End.Month == end.Month
                        && c.End.Year == end.Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Never);
    }
    
    [TestMethod]
    public void AddPartOfDatesIfExistMiddleDate()
    {
        var start = DateTime.Now.ToUniversalTime().Date;
        var end = DateTime.Now.AddDays(2).ToUniversalTime().Date;

        var command = new AddTimeEntryCommand
        {
            StartOn = start,
            EndOn = end
        };

        DataverseRepositoryMockSetUp(new List<TimeEntryEntity>
        {
            new TimeEntryEntity
            {
                Id = Guid.NewGuid(),
                Start =  start.AddDays(1),
                End = start.AddDays(1)
            }
        });

        var result = _timeEntryService.AddTimeEntryAsync(command, CancellationToken.None);
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == start.Day
                        && c.Start.Month == start.Month
                        && c.Start.Year == start.Year
                        && c.End.Day == start.Day
                        && c.End.Month == start.Month
                        && c.End.Year == start.Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Once); 
        
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == start.AddDays(1).Day
                        && c.Start.Month == start.AddDays(1).Month
                        && c.Start.Year == start.AddDays(1).Year
                        && c.End.Day == start.AddDays(1).Day
                        && c.End.Month == start.AddDays(1).Month
                        && c.End.Year == start.AddDays(1).Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Never); 
        
        _dataverseRepositoryMock.Verify(ts =>
                ts.AddTimeEntryEntityAsync(
                    It.Is<TimeEntryModel>(c =>
                        c.Start.Day == end.Day
                        && c.Start.Month == end.Month
                        && c.Start.Year == end.Year
                        && c.End.Day == end.Day
                        && c.End.Month == end.Month
                        && c.End.Year == end.Year
                    ),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }
}