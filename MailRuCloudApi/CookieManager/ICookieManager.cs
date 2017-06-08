using System.Net;

namespace MailRuCloudApi.CookieManager
{
    public interface ICookieManager
    {
        CookieContainer Load(string name);
        void Save(string name, CookieContainer container);
    }
}