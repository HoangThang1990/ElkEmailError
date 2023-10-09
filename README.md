Hướng dẫn cấu hình
1. Thiết lập email server (https://wiki.matbao.net/kb/thong-tin-smtp-gmail-cach-cau-hinh-smtp-gmail-free-vao-wordpress/)
bổ sung vào EmailConfig trong appsetting.json
2. Vào elk, thực hiện query theo yêu cầu của dự án để lấy được RealtimeReportUrl
3. Save lại query và lấy link post save báo cáo 
4. Thực hiện thay đổi lại thời gian trên báo cáo cho phép truyền động strict_date_optional_time theo format gte:%27{0}%27,lte:%27{1}%27 trong đó {0},{1} tương ứng fromtime (thời gian hiện tại - Interval), totime (thời gian hiện tại) và cập nhật vào ReportUrl
Ví dụ
    "ReportUrl": "...(format:strict_date_optional_time,gte:%27{0}%27,lte:%27{1}%27))))...",
5. Bổ sung khối job vào trong appsetting.json
Ví dụ
"EMISHomework": { //Tên job
      "Enabled": true,  //Có enable hay không
      //"CronExpression": "0 0 10,17,22 * * ?",
      "CronExpression": "0 0/3 * * * ?", //Cấu hình thời gian chạy, xem sample ở đây http://www.quartz-scheduler.org/documentation/quartz-2.3.0/tutorials/crontrigger.html
      "JobData": {
        //Tính theo giờ, map tương ứng thời gian config trong cron expression
        "Interval": 6, //Thời gian lấy dữ liệu. Ví dụ 6 tiếng trước
        "EmailTo": "hnthang@software.misa.com.vn",  //Gửi mail cho ai
        "EmailCc": "hnthang@software.misa.com.vn",  //CC cho ai
        "EmailErrorJobTo": "hnthang@software.misa.com.vn",  //Gửi mail lỗi cho ai
        "Subject": "[ELKError] {2} hits lỗi EMIS Ôn tập từ {0} đến {1}",    //Tiêu đề email
        "Body": "<b>{2}</b> hits lỗi <b>EMIS Ôn tập</b> từ <b>{0}</b> đến <b>{1}</b>. Xem chi tiết trong file đính kèm. Trường hợp file đính kèm bị lỗi hoặc không có có thể xem trực tiếp tại <a href='{3}'>đây</a>",  //Body format
        "DelayTime": 60,
        "ReportUrl": "https://elksoft.misaonline.vpnlocal/s/misa-emis/api/reporting/generate/csv?jobParams=(conflictedTypesFields:!(),fields:!(%27@timestamp%27,exception,message,request-url,system_id),indexPatternId:f218afe0-321c-11ec-9b6e-1776f9e402a9,metaFields:!(_source,_id,_type,_index,_score),searchRequest:(body:(_source:(excludes:!(),includes:!(%27@timestamp%27,exception,message,request-url,system_id)),docvalue_fields:!((field:%27@timestamp%27,format:date_time)),query:(bool:(filter:!((match_all:()),(match_phrase:(app_id:emishomework)),(match_phrase:(level:Error)),(range:(%27@timestamp%27:(format:strict_date_optional_time,gte:%27{0}%27,lte:%27{1}%27)))),must:!(),must_not:!(),should:!())),script_fields:(),sort:!((%27@timestamp%27:(order:desc,unmapped_type:boolean))),stored_fields:!(%27@timestamp%27,exception,message,request-url,system_id),version:!t),index:%27app-logs-*%27),title:logs-emishomework-last-1hour,type:search)",
        "DownloadReportUrl": "https://elksoft.misaonline.vpnlocal/s/misa-emis{0}",  //Link download
        "RealtimeReportUrl": "https://elksoft.misaonline.vpnlocal/s/misa-emis/app/kibana#/discover/b14d6200-4386-11ec-9b6e-1776f9e402a9?_g=(filters:!(),refreshInterval:(pause:!f,value:10000),time:(from:now%2Fd,to:now%2Fd))&_a=(columns:!(exception,message,request-url,system_id),filters:!((&#39;$state&#39;:(store:appState),meta:(alias:!n,disabled:!f,index:f218afe0-321c-11ec-9b6e-1776f9e402a9,key:app_id,negate:!f,params:(query:emishomework),type:phrase),query:(match_phrase:(app_id:emishomework))),(&#39;$state&#39;:(store:appState),meta:(alias:!n,disabled:!f,index:f218afe0-321c-11ec-9b6e-1776f9e402a9,key:level,negate:!f,params:(query:Error),type:phrase),query:(match_phrase:(level:Error)))),index:f218afe0-321c-11ec-9b6e-1776f9e402a9,interval:auto,query:(language:kuery,query:&#39;&#39;),sort:!(!(&#39;@timestamp&#39;,desc)))", //Link xem realtime ở bước 1
        "ELKUser": "",   //Tài khoản elk
        "ELKPassword": ""
      }
    }
  }
6. Cấu hình lại ELKUser, ELKPassword tương ứng
  