using PetroIneos.PowerPositions;
using PetroIneos.PowerPositions.Config;
using PetroIneos.PowerPositions.Services;
using Microsoft.Extensions.Hosting;
using Serilog;
using Quartz;
using Services;

var builder = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        var config = hostContext.Configuration;
        services.Configure<PowerPositionsSettings>(config.GetSection("PowerPositionsSettings"));

        var intervalInMinutes = config.GetValue<int?>("PowerPositionsSettings:IntervalInMinutes")
                                ?? throw new InvalidOperationException("IntervalInMinutes is not configured in appsettings.");

        if (intervalInMinutes <= 0)
            throw new InvalidOperationException("IntervalInMinutes must be greater than 0");


        services.AddTransient<IPowerService, PowerService>(); // Add this line
        services.AddTransient<IPowerTradeService, PowerTradeService>();
        services.AddTransient<IPowerTradeAggregator, PowerTradeAggregator>();
        services.AddTransient<IPowerReportGeneratorService, PowerReportGeneratorService>();


        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("PowerPositionsJob");

            q.AddJob<PowerPositionsJob>(opts => opts.WithIdentity(jobKey));

            q.AddTrigger(x =>x
                .ForJob(jobKey)
                .WithIdentity("PowerPositionsTrigger")
                .StartNow()
                .WithSimpleSchedule(y =>y
                    .WithIntervalInMinutes(intervalInMinutes)
                    .RepeatForever()
                    .WithMisfireHandlingInstructionFireNow()));
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


    });

builder.UseSerilog((ctx, loggerConfig) =>
    loggerConfig
        .WriteTo.Console()
        .WriteTo.File("logs/powerpositions-.log", rollingInterval: RollingInterval.Day));

await builder.Build().RunAsync();

