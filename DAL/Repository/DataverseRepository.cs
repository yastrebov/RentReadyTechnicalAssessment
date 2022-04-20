using DAL.Entities;
using DAL.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DAL.Repository;

public interface IDataverseRepository
{
    Task<List<TimeEntryEntity>> GetTimeEntryEntitiesByDatesAsync(TimeEntryModel datesInInterval, CancellationToken cancellationToken);
    Task<Guid> AddTimeEntryEntityAsync(TimeEntryModel entity, CancellationToken token);
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
        try
        {
            _dataverseServiceClient = new ServiceClient(connectionString, logger);
        }
        catch (Exception e)
        {
            logger.LogError($"Can't connect to Dataverse. {e.Message}");
            throw;
        }

        _logger = logger;
    }

    public async Task<List<TimeEntryEntity>> GetTimeEntryEntitiesByDatesAsync(
        TimeEntryModel datesInInterval,
        CancellationToken cancellationToken)
    {
        EntityCollection? timeEntryEntities;
        try
        {
            timeEntryEntities = await _dataverseServiceClient.RetrieveMultipleAsync(new QueryExpression(TimeEntryTableName)
                {
                    ColumnSet = new ColumnSet(TimeEntryStartFieldName, TimeEntryEndFieldName),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression
                            {
                                AttributeName = TimeEntryEndFieldName,
                                Operator = ConditionOperator.GreaterEqual,
                                Values = { datesInInterval.Start }
                            },
                            new ConditionExpression {
                                AttributeName = TimeEntryStartFieldName,
                                Operator = ConditionOperator.LessEqual,
                                Values = { datesInInterval.End }
                            }

                        }
                    }
                },
                cancellationToken);

        }
        catch (Exception e)
        {
            _logger.LogError($"Can't read from Dataverse. {e.Message}");
            throw new DataverseConnectionException("Can't read from Dataverse.");
        }
        
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
    
    public async Task<Guid> AddTimeEntryEntityAsync(
        TimeEntryModel model, 
        CancellationToken token)
    {
        var newEntity = new Entity(TimeEntryTableName);
        newEntity[TimeEntryStartFieldName] =model.Start;
        newEntity[TimeEntryEndFieldName] = model.End;

        try
        {
            var id = await _dataverseServiceClient.CreateAsync(newEntity, token);
            _logger.LogInformation($"New TimeEntry was created with Id: {id}");
            return id;
        }
        catch (Exception e)
        {
            _logger.LogError($"Can't write to Dataverse. {e.Message}");
            throw new DataverseConnectionException("Can't write to Dataverse.");
        }
    }
}