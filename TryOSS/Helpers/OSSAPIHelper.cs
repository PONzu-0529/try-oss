using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
            var date = DateTime.UtcNow;
            var httpMethod = HttpMethod.Get;

            var signature = GenerateSignature(
                requestModel: model,
                verb: httpMethod,
                date: date,
                ossHeaders: model.OssHeaders);

            var request = CreateHttpRequest(
                requestModel: model,
                httpMethod: httpMethod,
                date: date,
                signature: signature,
                ossHeaders: model.OssHeaders);

            return request;
        }

        public static HttpRequestMessage GeneratePubObjectTaggingRequest(ObjectTaggingRequestModel model, List<Tag> tags)
        {
            var date = DateTime.UtcNow;
            var httpMethod = HttpMethod.Put;
            var contentType = "application/xml";
            var contentMD5 = CalculateMd5Hash(tags);

            var signature = GenerateSignature(
                requestModel: model,
                verb: httpMethod,
                date: date,
                contentMD5: contentMD5,
                contentType: contentType,
                ossHeaders: model.OssHeaders);

            var request = CreateHttpRequest(
                requestModel: model,
                httpMethod: httpMethod,
                date: date,
                signature: signature,
                ossHeaders: model.OssHeaders);

            AddTaggingContent(request, contentType, tags);

            return request;
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

        private static string GenerateSignature(
            ObjectTaggingRequestModel requestModel,
            HttpMethod verb,
            DateTime date,
            string contentMD5 = "",
            string contentType = "",
            List<NameValueCollection> ossHeaders = null)
        {
            var canonicalizedOSSHeaders = ConvertToOSSHeaders(ossHeaders);

            // Construct the string to sign
            var stringToSign = $"{verb}\n{contentMD5}\n{contentType}\n{date:ddd, dd MMM yyyy HH:mm:ss 'GMT'}\n{canonicalizedOSSHeaders}/{requestModel.BucketName}/{requestModel.ObjectName}?tagging";

            // Generate the HMAC-SHA1 signature
            using var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(requestModel.SecretKey));
            var signatureBytes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

            // Base64 encode the signature
            return Convert.ToBase64String(signatureBytes);
        }

        private static HttpRequestMessage CreateHttpRequest(
            ObjectTaggingRequestModel requestModel,
            HttpMethod httpMethod,
            DateTime date,
            string signature,
            List<NameValueCollection> ossHeaders = null)
        {
            var request = new HttpRequestMessage(httpMethod, $"https://{requestModel.BucketName}.{requestModel.Region}.aliyuncs.com/{requestModel.ObjectName}?tagging");

            request.Headers.Add("Authorization", $"OSS {requestModel.AccessKey}:{signature}");
            request.Headers.Add("Date", date.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));

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

            return request;
        }

        private static void AddTaggingContent(
            HttpRequestMessage request, 
            string contentType, 
            List<Tag> tags)
        {
            var xmlContent = ConvertListToXml(tags);

            request.Content = new StringContent(xmlContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            request.Content.Headers.ContentMD5 = Convert.FromBase64String(CalculateMd5Hash(tags));
        }

        private static string CalculateMd5Hash(List<Tag> tags)
        {
            var xmlContent = ConvertListToXml(tags);

            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(xmlContent));
            return Convert.ToBase64String(hashBytes);
        }

        private static string ConvertListToXml(List<Tag> tags)
        {
            var tagging = new Tagging { TagSet = tags };

            var serializer = new XmlSerializer(typeof(Tagging));

            using var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, tagging);
            return stringWriter.ToString();
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
