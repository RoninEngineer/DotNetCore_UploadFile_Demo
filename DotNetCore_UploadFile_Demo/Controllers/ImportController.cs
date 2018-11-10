using DotNetCore_UploadFile_Demo.Attributes;
using DotNetCore_UploadFile_Demo.Engine;
using DotNetCore_UploadFile_Demo.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                FormOptions _defaultFormOptions = new FormOptions();
                if(!engine.IsMultipartContent(Request))
                {
                    return BadRequest($"Request is {Request.ContentType} not a multipart request");
                }

                var accumulator = new KeyValueAccumulator();
                string filePath = Path.GetTempFileName();

                var boundary = MultipartRequest.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary, HttpContext.Request.Body);
                var section = await reader.ReadNextSectionAsync();

                while (section != null)
                {
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

                    if (hasContentDispositionHeader)
                    {
                        if (MultipartRequest.HasFileContentDisposition(contentDisposition))
                        {
                            using (var targetStream = System.IO.File.Create(filePath))
                            {
                                await section.Body.CopyToAsync(targetStream);
                            }
                        }
                        else if (MultipartRequest.HasFormDataContentDisposition(contentDisposition))
                        {
                            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).ToString();
                            var encoding = GetEncoding(section);
                            using (var streamReader = new StreamReader(
                                                        section.Body,
                                                        encoding,
                                                        detectEncodingFromByteOrderMarks: true,
                                                        bufferSize: 1024,
                                                        leaveOpen: true))
                            {
                                var value = await streamReader.ReadToEndAsync();
                                if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = String.Empty;
                                }
                                accumulator.Append(key, value);

                                if (accumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                                {
                                    throw new InvalidDataException($"For key count limit {_defaultFormOptions.ValueCountLimit} exceeded");
                                }
                            }
                        }
                    }
                    section = await reader.ReadNextSectionAsync();
                }

                var processedCount = engine.ProcessFile(filePath);

                return Ok($"Processed {processedCount} rows from Product Data Import file");
            }
            catch (Exception)
            {
                return Content("Bad");
            }
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out MediaTypeHeaderValue mediaType);
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }
            return mediaType.Encoding;
        }

    }
}
