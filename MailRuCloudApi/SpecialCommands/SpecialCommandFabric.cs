namespace MailRuCloudApi.SpecialCommands
{
    public static class SpecialCommandFabric
    {
        public static SpecialCommand Build(MailRuCloud cloud, string param)
        {
            return param != null && param.StartsWith("/>>")
                ? new SharedFolderLinkCommand(cloud, param)
                : null;
        }
    }
}