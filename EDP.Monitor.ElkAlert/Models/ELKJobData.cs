public class ELKJobData{
    public int Interval {get;set;}
    public string EmailTo {get;set;}
    public string EmailCC {get;set;}
    public string Subject {get;set;}
    public string Body {get;set;}
    public double DelayTime {get;set;}
    public string ReportUrl {get;set;}
    public string DownloadReportUrl {get;set;}
    public string RealtimeReportUrl {get;set;}
    public string ELKUser {get;set;}
    public string ELKPassword {get;set;}
    public string EmailErrorJobTo {get;set;}
}

public class ELKExportReportResponse{
    public string path {get;set;}
}

public class @timestamp {
    public DateTime gte {get;set;}
    public DateTime lte {get;set;}
}