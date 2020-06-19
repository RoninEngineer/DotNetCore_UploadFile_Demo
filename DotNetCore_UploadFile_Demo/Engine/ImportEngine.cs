using DotNetCore_UploadFile_Demo.Attributes;
using DotNetCore_UploadFile_Demo.Data;
using DotNetCore_UploadFile_Demo.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace DotNetCore_UploadFile_Demo.Engine
{
    public class ImportEngine : IImportEngine
    {
        private IImportDataResource _importDataResource;

        public ImportEngine(IImportDataResource importDataResource)
        {
            _importDataResource = importDataResource;
        }
        public bool IsMultipartContent(HttpRequest request)
        {
            try
            {
                if (!MultipartRequest.IsMultipartContentType(request.ContentType))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> ProcessFile(string filePath)
        {
            try
            {
                List<ProductImport> productImportList = new List<ProductImport>();
                string[] formData = await File.ReadAllLinesAsync(filePath);

                var queryDataItem = from line in formData.Skip(1)
                                    select line;

                foreach (var product in queryDataItem)
                {
                    var productData = product.Split(',');

                    productImportList.Add(new ProductImport
                    {
                        ProductId = int.TryParse(productData[0], out int validProductId) ? validProductId : 0,
                        ProductName = !String.IsNullOrWhiteSpace((string)productData.GetValue(1)) ? (string)productData.GetValue(1) : null,
                        ProductDescription = !String.IsNullOrWhiteSpace((string)productData.GetValue(2)) ? (string)productData.GetValue(2) : null,
                        ProductSKU = !String.IsNullOrWhiteSpace((string)productData.GetValue(3)) ? (string)productData.GetValue(3) : null

                    });
                }
                File.Delete(filePath);
                if (productImportList.Any())
                {
                    return productImportList.Count();
                }
                else
                {
                    return 0;
                }

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<string> ValidateRequest(HttpRequest request)
        {
            var requestDataFile = await _importDataResource.ValidateImportRequestData(request);
            return requestDataFile;
        }
    }
}
