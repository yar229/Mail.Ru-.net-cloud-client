namespace MailRuCloudApi
{
    public class TwoFaCodeResult
    {
        public string Code { get; set; }
        public bool DoNotAskAgainForThisDevice { get; set; }

    }

    public interface ITwoFaHandler
    {
        TwoFaCodeResult Get(string login, bool isAutoRelogin);
    }
}
