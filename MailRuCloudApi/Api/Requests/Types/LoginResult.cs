namespace MailRuCloudApi.Api.Requests.Types
{
    //class LoginResult
    //{
    //    public string Csrf { get; set; }
    //}

    public class LoginResult
    {
        public string form_name { get; set; }
        public string auth_host { get; set; }
        public string secstep_phone { get; set; }
        public string secstep_page { get; set; }
        public string secstep_code_fail { get; set; }
        public string secstep_resend_fail { get; set; }
        public string secstep_resend_success { get; set; }
        public string secstep_timeout { get; set; }
        public string secstep_login { get; set; }
        public string secstep_disposable_fail { get; set; }
        public string secstep_smsapi_error { get; set; }
        public string secstep_captcha { get; set; }
        public string totp_enabled { get; set; }
        public string locale { get; set; }
        public string client { get; set; }
        public string csrf { get; set; }
        public string device { get; set; }
        public string test { get; set; }
        public bool withClientNavigation { get; set; }
        public string project { get; set; }
        public string img_name { get; set; }
    }
}