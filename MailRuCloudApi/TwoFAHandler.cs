namespace MailRuCloudApi
{
    public interface ITwoFaHandler
    {
        string Get(string login, bool isAutoRelogin);
    }
}
