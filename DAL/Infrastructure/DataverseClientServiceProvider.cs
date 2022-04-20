using DAL.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace DAL.Infrastructure;

public interface IDataverseClientServiceProvider
{
    ServiceClient GetDataverceServiceClient();
}
public class DataverseClientServiceProvider: IDataverseClientServiceProvider
{
    private readonly DataverseSettings _dataverseSettings;
    private readonly ILogger<DataverseClientServiceProvider> _logger;

    public DataverseClientServiceProvider(
        DataverseSettings dataverseSettings,
        ILogger<DataverseClientServiceProvider> logger)
    {
        _dataverseSettings = dataverseSettings;
        _logger = logger;
    }
    
    public ServiceClient GetDataverceServiceClient()
    {
        if (string.IsNullOrEmpty(_dataverseSettings.AppUser))
            throw new ArgumentException("Dataverse AppUser is empty");
        if (string.IsNullOrEmpty(_dataverseSettings.Password))
            throw new ArgumentException("Dataverse Password is empty");
        if (string.IsNullOrEmpty(_dataverseSettings.Url))
            throw new ArgumentException("Dataverse Url is empty");

        var connectionString =
            $"AuthType=OAuth;Url={_dataverseSettings.Url};UserName={_dataverseSettings.AppUser};Password={_dataverseSettings.Password};";
        try
        {
            return new ServiceClient(connectionString, _logger);
        }
        catch (Exception e)
        {
            _logger.LogError($"Can't connect to Dataverse. {e.Message}");
            throw new DataverseConnectionException("Can't connect to Dataverse.");
        }
    }
}