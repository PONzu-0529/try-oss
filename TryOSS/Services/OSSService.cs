using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TryOSS.Helpers;
using TryOSS.Models;
using System.Xml.Serialization;

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
            var requestModel = new ObjectTaggingRequestModel
            {
                AccessKey = accessKey,
                SecretKey = secretKey,
                Region = region,
                BucketName = bucketName,
                ObjectName = objectName
            };
            var request = OSSAPIHelper.GenerateGetObjectTaggingRequest(requestModel);

            using var client = new HttpClient();
            var response = await client.SendAsync(request);
            await ProcessResponse(response);
        }

        public async Task PutObjectTagging()
        {
            var requestModel = new ObjectTaggingRequestModel
            {
                AccessKey = accessKey,
                SecretKey = secretKey,
                Region = region,
                BucketName = bucketName,
                ObjectName = objectName
            };
            var tags = new List<OSSAPIHelper.Tag>
            {
                new OSSAPIHelper.Tag { Key = "RegisterDate", Value = DateTime.Now.ToString("yyyyMMddHHmmss") }
            };
            var request = OSSAPIHelper.GeneratePubObjectTaggingRequest(requestModel, tags);

            using var client = new HttpClient();
            var response = await client.SendAsync(request);
            await ProcessResponse(response);
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
