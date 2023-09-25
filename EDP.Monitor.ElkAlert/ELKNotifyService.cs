
using Newtonsoft.Json;
using Quartz;
using Quartz.Spi;

namespace EDP.Monitor.ElkAlert;

public class ELKNotifyService : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IJobFactory _jobFactory;
    private readonly IConfiguration _configuration;
    public IScheduler Scheduler { get; set; } = null;
    private readonly ILogger<ELKNotifyService> _logger;

    public ELKNotifyService(
        ISchedulerFactory schedulerFactory,
        IConfiguration configuration,
        ILogger<ELKNotifyService> logger,
        IJobFactory jobFactory
    )
    {
        _schedulerFactory = schedulerFactory;
        _jobFactory = jobFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        Scheduler.JobFactory = _jobFactory;
        var elkNotifyJobSchedulers = _configuration.GetSection("ELKNotifyJobSchedules").Get<Dictionary<string, ELKNotifyJobSchedule>>();

        if (elkNotifyJobSchedulers != null)
        {
            foreach (var sc in elkNotifyJobSchedulers)
            {
                if (sc.Value.Enabled)
                {
                    sc.Value.Name = sc.Key;

                    try
                    {
                        var job = CreateJob(sc.Value, sc.Value.Name);
                        var trigger = CreateTrigger(sc.Value);
                        await Scheduler.ScheduleJob(job, trigger, cancellationToken);
                        _logger.LogInformation($"Scheduler schedule '{sc.Value.Name}' success");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Exception during schedule '{sc.Value.Name}': {ex.Message}");
                    }
                }
            }
        }

        await Scheduler.Start(cancellationToken);
    }

    private ITrigger CreateTrigger(ELKNotifyJobSchedule scheduler)
    {
        return TriggerBuilder
        .Create()
        .WithIdentity($"{scheduler.Name}.trigger")
        .WithCronSchedule(scheduler.CronExpression)
        .WithDescription(scheduler.CronExpression)
        .Build();
    }

    private IJobDetail CreateJob(ELKNotifyJobSchedule scheduler, string name)
    {
        return JobBuilder
        .Create(typeof(ELKNotifyJob))
        .UsingJobData("JobData", JsonConvert.SerializeObject(scheduler.JobData))
        .UsingJobData("JobName", name)
        .WithIdentity(scheduler.Name)
        .WithDescription(scheduler.Name)
        .Build();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Scheduler?.Shutdown(cancellationToken);
    }
}
