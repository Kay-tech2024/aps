using APS.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace APS.Web.Models
{
    public class SmsSettingsViewModel
    {
        public List<SmsProviderViewModel> Providers { get; set; }

        public SmsProvider PreferredProvider { get; set; }
    }

    public class SmsProviderViewModel
    {
        public SmsProvider Type { get; set; }
        public string SenderName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TestEmailViewModel
    {
        [Required(ErrorMessage = "Required")]
        [AllowHtml]
        public string Body { get; set; }
    }
}