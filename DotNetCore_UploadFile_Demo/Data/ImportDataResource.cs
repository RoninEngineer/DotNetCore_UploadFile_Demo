﻿using DotNetCore_UploadFile_Demo.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;


namespace DotNetCore_UploadFile_Demo.Data
{
    public class ImportDataResource : IImportDataResource
    {
        public async Task<string> ValidateImportRequestData(HttpRequest request)
        {
            FormOptions _defaultFormOptions = new FormOptions();
            var accumulator = new KeyValueAccumulator();
            string filePath = Path.GetTempFileName();

            var boundary = MultipartRequest.GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out ContentDispositionHeaderValue contentDisposition);

                if (hasContentDispositionHeader)
                {
                    if (MultipartRequest.HasFileContentDisposition(contentDisposition))
                    {
                        using (var targetStream = File.Create(filePath))
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
            return filePath;
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
