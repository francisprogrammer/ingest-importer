namespace WebApi.Common
{
    public interface IHttpClientFactory
    {
        IHttpClient Create();
    }
}