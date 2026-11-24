using System;

namespace APS.TimeMachine
{
    public static class TM
    {
        public static DateTime Now
        {
            get
            {
                return TmUtils.CurrentDateTimeStatic;
            }
        }
    }
}