namespace MailRuCloudApi.SpecialCommands
{
    public static class SpecialCommandFabric
    {
        public static SpecialCommand Build(MailRuCloud cloud, string param)
        {
            return new SharedFolderLinkCommand(cloud, param);
        }
    }
}