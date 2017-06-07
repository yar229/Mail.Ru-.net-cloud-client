// ReSharper disable All

using System.Collections.Generic;

namespace MailRuCloudApi.Api.Requests.Types
{

    public class FolderInfoResult
    {
        public string email { get; set; }
        public Body body { get; set; }
        public long time { get; set; }
        public int status { get; set; }
    }


    public class Count
    {
        public int folders { get; set; }
        public int files { get; set; }
    }

    public class Sort
    {
        public string order { get; set; }
        public string type { get; set; }
    }

    public class Props
    {
        public long mtime;
        public Count count { get; set; }
        public string tree { get; set; }
        public string name { get; set; }
        public int grev { get; set; }
        public long size { get; set; }
        public string kind { get; set; }
        public int rev { get; set; }
        public string type { get; set; }
        public string home { get; set; }
        public string weblink { get; set; }
        public string hash { get; set; }

    }

    public class Body
    {
        public Count count { get; set; }
        public string tree { get; set; }
        public string name { get; set; }
        public int grev { get; set; }
        public long size { get; set; }
        public Sort sort { get; set; }
        public string kind { get; set; }
        public int rev { get; set; }
        public string type { get; set; }
        public string home { get; set; }
        public List<Props> list { get; set; }
        public string weblink { get; set; }
    }

}
