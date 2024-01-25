using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TryOSS.Models;

namespace TryOSS.Helpers
{
    public class OSSAPIHelper
    {
        public static HttpRequestMessage GenerateGetObjectTaggingRequest(ObjectTaggingRequestModel model)
        {
            return CreateHttpRequest(
                requestModel: model,
                httpMethod: HttpMethod.Get,
                date: DateTime.UtcNow,
                ossHeaders: model.OssHeaders);
        }

        public static HttpRequestMessage GeneratePubObjectTaggingRequest(ObjectTaggingRequestModel model, List<Tag> tags)
        {
            return CreateHttpRequest(
                requestModel: model,
                httpMethod: HttpMethod.Put,
                date: DateTime.UtcNow,
                tags: tags,
                contentType: "application/xml",
                ossHeaders: model.OssHeaders);
        }

        private static HttpRequestMessage CreateHttpRequest(
            ObjectTaggingRequestModel requestModel,
            HttpMethod httpMethod,
            DateTime date,
            List<Tag> tags = null,
            string contentType = "",
            List<NameValueCollection> ossHeaders = null)
        {
            var signature = GenerateSignature(
                requestModel: requestModel,
                verb: httpMethod,
                date: date,
                tags: tags,
                contentType: contentType,
                ossHeaders: ossHeaders);

            var request = new HttpRequestMessage(httpMethod, $"http://{requestModel.Endpoint}/{requestModel.BucketName}/{requestModel.ObjectName}?tagging");

            request.Headers.Add("Authorization", $"OSS {requestModel.AccessKey}:{signature}");
            request.Headers.Add("Date", date.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture));

            // Add Custom X-OSS Headers
            if (ossHeaders != null)
            {
                foreach (var headers in ossHeaders)
                {
                    foreach (var key in headers.AllKeys)
                    {
                        var value = headers[key];
                        request.Headers.Add(key, value);
                    }
                }
            }

            AddTaggingContent(request, contentType, tags);

            return request;
        }

        private static string GenerateSignature(
            ObjectTaggingRequestModel requestModel,
            HttpMethod verb,
            DateTime date,
            List<Tag> tags = null,
            string contentType = "",
            List<NameValueCollection> ossHeaders = null)
        {
            var contentMD5 = CalculateMd5Hash(tags);
            var canonicalizedOSSHeaders = ConvertToOSSHeaders(ossHeaders);

            // Construct the string to sign
            var stringToSign = $"{verb}\n{contentMD5}\n{contentType}\n{date.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'", CultureInfo.InvariantCulture)}\n{canonicalizedOSSHeaders}/{requestModel.BucketName}/{requestModel.BucketName}/{requestModel.ObjectName}?tagging";

            // Generate the HMAC-SHA1 signature
            using var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(requestModel.SecretKey));
            var signatureBytes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

            // Base64 encode the signature
            return Convert.ToBase64String(signatureBytes);
        }

        private static string ConvertToOSSHeaders(List<NameValueCollection> ossHeaders)
        {
            if (ossHeaders == null)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            foreach (var headers in ossHeaders)
            {
                foreach (var key in headers.AllKeys)
                {
                    var value = headers[key];
                    stringBuilder.Append($"{key.ToLower()}:{value}\n");
                }
            }

            return stringBuilder.ToString();
        }

        private static void AddTaggingContent(
            HttpRequestMessage request,
            string contentType,
            List<Tag> tags)
        {
            if (tags == null)
            {
                return;
            }

            var xmlContent = ConvertListToXml(tags);

            request.Content = new StringContent(xmlContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            request.Content.Headers.ContentMD5 = Convert.FromBase64String(CalculateMd5Hash(tags));
        }

        private static string ConvertListToXml(List<Tag> tags)
        {
            var tagging = new Tagging { TagSet = tags };

            var serializer = new XmlSerializer(typeof(Tagging));

            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, tagging);
            return stringWriter.ToString();
        }

        private static string CalculateMd5Hash(List<Tag> tags)
        {
            if (tags == null)
            {
                return string.Empty;
            }

            var xmlContent = ConvertListToXml(tags);

            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(xmlContent));
            return Convert.ToBase64String(hashBytes);
        }

        public class Tag
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        public class Tagging
        {
            public List<Tag> TagSet { get; set; }
        }
    }
}
