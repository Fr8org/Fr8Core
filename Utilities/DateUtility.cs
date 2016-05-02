using System;
using System.Linq;
using System.Linq.Expressions;

namespace Utilities
{
    public static class DateUtility
    {
        public static DateRange GenerateDateRange(string queryPeriod)
        {
            DateRange dateRange = new DateRange();
            switch (queryPeriod.ToLower())
            {

                case "last5minutes":
                    dateRange.StartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
                    break;
                case "lasthour":
					dateRange.StartTime = DateTimeOffset.UtcNow.AddHours(-1);
                    break;
                case "lastday":
					dateRange.StartTime = DateTimeOffset.UtcNow.AddDays(-1);
                    break;
                case "lastweek":
					dateRange.StartTime = DateTimeOffset.UtcNow.AddDays(-7);
                    break;
            }
			dateRange.EndTime = DateTimeOffset.UtcNow;
            return dateRange;
        }

        public static string TimeAgo(this DateTimeOffset dt)
        {
			TimeSpan span = DateTimeOffset.UtcNow - dt;
            if (span.Days > 365)
            {
                int years = (span.Days / 365);
                if (span.Days % 365 != 0)
                    years += 1;
                return String.Format("about {0} {1} ago",
                    years, years == 1 ? "year" : "years");
            }
            if (span.Days > 30)
            {
                int months = (span.Days / 30);
                if (span.Days % 31 != 0)
                    months += 1;
                return String.Format("about {0} {1} ago",
                    months, months == 1 ? "month" : "months");
            }
            if (span.Days > 0)
                return String.Format("about {0} {1} ago",
                span.Days, span.Days == 1 ? "day" : "days");
            if (span.Hours > 0)
                return String.Format("about {0} {1} ago",
                span.Hours, span.Hours == 1 ? "hour" : "hours");
            if (span.Minutes > 0)
                return String.Format("about {0} {1} ago",
                span.Minutes, span.Minutes == 1 ? "minute" : "minutes");
            if (span.Seconds > 5)
                return String.Format("about {0} seconds ago", span.Seconds);
            if (span.Seconds <= 5)
                return "just now";
            return string.Empty;
        }

        public static string TimeAgo(this DateTime dt)
        {
            return TimeAgo((DateTimeOffset) dt);
        }


        public static DateTime? CalculateDayBucket(DateTime? createDate)
        {
            if (!createDate.HasValue)
            {
                return null;
            }

            return createDate.Value.Date;
        }

        public static DateTime? CalculateWeekBucket(DateTime? createDate)
        {
            if (!createDate.HasValue)
            {
                return null;
            }

            var date = createDate.Value.Date;
            return date.AddDays(-1 * (int)date.DayOfWeek);
        }

        public static DateTime? CalculateMonthBucket(DateTime? createDate)
        {
            if (!createDate.HasValue)
            {
                return null;
            }

            var date = createDate.Value.Date;
            return date.AddDays(-date.Day + 1);
        }

        public static DateTime? CalculateYearBucket(DateTime? createDate)
        {
            if (!createDate.HasValue)
            {
                return null;
            }

            var date = createDate.Value.Date;
            return date.AddDays(-date.DayOfYear + 1);
        }
    }

    public struct DateRange
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }

    public static class DateRangeExtensions
    {
        /// <summary>
        /// Alias for Where filter over date range
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <param name="query">Source query</param>
        /// <param name="dateTimeField">Expression for navigating date time field to filter against</param>
        /// <param name="range">Date range</param>
        /// <param name="inclusive">To include range ends in filter</param>
        /// <returns>New query</returns>
        /// <example>
        /// var thisYearDateRange = new DateRange() { StartTime = DateTimeOffset.UtcNow.AddYears(-1), EndTime = DateTimeOffset.UtcNow }
        /// var incidentsOfThisYear = uow.IncidentRepository.GetQuery().WhereInDateRange(i => i.CreateDate, thisYearDateRange).ToList();
        /// </example>
        public static IQueryable<T> WhereInDateRange<T>(this IQueryable<T> query, Expression<Func<T, DateTimeOffset>> dateTimeField, DateRange range, bool inclusive = false)
        {
            var memberExpression = dateTimeField.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("Expression should point a member", "dateTimeField");
            var memberName = memberExpression.Member.Name;
            var entityExpression = Expression.Parameter(typeof(T));
            Expression whereExpression;
            if (inclusive)
            {
                whereExpression = Expression.And(
                        Expression.GreaterThanOrEqual(Expression.PropertyOrField(entityExpression, memberName), Expression.Constant(range.StartTime)),
                        Expression.LessThanOrEqual(Expression.PropertyOrField(entityExpression, memberName), Expression.Constant(range.EndTime)));
            }
            else
            {
                whereExpression = Expression.And(
                        Expression.GreaterThan(Expression.PropertyOrField(entityExpression, memberName), Expression.Constant(range.StartTime)),
                        Expression.LessThan(Expression.PropertyOrField(entityExpression, memberName), Expression.Constant(range.EndTime)));
            }
            Expression<Func<T, bool>> whereLambdaExpression = Expression.Lambda<Func<T, bool>>(whereExpression, entityExpression);
            return query.Where(whereLambdaExpression);
        }
    }

}

