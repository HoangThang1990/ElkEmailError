public class ELKNotifyJobSchedule
{
    public bool Enabled { get; set; }
    public string Name { get; set; }
    public string CronExpression {get;set;}
    public ELKJobData JobData {get;set;}
}
