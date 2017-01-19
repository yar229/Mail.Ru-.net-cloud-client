using System;

namespace MailRuCloudApi
{
    public static class WebDavPath
    {
        public static string Combine(string a, string b)
        {
            a = Clean(a);
            b = Clean(b);
            a = a.Trim('/');
            b = b.TrimStart('/');
            string res = "/" + a + (string.IsNullOrEmpty(b) ? "" : "/" + b);
            return res;

        }

        public static string Clean(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string Parent(string path)
        {
            int pos = path.LastIndexOf("/", StringComparison.Ordinal);
            return pos > 0
                ? path.Substring(0, pos)
                : path;
        }
    }
}
