using System.IO;
using AddTimeEntry;
using AddTimeEntryServices.Services;
using DAL;
using DAL.Infrastructure;
using DAL.Repository;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
        builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();

        //repo
        builder.Services.AddScoped<IDataverseRepository, DataverseRepository>();
        
        //providers
        builder.Services.AddScoped<IDataverseClientServiceProvider, DataverseClientServiceProvider>(); 

        //settings
        DataverseSettings dataverseSettings = config
            .GetSection("DataverseSettings")
            .Get<DataverseSettings>();
        builder.Services.AddSingleton(dataverseSettings);
    }
}