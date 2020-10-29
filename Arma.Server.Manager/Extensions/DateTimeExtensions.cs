using System;

namespace Arma.Server.Manager.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        ///     Checks whether given <paramref name="dateTime" /> is within <paramref name="precision" /> <see cref="TimeSpan" />
        ///     from <paramref name="referenceDateTime" />.
        /// </summary>
        /// <param name="dateTime">Checked date time.</param>
        /// <param name="referenceDateTime">Reference date time.</param>
        /// <param name="precision">Time span from reference date time. Default is 1 hour.</param>
        public static bool IsCloseTo(
            this DateTime dateTime,
            DateTime referenceDateTime,
            TimeSpan? precision = null)
        {
            precision ??= TimeSpan.FromHours(1);
            return referenceDateTime - precision < dateTime && referenceDateTime + precision > dateTime;
        }
    }
}
