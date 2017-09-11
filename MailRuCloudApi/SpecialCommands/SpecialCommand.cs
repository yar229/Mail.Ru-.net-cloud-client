using System.Threading.Tasks;
using NWebDav.Server.Stores;

namespace MailRuCloudApi.SpecialCommands
{
    public abstract class SpecialCommand
    {
        public abstract Task<StoreCollectionResult> Execute();
    }
}
