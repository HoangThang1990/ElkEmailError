using EDP.Monitor.ElkAlert;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<IHttpClient, StandardHttpClient>();
        services.AddTransient<ELKNotifyJob>();
        services.AddSingleton<IJobFactory, JobFactory>();
        services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
        services.AddHostedService<ELKNotifyService>();
    })
    .Build();

host.Run();
