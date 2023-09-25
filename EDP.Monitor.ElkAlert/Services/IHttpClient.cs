using System.Text;
using Newtonsoft.Json;

public class StandardHttpClient : IHttpClient
{
     private readonly ILogger<StandardHttpClient> _logger;

    public StandardHttpClient(ILogger<StandardHttpClient> logger)
    {
        _logger = logger;
    }

    public async Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers = null)
    {
        try
        {
            using(var handler = new HttpClientHandler(){
            CookieContainer = new System.Net.CookieContainer(),
            })
            {
                using(var client = new HttpClient(handler))
                {
                    client.BaseAddress= new Uri(uri);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    if(headers != null && headers.Count > 0){
                        foreach(var header in headers){
                            if(string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value)) continue;
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            
                        }
                    }

                    return await client.GetAsync(new Uri(uri));
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return null;
    }

    public async Task<T> PostAsync<T>(string uri, object data, Dictionary<string, string> headers = null)
    {
        T result = Activator.CreateInstance<T>();
        try
        {
            using(var handler = new HttpClientHandler(){
            CookieContainer = new System.Net.CookieContainer(),
            })
            {
                using(var client = new HttpClient(handler))
                {
                    client.BaseAddress= new Uri(uri);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    if(headers != null && headers.Count > 0){
                        foreach(var header in headers){
                            if(string.IsNullOrEmpty(header.Key) || string.IsNullOrEmpty(header.Value)) continue;
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            
                        }
                    }

                    var response =  await client.SendAsync(new HttpRequestMessage(){
                        Method = HttpMethod.Post,
                        Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
                    });

                    if(response != null){
                        var json = await response.Content.ReadAsStringAsync();
                        if(!string.IsNullOrEmpty(json)){
                            result = JsonConvert.DeserializeObject<T>(json);
                        }
                    }
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return result;
    }

}

public interface IHttpClient{
    Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string,string> headers = null);
    Task<T> PostAsync<T>(string uri, object data = null, Dictionary<string, string> headers = null);
}