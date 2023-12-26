using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryOSS.Services;

namespace TryOSS
{
    public class Program
    {
        public static async Task Main()
        {
            var service = new OSSService();
            await service.PutObjectTagging();
            await service.GetObjectTagging();
        }
    }
}
