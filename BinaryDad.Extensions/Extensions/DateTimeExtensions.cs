using System;

namespace BinaryDad.Extensions
{
    public static class DateTimeExtensions
    {
        public static bool IsWeekDay(this DateTime date)
        {
            //default to false
            var returnValue = false;

            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                case DayOfWeek.Tuesday:
                case DayOfWeek.Wednesday:
                case DayOfWeek.Thursday:
                case DayOfWeek.Friday:
                    returnValue = true;
                    break;
                default:
                    break;

            }

            return returnValue;
        }

        public static bool IsWeekEnd(this DateTime date)
        {
            //default to false
            var returnValue = false;

            switch (date.DayOfWeek)
            {
                case DayOfWeek.Saturday:
                case DayOfWeek.Sunday:

                    returnValue = true;
                    break;
                default:
                    break;
            }

            return returnValue;
        }
    }
}