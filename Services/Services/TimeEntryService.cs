using AddTimeEntryServices.Commands;
using DAL.Repository;
using Microsoft.Extensions.Logging;

namespace AddTimeEntryServices.Services;

public interface ITimeEntryService
{
    Task AddTimeEntryAsync(AddTimeEntryCommand command, CancellationToken cancellationToken);
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
    
    public async Task AddTimeEntryAsync(
        AddTimeEntryCommand command, 
        CancellationToken cancellationToken)
    {
        var datesInInterval = GetAllDaysFromInterval(command);
        var timeEntryEntities = await _dataverseRepository.GetTimeEntryEntitiesByDatesAsync(
            datesInInterval,
            cancellationToken);
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