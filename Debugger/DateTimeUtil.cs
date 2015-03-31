using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ModTools
{
    public static class DateTimeUtil
    {

        private static readonly float secondsInMinute = 60.0f;
        private static readonly float secondsInHour = secondsInMinute * 60.0f;
        private static readonly float secondsInDay = secondsInHour * 24.0f;
        private static readonly float secondsInWeek = secondsInDay * 7.0f;
        private static readonly float secondsInYear = secondsInWeek * 52.0f;
        public static string TimeSpanToString(TimeSpan timeSpan)
        {
            var seconds = Math.Abs(timeSpan.TotalSeconds);
            if (seconds < secondsInMinute)
            {
                return "Less than a minute ago";
            }

            if (seconds < secondsInHour)
            {
                return TimeSpanToString((int)(seconds / secondsInMinute), "minute");
            }

            if (seconds < secondsInDay)
            {
                return TimeSpanToString((int)(seconds / secondsInHour), "hour");
            }

            if (seconds < secondsInWeek)
            {
                return TimeSpanToString((int)(seconds / secondsInDay), "day");
            }

            if (seconds < secondsInYear)
            {
                return TimeSpanToString((int)(seconds / secondsInWeek), "week");
            }

            return TimeSpanToString((int)(seconds / secondsInYear), "years");
        }

        private static string TimeSpanToString(int count, string s)
        {
            return String.Format("{0} {1} ago", count.ToString(), Pluralize(s, count));
        }

        public static string Pluralize(string s, int count)
        {
            if (count < 2)
            {
                return s;
            }

            return s + "s";
        }

    }

}
