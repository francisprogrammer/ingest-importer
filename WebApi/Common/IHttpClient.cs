using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Common
{
    public interface IHttpClient : IDisposable
    {
        Task SaveAsync<TObject>(string url, TObject @object);
    }
}