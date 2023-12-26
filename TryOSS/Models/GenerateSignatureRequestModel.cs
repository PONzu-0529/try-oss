using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TryOSS.Models
{
    public class GenerateSignatureRequestModel
    {
        public string AccessKeySecret { get; set; }
        public string Verb { get; set; }
        public string ContentMD5 { get; set; }
        public string ContentType { get; set; }
        public string Date { get; set; }
        public string CanonicalizedOSSHeaders { get; set; }
        public string CanonicalizedResource { get; set; }
    }
}
