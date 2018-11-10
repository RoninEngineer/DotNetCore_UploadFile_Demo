using Microsoft.Net.Http.Headers;
using System;
using System.IO;

namespace DotNetCore_UploadFile_Demo.Attributes
{
    public class MultipartRequest
    {
        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).ToString();
            if(string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundry");
            }

            if(boundary.Length > lengthLimit)
            {
                throw new InvalidDataException($"Multipart boundry length limit { lengthLimit } exceeded");
            }
            return boundary;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase)>= 0 ;
        }

        public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && string.IsNullOrEmpty(contentDisposition.FileName.ToString())
                && string.IsNullOrEmpty(contentDisposition.FileNameStar.ToString());
        }

        public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
        {
            return contentDisposition != null
                && contentDisposition.DispositionType.Equals("form-data")
                && (!string.IsNullOrEmpty(contentDisposition.FileName.ToString())
                || !string.IsNullOrEmpty(contentDisposition.FileNameStar.ToString()));
        }
    }
}
