using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TryOSS.Helpers;
using TryOSS.Models;

namespace TryOSS.Services
{
    public class OSSService
    {
        public async Task GetObjectTagging()
        {
            // 指定された情報
            var url = $"https://{ConfigurationManager.AppSettings["BucketName"]}.{ConfigurationManager.AppSettings["Region"]}.aliyuncs.com/{ConfigurationManager.AppSettings["ObjectName"]}?tagging";
            var accessKey = ConfigurationManager.AppSettings["AccessKey"];
            var date = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");

            var generateSignatureRequest = new GenerateSignatureRequestModel()
            {
                AccessKeySecret = ConfigurationManager.AppSettings["SecretKey"],
                Verb = "GET",
                ContentMD5 = "",
                ContentType = "",
                Date = date,
                CanonicalizedOSSHeaders = "",
                CanonicalizedResource = $"/{ConfigurationManager.AppSettings["BucketName"]}/{ConfigurationManager.AppSettings["ObjectName"]}?tagging"
            };

            var signature = OSSAPIHelper.GenerateSignature(generateSignatureRequest);

            // HttpClientのインスタンスを作成
            using HttpClient client = new HttpClient();
            // Authorizationヘッダーの作成
            string authorizationHeader = $"OSS {accessKey}:{signature}";

            // HTTPリクエストの作成
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", authorizationHeader);
            request.Headers.Add("Date", date);

            //// Content-Type ヘッダーの設定
            //request.Content = new StringContent("");
            //request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            // HTTPリクエストの送信
            HttpResponseMessage response = await client.SendAsync(request);

            // レスポンスの読み取り
            string responseBody = await response.Content.ReadAsStringAsync();

            // 結果の表示
            Debug.WriteLine($"HTTP StatusCode: {response.StatusCode}");
            Debug.WriteLine($"ResponseBody:\n{responseBody}");
        }
    }
}
