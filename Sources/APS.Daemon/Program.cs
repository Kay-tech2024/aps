using APS.Core;
using APS.TimeMachine;
using System;
using System.IO;

namespace APS.Daemon
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                string path = @"C:\APS_Daemon\Logs\APS_Daemon_Info.log";
                File.AppendAllText(path, Environment.NewLine + "Date: " + DateTime.Now.ToShortDateString() + Environment.NewLine + "/n/n *** Running on: " + TM.Now.ToString());

                (new RefereeUtil()).CheckRefereeFundingReviewRequestExpiryAndSendNotificationToNextReferee();
            }
            catch (Exception ex)
            {
                // string _exception = "******************** /n/n Message: " + ex.Message + "\n InnerExceptionMessae: " + ex.InnerException.Message + " \n StackTrack: " + ex.StackTrace + "\n\n\n\n";
                string _exception = "Date: " + DateTime.Now.ToShortDateString() + Environment.NewLine + "******************** /n/n Message: " + ex.Message;
                if (ex.InnerException != null)
                {
                    _exception += "\n InnerExceptionMessae: " + ex.InnerException.Message;
                }
                if (ex.StackTrace != null)
                {
                    _exception += "\n StackTrack: " + ex.StackTrace;
                }
                Console.WriteLine(_exception);
                string path = @"C:\APS_Daemon\Logs\APS_Daemon_Error.log";
                File.AppendAllText(path, Environment.NewLine + _exception);
            }
        }
    }
}