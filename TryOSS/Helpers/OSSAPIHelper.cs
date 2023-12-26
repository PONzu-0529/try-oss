using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TryOSS.Models;

namespace TryOSS.Helpers
{
    public class OSSAPIHelper
    {
        public static string GenerateSignature(GenerateSignatureRequestModel model)
        {
            // Construct the string to sign
            string stringToSign = $"{model.Verb}\n{model.ContentMD5}\n{model.ContentType}\n{model.Date}\n{model.CanonicalizedOSSHeaders}{model.CanonicalizedResource}";

            // Generate the HMAC-SHA1 signature
            using var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(model.AccessKeySecret));
            byte[] signatureBytes = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

            // Base64 encode the signature
            return Convert.ToBase64String(signatureBytes);
        }
    }
}
