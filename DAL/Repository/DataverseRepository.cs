using DAL.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace DAL.Repository;

public interface IDataverseRepository
{
    Task<List<TimeEntryEntity>> GetTimeEntryEntitiesByDatesAsync(IEnumerable<DateTime> datesInInterval, CancellationToken cancellationToken);
}
public class DataverseRepository: IDataverseRepository
{
    private readonly ServiceClient _dataverseServiceClient;
    private readonly ILogger<DataverseRepository> _logger;

    private const string TimeEntryTableName = "msdyn_timeentry";
    
    private const string TimeEntryIdFieldName = "msdyn_timeentryid";
    
    private const string TimeEntryStartFieldName = "msdyn_start";
    private const string TimeEntryEndFieldName = "msdyn_end";

    public DataverseRepository(
        ILogger<DataverseRepository> logger,
        DataverseSettings dataverseSettings)
    {
        if (string.IsNullOrEmpty(dataverseSettings.AppUser))
            throw new ArgumentException("Dataverse AppUser is empty");
        if (string.IsNullOrEmpty(dataverseSettings.Password))
            throw new ArgumentException("Dataverse Password is empty");
        if (string.IsNullOrEmpty(dataverseSettings.Url))
            throw new ArgumentException("Dataverse Url is empty");

        var connectionString =
            $"AuthType=OAuth;Url={dataverseSettings.Url};UserName={dataverseSettings.AppUser};Password={dataverseSettings.Password};";
        _dataverseServiceClient = new ServiceClient(connectionString, logger);

        _logger = logger;
    }

    public async Task<List<TimeEntryEntity>> GetTimeEntryEntitiesByDatesAsync(
        IEnumerable<DateTime> datesInInterval,
        CancellationToken cancellationToken)
    {
        var timeEntryEntities = await _dataverseServiceClient.RetrieveMultipleAsync(new QueryExpression(TimeEntryTableName)
            {
                ColumnSet = new ColumnSet(TimeEntryStartFieldName, TimeEntryEndFieldName),
                Criteria = new FilterExpression
                {
                    Filters =
                    {
                        new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression(TimeEntryStartFieldName, ConditionOperator.In, datesInInterval),
                            }
                        }
                    }
                }
            },
            cancellationToken);

        if (timeEntryEntities?.Entities == null || !timeEntryEntities.Entities.Any())
            return new List<TimeEntryEntity>();

        var returnCollection = timeEntryEntities.Entities.Select(te => new TimeEntryEntity
        {
            Id = te.GetAttributeValue<Guid>(TimeEntryIdFieldName),
            Start = te.GetAttributeValue<DateTime>(TimeEntryStartFieldName),
            End = te.GetAttributeValue<DateTime>(TimeEntryEndFieldName)
        }).ToList();

        return returnCollection;
    }
}