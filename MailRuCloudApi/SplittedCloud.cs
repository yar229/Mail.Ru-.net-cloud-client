using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MailRuCloudApi.EntryTypes;

namespace MailRuCloudApi
{
    public class SplittedCloud : MailRuCloud
    {
        public SplittedCloud(string login, string password, ITwoFaHandler twoFaHandler) : base(login, password, twoFaHandler)
        {
        }

        public override async Task<IFileOrFolder> GetItems(string path)
        {
            var entry = await base.GetItems(path);

            if (null == entry) return null;

            if (entry is Folder folder)
            {

                var groupedFiles = folder.Files
                    .GroupBy(f => Regex.Match(f.Name, @"(?<name>.*?)(\.wdmrc\.(crc|\d\d\d))?\Z").Groups["name"].Value,
                        file => file)
                    .Select(group => group.Count() == 1
                        ? group.First()
                        : new SplittedFile(group.ToList()))
                    .ToList();

                folder.Files = groupedFiles;
            }

            return entry;
        }
    }
}
