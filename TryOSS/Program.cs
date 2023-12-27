using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryOSS.Helpers;
using TryOSS.Services;

namespace TryOSS
{
    public class Program
    {
        public static async Task Main()
        {
            var objectName = ConfigurationManager.AppSettings["ObjectName"];
            var tags = new List<OSSAPIHelper.Tag>
            {
                new OSSAPIHelper.Tag { Key = "RegisterDate", Value = DateTime.Now.ToString("yyyyMMddHHmmss") }
            };
            var ossHeaders = new List<NameValueCollection>()
            {
                new NameValueCollection { { "X-OSS-Custom-A", "A" } },
                new NameValueCollection { { "X-OSS-Custom-B", "B" } }
            };

            var service = new OSSService();

            await service.PutObjectTagging(objectName, tags);
            await service.GetObjectTagging(objectName);

            await service.PutObjectTagging(objectName, tags, ossHeaders);
            await service.GetObjectTagging(objectName, ossHeaders);
        }
    }
}
