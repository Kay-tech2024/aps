using APS.Services;
using System.Configuration;

namespace APS.Core
{
    public class SmsService
    {
        public static bool SendMessage(string message, params string[] to)
        {
            string error = null;
            return SendMessage(message, to, out error);
        }

        public static bool SendMessage(string message, string[] to, out string error)
        {
            error = null;

            if (ConfigurationManager.AppSettings["DontSendSMS"] == "true")
                return true;

            var useMalath = Utils.PreferredSmsProvider == SmsProvider.MalathSMS;

            var settings = (SmsSettings)(useMalath ? Utils.MalathSmsSettings : Utils.MobilySmsSettings);

            if (useMalath)
            {
                return SmsServiceMalath.SendMessage(settings, message, to, out error);
            }
            else
            {
                return SmsServiceMobily.SendMessage(settings, message, to, out error);
            }
        }
    }
}