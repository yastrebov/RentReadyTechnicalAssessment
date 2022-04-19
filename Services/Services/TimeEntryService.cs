using AddTimeEntryServices.Commands;
using DAL.Entities;
using DAL.Repository;
using Microsoft.Extensions.Logging;

namespace AddTimeEntryServices.Services;

public interface ITimeEntryService
{
    Task<List<Guid>> AddTimeEntryAsync(AddTimeEntryCommand command, CancellationToken cancellationToken);
}

public class TimeEntryService: ITimeEntryService
{
    private readonly IDataverseRepository _dataverseRepository;
    private readonly ILogger<TimeEntryService> _logger;
    
    public TimeEntryService(
        IDataverseRepository dataverseRepository,
        ILogger<TimeEntryService> logger)
    {
        _dataverseRepository = dataverseRepository;
        _logger = logger;
    }
    
    public async Task<List<Guid>> AddTimeEntryAsync(AddTimeEntryCommand command,
        CancellationToken cancellationToken)
    {
        command.StartOn = command.StartOn.ToUniversalTime().Date;
        command.EndOn = command.EndOn.ToUniversalTime().Date;
        
        var existedTimeEntryEntities = await _dataverseRepository.GetTimeEntryEntitiesByDatesAsync(
            new TimeEntryModel
            {
                Start = command.StartOn,
                End = command.EndOn
            },
            cancellationToken);
        
        var datesInInterval = GetAllDaysFromInterval(command).ToList();
        var excludeDates = existedTimeEntryEntities.Select(x => x.Start).ToList();
        var newDates = datesInInterval.Except(excludeDates);
        
        var addTimeEntryEntityTasks = new List<Task>();
        foreach (var newDate in newDates)
        {
            var timeEntryModel = new TimeEntryModel
            {
                Start = newDate,
                End = newDate
            };
            var task = _dataverseRepository.AddTimeEntryEntityAsync(timeEntryModel, cancellationToken);
            addTimeEntryEntityTasks.Add(task);
        }
        
        await Task.WhenAll(addTimeEntryEntityTasks);

        var createdEntityIds = new List<Guid>(); 
        foreach (var addTimeEntryEntityTask in addTimeEntryEntityTasks)
        {
            var id = ((Task<Guid>)addTimeEntryEntityTask).Result;
            createdEntityIds.Add(id);
        }
        
        return createdEntityIds;
    }

    private IEnumerable<DateTime> GetAllDaysFromInterval(AddTimeEntryCommand command)
    {
        for (var dateTime = command.StartOn.Date;
             dateTime.Date <= command.EndOn.Date;
             dateTime = dateTime.AddDays(1))
        {
            yield return dateTime;
        }
    }
}