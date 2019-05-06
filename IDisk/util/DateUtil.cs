using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudManager.util
{
   public class DateUtil
    {
        public static long currentTimeMillis(DateTime d) {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long unixTime = (long)System.Math.Round((d - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }
           
    }
} 
