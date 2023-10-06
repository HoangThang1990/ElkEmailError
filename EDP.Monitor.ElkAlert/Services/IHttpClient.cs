public interface IHttpClient
{
    Task<HttpResponseMessage> GetAsync(string uri, Dictionary<string, string> headers = null);
    Task<T> PostAsync<T>(string uri, object data = null, Dictionary<string, string> headers = null);
}