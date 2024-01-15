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
using System.Collections.Specialized;

namespace TryOSS.Services
{
    public class OSSService
    {
        private readonly string accessKey;
        private readonly string secretKey;
        private readonly string bucketName;
        private readonly string region;

        public OSSService()
        {
            bucketName = ConfigurationManager.AppSettings["BucketName"];
            region = ConfigurationManager.AppSettings["Region"];
            accessKey = ConfigurationManager.AppSettings["AccessKey"];
            secretKey = ConfigurationManager.AppSettings["SecretKey"];
        }

        public async Task GetObjectTagging(string objectName, List<NameValueCollection> ossHeaders = null)
        {
            try
            {
                var requestModel = new ObjectTaggingRequestModel
                {
                    AccessKey = accessKey,
                    SecretKey = secretKey,
                    Region = region,
                    BucketName = bucketName,
                    ObjectName = objectName,
                    OssHeaders = ossHeaders
                };
                var request = OSSAPIHelper.GenerateGetObjectTaggingRequest(requestModel);

                using var client = new HttpClient();
                var response = await client.SendAsync(request);
                await ProcessResponse(response);
            }
            catch (HttpResponseException ex)
            {
                Debug.WriteLine($"HTTP StatusCode: {ex.StatusCode}");
                Debug.WriteLine($"ResponseBody:\n{ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
                throw;
            }
        }

        public async Task PutObjectTagging(string objectName, List<OSSAPIHelper.Tag> tags, List<NameValueCollection> ossHeaders = null)
        {
            try
            {
                var requestModel = new ObjectTaggingRequestModel
                {
                    AccessKey = accessKey,
                    SecretKey = secretKey,
                    Region = region,
                    BucketName = bucketName,
                    ObjectName = objectName,
                    OssHeaders = ossHeaders
                };
                var request = OSSAPIHelper.GeneratePubObjectTaggingRequest(requestModel, tags);

                using var client = new HttpClient();
                var response = await client.SendAsync(request);
                await ProcessResponse(response);
            }
            catch (HttpResponseException ex)
            {
                Debug.WriteLine($"HTTP StatusCode: {ex.StatusCode}");
                Debug.WriteLine($"ResponseBody:\n{ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
                throw;
            }
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
                throw new HttpResponseException(response.StatusCode, responseBody);
            }
        }
    }
}
