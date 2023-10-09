using System.Text;
using Aspose.Cells;
using Newtonsoft.Json;
using Quartz;

public class ELKNotifyJob : IJob
{
    private readonly ILogger<ELKNotifyJob> _logger;
    private readonly IEmailService _emailService;
    private readonly IHttpClient _httpClient;

    public ELKNotifyJob(ILogger<ELKNotifyJob> logger,
            IHttpClient httpClient,
            IEmailService emailService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _emailService = emailService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var jobDataMap = context.JobDetail.JobDataMap;
            var jobDataStr = jobDataMap.GetString("JobData");
            var jobName = jobDataMap.GetString("JobName");

            if (!string.IsNullOrEmpty(jobDataStr) && !string.IsNullOrEmpty(jobName))
            {
                var jobData = JsonConvert.DeserializeObject<ELKJobData>(jobDataStr);
                if (jobData != null)
                {
                    var utcFromTime = DateTime.UtcNow.AddHours(-jobData.Interval);
                    var utcToTime = DateTime.UtcNow;

                    var fromTime = utcFromTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ");
                    var toTime = utcToTime.ToString("yyyy-MM-ddTHH:mm:ss.FFFZ");
                    _logger.LogInformation($"[ELKNotifyJob] Do get path");
                    var exportResponse = await _httpClient.PostAsync<ELKExportReportResponse>(
                        string.Format(jobData.ReportUrl, fromTime, toTime),
                        null,
                        new Dictionary<string, string>(){
                        { "kbn-xsrf", "true"},
                        { "Authorization", AuthBasicStr(jobData.ELKUser, jobData.ELKPassword)},
                        });
                    
                    if (!string.IsNullOrEmpty(exportResponse?.path))
                    {
                        _logger.LogInformation($"[ELKNotifyJob] Get data from {exportResponse?.path}");
                        try
                        {
                            var path = exportResponse.path;
                            await DownloadAndSendEmailReport(jobName, jobData, path, utcFromTime, utcToTime);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, ex.Message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    private string AuthBasicStr(string uname, string pwd)
    {
        byte[] utfBytes = Encoding.UTF8.GetBytes($"{uname}:{pwd}");
        var encoded = Convert.ToBase64String(utfBytes);
        return $"Basic {encoded}";
    }

    private async Task DownloadAndSendEmailReport(string jobName, ELKJobData jobData, string downloadUrl, DateTime fromTime, DateTime toTime, bool isRetry = false)
    {
        await Task.Delay(TimeSpan.FromSeconds(jobData.DelayTime));
        downloadUrl = string.Format(jobData.DownloadReportUrl, downloadUrl);
        var downloadResponse = await _httpClient.GetAsync(downloadUrl,
        new Dictionary<string, string>() { { "Authorization", AuthBasicStr(jobData.ELKUser, jobData.ELKPassword) } });

        if (downloadResponse.IsSuccessStatusCode)
        {
            var fileByte = await downloadResponse.Content.ReadAsByteArrayAsync();

            var ms = new MemoryStream(fileByte);
            ms.Position = 0;

            var wb = new Workbook(ms, new Aspose.Cells.LoadOptions(Aspose.Cells.LoadFormat.CSV));
            var ws = wb.Worksheets[0];
            var totalRow = ws.Cells.Rows.Count;

            if (totalRow > 0)
            {
                var fromTimeStr = fromTime.ToLocalTime().ToString(@"dd\/MM\/yyyy HH:mm");
                var toTimeStr = toTime.ToLocalTime().ToString(@"dd\/MM\/yyyy HH:mm");
                var count = totalRow - 1;
                var subject = string.Format(jobData.Subject, fromTimeStr, toTimeStr, count);
                var body = string.Format(jobData.Body, fromTime, toTimeStr, count, jobData.RealtimeReportUrl);

                //Gửi mail
                try
                {
                    _logger.LogInformation($"Send email of job {jobName} To: {jobData.EmailTo} - CC: {jobData.EmailCC} - Total rows: {count}");
                    await _emailService.SendEmailWithAttachmentAsync(
                     jobData.EmailTo,
                     jobData.EmailCC,
                     subject,
                     body,
                     ms);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

            }
        }
        else
        {
            if (isRetry)
            {
                _logger.LogError($"Download {downloadUrl} error status {downloadResponse.StatusCode}");

                //Gửi email lỗi
                try
                {
                    _logger.LogInformation($"Send email of job {jobName} To: {jobData.EmailErrorJobTo} - Error: {downloadResponse.ReasonPhrase}");
                    await _emailService.SendEmailWithAttachmentAsync(
                        jobData.EmailErrorJobTo,
                        string.Empty,
                        $"Lỗi download report {jobName}",
                        $"Lỗi download report {jobName} - {downloadResponse.ReasonPhrase}"
                    );

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }
            else
            {
                await DownloadAndSendEmailReport(jobName, jobData, downloadUrl, fromTime, toTime, true);
            }
        }
    }

}