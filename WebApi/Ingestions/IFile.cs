using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApi.Ingestions
{
    public interface IFile
    {
        Task<string> GetTempFilePath(IFormFile formFile);
    }
}