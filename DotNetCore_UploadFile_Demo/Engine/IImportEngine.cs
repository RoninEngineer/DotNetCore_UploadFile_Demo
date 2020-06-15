using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCore_UploadFile_Demo.Engine
{
    public interface IImportEngine
    {
        bool IsMultipartContent(HttpRequest request);
        Task<int> ProcessFile(string filePath);
        Task<string> ValidateRequest(HttpRequest request);
    }
}
