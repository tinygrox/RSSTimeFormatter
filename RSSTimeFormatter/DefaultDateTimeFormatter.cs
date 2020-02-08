using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace RSSTimeFormatter
{
	public class DefaultDateTimeFormatter : IDateTimeFormatter
	{
		static string AddUnits(int val, string singular, string plural)
		{
			return val.ToString() + (val == 1 ? singular : plural);
		}
		static string IsBadNum(double time)
		{
			if (double.IsNaN(time)) {
				return "NaN";
			}
			if (double.IsPositiveInfinity(time)) {
				return "+Inf";
			}
			if (double.IsNegativeInfinity(time)) {
				return "-Inf";
			}
			return null;
		}
		public string PrintTimeLong(double time)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			int[] timeIntervals = GetDateFromUT(time);
			string timeStr = AddUnits(timeIntervals[4], Localizer.Format("#autoLOC_6002322"), Localizer.Format("#autoLOC_6002323"));//"Year""Years"
			timeStr += ", " + AddUnits(timeIntervals[3], Localizer.Format("#autoLOC_6002324"), Localizer.Format("#autoLOC_6002325"));//"Day""Days"
			timeStr += ", " + AddUnits(timeIntervals[2], Localizer.Format("#autoLOC_6002326"), Localizer.Format("#autoLOC_6002327"));//"Hour""Hours"
			timeStr += ", " + AddUnits(timeIntervals[1], Localizer.Format("#autoLOC_6002328"), Localizer.Format("#autoLOC_6002329"));//"Min""Mins"
			timeStr += ", " + AddUnits(timeIntervals[0], Localizer.Format("#autoLOC_6002330"), Localizer.Format("#autoLOC_6002331"));//"Sec""Secs"
			return timeStr;
		}
		public string PrintTimeStamp(double time, bool days = false, bool years = false)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			int[] timeIntervals = GetDateFromUT(time);
			string timeStr = "";
			if (years) {
				timeStr += Localizer.Format("#autoLOC_6002322") + " " + timeIntervals[4] + ", ";//Year
			}
			if (days) {
				timeStr += Localizer.Format("#autoLOC_6002324") + " " + timeIntervals[3] + " - ";//Day
			}
			timeStr += timeIntervals[2].ToString("00");
			timeStr += ":" + timeIntervals[1].ToString("00");
			if (timeIntervals[4] < 10)
				timeStr += ":" + timeIntervals[0].ToString("00");
			return timeStr;
		}
		public string PrintTimeStampCompact(double time, bool days = false, bool years = false)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			int[] timeIntervals = GetDateFromUT(time);
			string timeStr = "";
			if (years) {
				timeStr += timeIntervals[4].ToString() + Localizer.Format("#autoLOC_6002321") + ", ";//y
			}
			if (days) {
				timeStr += timeIntervals[3].ToString() + Localizer.Format("#autoLOC_6002320") + ", ";//d
			}
			timeStr += timeIntervals[2].ToString("00");
			timeStr += ":" + timeIntervals[1].ToString("00");
			if (timeIntervals[4] < 10)
				timeStr += ":" + timeIntervals[0].ToString("00");
			return timeStr;
		}
		public string PrintTime(double time, int valuesOfInterest, bool explicitPositive)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			bool isNegative = time < 0;
			int[] timeIntervals = GetDateFromUT(time);

			string[] intervalCaptions = new string[]
				{
					Localizer.Format("#autoLOC_6002317"), Localizer.Format("#autoLOC_6002318"), Localizer.Format("#autoLOC_6002319"), Localizer.Format("#autoLOC_6002320"), Localizer.Format("#autoLOC_6002321")//"s""m""h""d""y"
				};

			string timeString = isNegative ? "- " : (explicitPositive ? "+ " : "");

			// find first non-zero value, checking backwards
			for (int i = timeIntervals.Length - 1; i >= 0; i--) {
				if (timeIntervals[i] != 0) {
					for (int j = i; j > Mathf.Max(i - valuesOfInterest, -1); j--) {
						timeString += Math.Abs(timeIntervals[j]) + intervalCaptions[j] + (j - 1 > Mathf.Max(i - valuesOfInterest, -1) ? ", " : "");
					}
					break;
				}
			}

			return timeString;
		}

		public string PrintTimeCompact(double time, bool explicitPositive)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			bool isNegative = time < 0;
			int[] timeIntervals = GetDateFromUT(time);

			string timeString = isNegative ? "T- " : (explicitPositive ? "T+ " : "");

			timeString += (timeIntervals[3] > 0 ? Math.Abs(timeIntervals[3]).ToString() + ":" : "") +
				Math.Abs(timeIntervals[2]).ToString("00") + ":" +
				Math.Abs(timeIntervals[1]).ToString("00") + ":" +
				Math.Abs(timeIntervals[0]).ToString("00");

			// find first non-zero value, checking backwards
			/*for (int i = timeIntervals.Length - 1; i >= 0; i--)
            {
                if (timeIntervals[i] != 0)
                {
                    for (int j = i; j > Mathf.Max(i - valuesOfInterest, -1); j--)
                    {
                        timeString += (j==3 ? Math.Abs(timeIntervals[j]).ToString() : Math.Abs(timeIntervals[j]).ToString("00")) + (j - 1 > Mathf.Max(i - valuesOfInterest, -1) ? ":" : "");
                    }
                    break;
                }
            }
            */
			return timeString;
		}

		public int[] GetDateFromUT(double time)
		{
			return GameSettings.KERBIN_TIME ? GetKerbinDateFromUT(time) : GetEarthDateFromUT(time);
		}

		static int[] get_date_from_UT(double time, int year_len, int day_len)
		{
			int years = (int)(time / year_len);
			time -= (double)years * (double)year_len;
			int seconds = (int)time;
			int minutes = (seconds / 60) % 60;
			int hours = (seconds / 3600) % (day_len / 3600);
			int days = seconds / day_len;
			int[] timeIntervals = new int[] {
				seconds % 60,
				minutes,
				hours,
				days,
				years
			};

			return timeIntervals;
		}

		public int[] GetEarthDateFromUT(double time)
		{
			return get_date_from_UT(time, EarthYear, EarthDay);
		}

		public int[] GetKerbinDateFromUT(double time)
		{
			return get_date_from_UT(time, KerbinYear, KerbinDay);
		}

		public string PrintDateDelta(double time, bool includeTime, bool includeSeconds, bool useAbs)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			if (useAbs && time < 0d)
				time = -time;

			string date = "";

			int[] saveDate = GetDateFromUT(time);

			if (saveDate[4] > 1) {
				date += saveDate[4].ToString() + " " + Localizer.Format("#autoLOC_6002335");//years
			}
			else if (saveDate[4] == 1) {
				date += saveDate[4].ToString() + " " + Localizer.Format("#autoLOC_6002334");//year
			}

			if (saveDate[3] > 1) {
				if (date != "")
					date += ", ";

				date += saveDate[3].ToString() + " " + Localizer.Format("#autoLOC_6002336");//days
			}
			else if (saveDate[3] == 1) {
				if (date != "")
					date += ", ";

				date += saveDate[3].ToString() + " "+Localizer.Format("#autoLOC_6002337");//day
			}

			if (includeTime) {
				if (saveDate[2] > 1) {
					if (date != "")
						date += ", ";

					date += saveDate[2].ToString() + " "+Localizer.Format("#autoLOC_6002339");//hours
				}
				else if (saveDate[2] == 1) {
					if (date != "")
						date += ", ";

					date += saveDate[2].ToString() + " "+Localizer.Format("#autoLOC_6002338");//hour
				}

				if (saveDate[1] > 1) {
					if (date != "")
						date += ", ";

					date += saveDate[1].ToString() + " "+Localizer.Format("#autoLOC_6002340");//minutes
				}
				else if (saveDate[1] == 1) {
					if (date != "")
						date += ", ";

					date += saveDate[1].ToString() + " "+Localizer.Format("#autoLOC_6002341");//minute
				}

				if (includeSeconds) {
					if (saveDate[0] > 1) {
						if (date != "")
							date += ", ";

						date += saveDate[0].ToString() + " "+Localizer.Format("#autoLOC_6002342");//seconds
					}
					else if (saveDate[0] == 1) {
						if (date != "")
							date += ", ";

						date += saveDate[0].ToString() + " "+Localizer.Format("#autoLOC_6002343");//second
					}
				}
			}

			if (string.IsNullOrEmpty(date))
				date = includeTime ? (includeSeconds ? "0 "+Localizer.Format("#autoLOC_6002342") : "0 "+Localizer.Format("#autoLOC_6002340")) : "0 "+Localizer.Format("#autoLOC_6002336");//seconds minutesdays

			return date;
		}

		public string PrintDateDeltaCompact(double time, bool includeTime, bool includeSeconds, bool useAbs)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			if (useAbs && time < 0d)
				time = -time;

			string date = "";

			int[] saveDate = GetDateFromUT(time);

			if (saveDate[4] > 0) {
				date += saveDate[4].ToString() + Localizer.Format("#autoLOC_6002321");//"y"
			}

			if (saveDate[3] > 0) {
				if (date != "")
					date += ", ";

				date += saveDate[3].ToString() + Localizer.Format("#autoLOC_6002320");//"d"
			}

			if (includeTime) {
				if (saveDate[2] > 0) {
					if (date != "")
						date += ", ";

					date += saveDate[2].ToString() + Localizer.Format("#autoLOC_6002319");//"h"
				}
				if (saveDate[1] > 0) {
					if (date != "")
						date += ", ";

					date += saveDate[1].ToString() + Localizer.Format("#autoLOC_6002318");//"m"
				}

				if (includeSeconds) {
					if (saveDate[0] > 0) {
						if (date != "")
							date += ", ";

						date += saveDate[0].ToString() + Localizer.Format("#autoLOC_6002317");//"s"
					}
				}
			}

			if (string.IsNullOrEmpty(date))
				date = includeTime ? (includeSeconds ? "0"+Localizer.Format("#autoLOC_6002317") : "0"+Localizer.Format("#autoLOC_6002318")) : "0"+Localizer.Format("#autoLOC_6002320");//smd

			return date;
		}

		public string PrintDate(double time, bool includeTime, bool includeSeconds = false)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			string date = "";

			int[] saveDate = GetDateFromUT(time);

			date += Localizer.Format("#autoLOC_6002322") + " " + (saveDate[4] + 1) + ", "+Localizer.Format("#autoLOC_6002324") +" " + (saveDate[3] + 1);//YearDay

			if (includeTime) {
				date += " - " + saveDate[2] + Localizer.Format("#autoLOC_6002319") +", " + saveDate[1] + Localizer.Format("#autoLOC_6002318");//h"m"
			}
			if (includeSeconds) {
				date += ", " + saveDate[0] + Localizer.Format("#autoLOC_6002317");//"s"
			}

			return date;
		}

		public string PrintDateNew(double time, bool includeTime)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			string date = "";

			int[] saveDate = GetDateFromUT(time);

			date += Localizer.Format("#autoLOC_6002322") + " " + (saveDate[4] + 1) + ", " + Localizer.Format("#autoLOC_6002324") + " " + (saveDate[3] + 1);//YearDay

			if (includeTime) {
				date += " - " + saveDate[2].ToString("D2") + ":" + saveDate[1].ToString("D2") + ":" + saveDate[0].ToString("D2");
			}

			return date;
		}

		public string PrintDateCompact(double time, bool includeTime, bool includeSeconds = false)
		{
			string badStr = IsBadNum(time);
			if (badStr != null) {
				return badStr;
			}
			string date = "";

			int[] saveDate = GetDateFromUT(time);

			date += Localizer.Format("#autoLOC_6002344") + (saveDate[4] + 1) + ", "+ Localizer.Format("#autoLOC_6002345") + (saveDate[3] + 1).ToString("00");//"Y"D

			if (includeTime) {
				date += ", " + saveDate[2] + ":" + saveDate[1].ToString("00");
			}
			if (includeSeconds) {
				date += ":" + saveDate[0].ToString("00");
			}

			return date;
		}

		public string PrintTime(double time, int valuesOfInterest, bool explicitPositive, bool logEnglish)
		{
			return PrintTime(time, valuesOfInterest, explicitPositive);
		}

		public int Minute { get { return 60; } }
		public int Hour { get { return 3600; } }
		public int Day { get { return GameSettings.KERBIN_TIME ? KerbinDay : EarthDay; } }
		public int Year { get { return GameSettings.KERBIN_TIME ? KerbinYear : EarthYear; } }

		public int KerbinDay { get { return 21600; } }
		public int KerbinYear { get { return 9201600; } }
		public int EarthDay { get { return 86400; } }
		public int EarthYear { get { return 31536000; } }
	}
}
