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
        private readonly string bucketName = ConfigurationManager.AppSettings["BucketName"];
        private readonly string region = ConfigurationManager.AppSettings["Region"];
        private readonly string objectName = ConfigurationManager.AppSettings["ObjectName"];
        private readonly string accessKey = ConfigurationManager.AppSettings["AccessKey"];

        public async Task GetObjectTagging()
        {
            var date = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");

            var generateSignatureRequest = new GenerateSignatureRequestModel()
            {
                AccessKeySecret = ConfigurationManager.AppSettings["SecretKey"],
                Verb = HttpMethod.Get.ToString(),
                ContentMD5 = "",
                ContentType = "",
                Date = date,
                CanonicalizedOSSHeaders = "",
                CanonicalizedResource = $"/{bucketName}/{objectName}?tagging"
            };

            var signature = OSSAPIHelper.GenerateSignature(generateSignatureRequest);

            using var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Get, $"https://{bucketName}.{region}.aliyuncs.com/{objectName}?tagging");

            request.Headers.Add("Authorization", $"OSS {accessKey}:{signature}");
            request.Headers.Add("Date", date);

            var response = await client.SendAsync(request);
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

        public async Task PutObjectTagging()
        {
            var contentType = "application/xml";
            var date = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");

            var generateSignatureRequest = new GenerateSignatureRequestModel()
            {
                AccessKeySecret = ConfigurationManager.AppSettings["SecretKey"],
                Verb = HttpMethod.Put.ToString(),
                ContentMD5 = "",
                ContentType = contentType,
                Date = date,
                CanonicalizedOSSHeaders = "",
                CanonicalizedResource = $"/{bucketName}/{objectName}?tagging"
            };

            var signature = OSSAPIHelper.GenerateSignature(generateSignatureRequest);

            using var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Put, $"https://{bucketName}.{region}.aliyuncs.com/{objectName}?tagging");

            request.Headers.Add("Authorization", $"OSS {accessKey}:{signature}");
            request.Headers.Add("Date", date);

            request.Content = new StringContent($"<Tagging><TagSet><Tag><Key>RegisterDate</Key><Value>{DateTime.Now.ToString("yyyyMMddHHmmss")}</Value></Tag></TagSet></Tagging>");
            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"HTTP StatusCode: {response.StatusCode}");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"HTTP StatusCode: {response.StatusCode}, Error Message: {errorMessage}");
            }
        }
    }
}
