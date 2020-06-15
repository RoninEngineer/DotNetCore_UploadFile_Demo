using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DotNetCore_UploadFile_Demo.Data
{
    public interface IImportDataResource
    {
        Task<string> ValidateImportRequestData(HttpRequest request);
    }
}
