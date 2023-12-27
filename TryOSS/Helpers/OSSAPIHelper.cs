using System;
using System.Collections.Generic;
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
                date: date);

            var request = CreateHttpRequest(
                requestModel: model,
                httpMethod: httpMethod,
                date: date,
                signature: signature);

            return request;
        }

        public static HttpRequestMessage GeneratePubObjectTaggingRequest(ObjectTaggingRequestModel model, List<Tag> tags)
        {
            var date = DateTime.UtcNow;
            var httpMethod = HttpMethod.Put;
            var contentType = "application/xml";

            var signature = GenerateSignature(
                requestModel: model,
                verb: httpMethod,
                date: date,
                contentType: contentType,
                contentMD5: "");

            var request = CreateHttpRequest(
                requestModel: model,
                httpMethod: httpMethod,
                date: date,
                signature: signature);

            AddTaggingContent(request, contentType, tags);

            return request;
        }
        private static string GenerateSignature(
            ObjectTaggingRequestModel requestModel,
            HttpMethod verb,
            DateTime date,
            string contentMD5 = "",
            string contentType = "",
            string canonicalizedOSSHeaders = "")
        {
            // Construct the string to sign
            string stringToSign = $"{verb}\n{contentMD5}\n{contentType}\n{date:ddd, dd MMM yyyy HH:mm:ss 'GMT'}\n{canonicalizedOSSHeaders}/{requestModel.BucketName}/{requestModel.ObjectName}?tagging";

            // Generate the HMAC-SHA1 signature
            using var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(requestModel.SecretKey));
            byte[] signatureBytes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

            // Base64 encode the signature
            return Convert.ToBase64String(signatureBytes);
        }

        private static HttpRequestMessage CreateHttpRequest(
            ObjectTaggingRequestModel requestModel,
            HttpMethod httpMethod,
            DateTime date,
            string signature)
        {
            var request = new HttpRequestMessage(httpMethod, $"https://{requestModel.BucketName}.{requestModel.Region}.aliyuncs.com/{requestModel.ObjectName}?tagging");

            request.Headers.Add("Authorization", $"OSS {requestModel.AccessKey}:{signature}");
            request.Headers.Add("Date", date.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'"));

            return request;
        }

        private static void AddTaggingContent(HttpRequestMessage request, string contentType, List<Tag> tags)
        {
            string xmlContent = ConvertListToXml(tags);

            request.Content = new StringContent(xmlContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }

        private static string ConvertListToXml(List<Tag> tags)
        {
            var tagging = new Tagging { TagSet = tags };

            var serializer = new XmlSerializer(typeof(Tagging));

            using StringWriter stringWriter = new StringWriter();
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
