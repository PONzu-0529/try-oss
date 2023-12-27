using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TryOSS.Models
{
    public class ObjectTaggingRequestModel
    {
        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public string Region { get; set; }

        public string BucketName { get; set; }

        public string ObjectName { get; set; }

        public List<NameValueCollection> OssHeaders { get; set; }
    }
}
