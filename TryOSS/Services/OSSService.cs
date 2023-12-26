using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TryOSS.Helpers;
using TryOSS.Models;

namespace TryOSS.Services
{
    public class OSSService
    {
        private readonly string bucketName;
        private readonly string region;
        private readonly string objectName;
        private readonly string accessKey;
        private readonly string secretKey;

        public OSSService()
        {
            bucketName = ConfigurationManager.AppSettings["BucketName"];
            region = ConfigurationManager.AppSettings["Region"];
            objectName = ConfigurationManager.AppSettings["ObjectName"];
            accessKey = ConfigurationManager.AppSettings["AccessKey"];
            secretKey = ConfigurationManager.AppSettings["SecretKey"];
        }

        public async Task GetObjectTagging()
        {
            var date = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");
            var canonicalizedResource = $"/{bucketName}/{objectName}?tagging";
            var signature = GenerateSignature(HttpMethod.Get, date, canonicalizedResource);

            using var client = new HttpClient();
            var request = CreateHttpRequest(HttpMethod.Get, date, canonicalizedResource, signature);

            var response = await client.SendAsync(request);
            await ProcessResponse(response);
        }

        public async Task PutObjectTagging()
        {
            var date = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");
            var contentType = "application/xml";
            var canonicalizedResource = $"/{bucketName}/{objectName}?tagging";
            var signature = GenerateSignature(HttpMethod.Put, date, canonicalizedResource);

            using var client = new HttpClient();
            var request = CreateHttpRequest(HttpMethod.Put, date, canonicalizedResource, signature);

            AddTaggingContent(request, contentType);

            var response = await client.SendAsync(request);
            await ProcessResponse(response);
        }

        private string GenerateSignature(HttpMethod httpMethod, string date, string canonicalizedResource)
        {
            var generateSignatureRequest = new GenerateSignatureRequestModel()
            {
                AccessKeySecret = secretKey,
                Verb = httpMethod.ToString(),
                ContentMD5 = "",
                ContentType = httpMethod == HttpMethod.Put ? "application/xml" : "",
                Date = date,
                CanonicalizedOSSHeaders = "",
                CanonicalizedResource = canonicalizedResource
            };

            return OSSAPIHelper.GenerateSignature(generateSignatureRequest);
        }

        private HttpRequestMessage CreateHttpRequest(HttpMethod httpMethod, string date, string canonicalizedResource, string signature)
        {
            var request = new HttpRequestMessage(httpMethod, $"https://{bucketName}.{region}.aliyuncs.com/{objectName}?tagging");
            request.Headers.Add("Authorization", $"OSS {accessKey}:{signature}");
            request.Headers.Add("Date", date);
            return request;
        }

        private void AddTaggingContent(HttpRequestMessage request, string contentType)
        {
            request.Content = new StringContent($"<Tagging><TagSet><Tag><Key>RegisterDate</Key><Value>{DateTime.Now.ToString("yyyyMMddHHmmss")}</Value></Tag></TagSet></Tagging>");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        }

        private async Task ProcessResponse(HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"HTTP StatusCode: {response.StatusCode}");
                Debug.WriteLine($"ResponseBody:\n{responseBody}");
            }
            else
            {
                Debug.WriteLine($"HTTP StatusCode: {response.StatusCode}, Error Message: {responseBody}");
            }
        }
    }
}
