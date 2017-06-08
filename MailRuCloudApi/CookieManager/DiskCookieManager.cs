using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Soap;

namespace MailRuCloudApi.CookieManager
{
    public class DiskCookieManager : ICookieManager
    {
        private readonly string _basePath;
        private const string Extension = ".cookie.dat";

        public DiskCookieManager(string basePath)
        {
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            _basePath = basePath;
        }

        public CookieContainer Load(string name)
        {
            var formatter = new SoapFormatter();
            CookieContainer retrievedCookies = null;
            string file = Path.Combine(_basePath, $"{name}{Extension}");
            if (System.IO.File.Exists(file))
                using (Stream s = System.IO.File.OpenRead(file))
                    retrievedCookies = (CookieContainer)formatter.Deserialize(s);

            return retrievedCookies;
        }

        public void Save(string name, CookieContainer container)
        {
            var formatter = new SoapFormatter();
            string file = Path.Combine(_basePath, $"{name}{Extension}");

            using (Stream s = System.IO.File.Create(file))
                formatter.Serialize(s, container);
        }
    }
}
