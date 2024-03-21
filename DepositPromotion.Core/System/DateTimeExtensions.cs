namespace System;

public static class DateTimeExtensions
{
	public static DateOnly CeilingWeekDay(this DateOnly date)
		=> date.FloorWeekDay().AddDays(7);

	public static DateOnly FloorWeekDay(this DateOnly date)
	{
		var dayOfWeek = (int)date.DayOfWeek;

		if (dayOfWeek == 0)
			dayOfWeek = 7;
		var diffDay = 1 - dayOfWeek;

		return date.AddDays(diffDay);
	}

	public static DateOnly FloorMonthDay(this DateOnly date)
		=> date.AddDays(1 - date.Day);

	public static DateOnly CeilingMonthDay(this DateOnly date)
		=> date.FloorMonthDay().AddMonths(1);
}
