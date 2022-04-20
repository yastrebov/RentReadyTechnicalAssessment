using System.IO;
using AddTimeEntry;
using AddTimeEntryServices.Services;
using DAL;
using DAL.Repository;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.PowerPlatform.Dataverse.Client;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AddTimeEntry;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
#if DEBUG
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
#endif

#if RELEASE
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("release.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
#endif
        
        builder.Services.AddLogging();

        //services
        builder.Services.AddTransient<ITimeEntryService, TimeEntryService>();

        //repo
        builder.Services.AddTransient<IDataverseRepository, DataverseRepository>();

        //settings
        DataverseSettings dataverseSettings = config
            .GetSection("DataverseSettings")
            .Get<DataverseSettings>();

        var connectionString =
            $"AuthType=OAuth;Url={dataverseSettings.Url};UserName={dataverseSettings.AppUser};Password={dataverseSettings.Password};";
        builder.Services.AddSingleton(new ServiceClient(connectionString));
    }
}