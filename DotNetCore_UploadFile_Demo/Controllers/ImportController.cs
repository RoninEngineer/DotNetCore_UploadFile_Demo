using DotNetCore_UploadFile_Demo.Attributes;
using DotNetCore_UploadFile_Demo.Engine;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

using System;
using System.Text;
using System.Threading.Tasks;

namespace DotNetCore_UploadFile_Demo.Controllers
{
    [Route("api/import")]

    public class ImportController : Controller
    {
        private readonly IImportEngine engine;
        public ImportController(IImportEngine _engine)
        {
            engine = _engine;
        }


        [HttpPost("Products")]
        [DisableFormModelValueBinding]
        public async Task<IActionResult> ImportData()
        {
            try
            {
                if(!engine.IsMultipartContent(Request))
                {
                    return BadRequest($"Request is {Request.ContentType} not a multipart request");
                }

                var filePath = await engine.ValidateRequest(Request);
                if(String.IsNullOrEmpty(filePath))
                {
                    return BadRequest($"Unable to validate data in request");
                }
                var processedCount = engine.ProcessFile(filePath);

                return Ok($"Processed {processedCount} rows from Product Data Import file");
            }
            catch (Exception ex)
            {
                return Content($"Internal Server Error while processing request : {ex}");
            }
        }

        

    }
}
