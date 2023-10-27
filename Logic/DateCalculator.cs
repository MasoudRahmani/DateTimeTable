using System;
using System.Data;
using System.Globalization;

namespace DateTimeTable.Logic
{

    public class DateCalculator
    {
        public DateCalculator()
        {

        }
        public DataTable CalculateDates(DataTable TableScheme, DateTime start, DateTime End)
        {
            DataTable Dates = TableScheme;

            var date = start;

            while (date < End)
            {
                var DtInfo = new DateTimeFormatInfo();
                
                var pc = new PersianCalendar();
                var hc = new HijriCalendar();
                var gc = new GregorianCalendar();

                #region Miladi
                //Miladi
                int? g_weekofYear; int? g_year; int? g_month; int? g_day; string g_month_pad; string g_day_pad;
                int? g_dayofweek; string g_monthname; bool? g_leapday;
                if (date >= gc.MinSupportedDateTime && date <= gc.MaxSupportedDateTime)
                {
                    g_year = gc.GetYear(date);
                    g_month = gc.GetMonth(date);
                    g_day = gc.GetDayOfMonth(date);
                    g_month_pad = g_month.ToString().PadLeft(2, '0');
                    g_monthname = DtInfo.GetMonthName((int)g_month);

                    g_day_pad = g_day.ToString().PadLeft(2, '0');
                    g_dayofweek = (int)gc.GetDayOfWeek(date);
                    g_weekofYear = GetIso8601WeekOfYear(date); //.NetCore 3.1 Fixes this
                                                               //int g_weekofYear = gc.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    g_leapday = gc.IsLeapDay((int)g_year, (int)g_month, (int)g_day);
                }
                else
                {
                    g_dayofweek = g_weekofYear = g_year = g_month = g_day = null;
                    g_monthname = g_month_pad = g_day_pad = string.Empty;
                    g_leapday = null;
                }
                #endregion

                #region Shamsi
                //Persian
                int? p_weekofYear; int? p_year; int? p_month; int? p_day; string p_month_pad; string p_day_pad;
                DayOfWeek? p_dayofWeek; int? p_season_n; bool? p_holiday; bool? p_leapday;
                int? p_persinadayofweek;
                if (date >= pc.MinSupportedDateTime && date <= pc.MaxSupportedDateTime)
                {
                    p_year = pc.GetYear(date);
                    p_month = pc.GetMonth(date);
                    p_day = pc.GetDayOfMonth(date);
                    p_month_pad = p_month.ToString().PadLeft(2, '0');
                    p_day_pad = p_day.ToString().PadLeft(2, '0');
                    p_weekofYear = pc.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Saturday);
                    p_dayofWeek = pc.GetDayOfWeek(date);
                    p_season_n = GetJalaliSeasonNumber((int)p_month);
                    p_holiday = IsOneDayBeforePersianHoliday((int)p_year, (int)p_month, (int)p_day, (DayOfWeek)p_dayofWeek);
                    p_persinadayofweek = (int)GetPersionDayOfWeek((DayOfWeek)p_dayofWeek);
                    p_leapday = pc.IsLeapDay((int)p_year, (int)p_month, (int)p_day);
                }
                else
                {
                    p_persinadayofweek = p_weekofYear = p_year = p_month = p_day = p_season_n = null;
                    p_month_pad = p_day_pad = string.Empty;
                    p_dayofWeek = null;
                    p_leapday = p_holiday = null;
                }

                string p_dayofweekName = GetJalaliDayName(p_dayofWeek);
                string p_monthName = GetJalaliMonthName(p_month);
                #endregion

                #region Hijri
                //Hijri
                int? h_weekofYear; int? h_year; int? h_month; int? h_day; string h_month_pad; string h_day_pad; bool? h_holiday;
                string h_monthname; string h_dayname;
                if (date >= hc.MinSupportedDateTime && date <= hc.MaxSupportedDateTime)
                {
                    h_year = hc.GetYear(date);
                    h_month = hc.GetMonth(date);
                    h_day = hc.GetDayOfMonth(date);
                    h_month_pad = h_month.ToString().PadLeft(2, '0');
                    h_day_pad = h_day.ToString().PadLeft(2, '0');
                    h_weekofYear = hc.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Saturday);
                    h_holiday = IsOneDayBeforeHijriHoliday((int)h_year, (int)h_month, (int)h_day);
                    
                    h_monthname = GetHijriMonthName(h_month);
                    h_dayname = GetHijriDayName(p_dayofWeek);

                }
                else
                {
                    h_weekofYear = h_year = h_month = h_day = null;
                    h_dayname = h_monthname = h_month_pad = h_day_pad = string.Empty;
                    h_holiday = null;
                }

                #endregion


                Dates.Rows.Add(
                                //Greg
                                string.Concat(g_year, g_month_pad, g_day_pad), date,
                                g_year, g_month, g_day,
                                ToInt32(string.Concat(g_month, g_day_pad)), g_dayofweek,
                                g_monthname, string.Concat(g_year, "/", g_month_pad, "/", g_day_pad),
                                ToInt32(string.Concat(g_year, g_month_pad)), string.Concat(g_year, "/", g_month_pad),
                                gc.GetDayOfWeek(date).ToString(), "Week " + g_weekofYear,
                                g_weekofYear,
                                //Persian
                                string.Concat(p_year, p_month_pad, p_day_pad), p_year, p_month, p_day,
                                string.Concat(p_month, p_day_pad), p_persinadayofweek,
                                p_monthName, string.Concat(p_year, "/", p_month_pad, "/", p_day_pad),
                                ToInt32(string.Concat(p_year, p_month_pad)), string.Concat(p_year, "/", p_month_pad),
                                p_dayofweekName, string.Concat("هفته ", p_weekofYear), p_weekofYear,
                                string.Concat(p_dayofweekName, " ", p_day, " ", p_monthName, " ", p_year),
                                //Hijri
                                string.Concat(h_year, h_month_pad, h_day_pad), h_year, h_month, h_day,
                                string.Concat(h_month, h_day_pad), p_persinadayofweek, h_monthname,
                                string.Concat(h_year, "/", h_month_pad, "/", h_day_pad), ToInt32(string.Concat(h_year, h_month_pad)),
                                string.Concat(h_year, "/", h_month_pad), h_dayname,
                                string.Concat("الأسبوع ", h_weekofYear), h_weekofYear,
                                //Tots
                                p_season_n, GetJalaliSeasonName(p_season_n),
                                g_leapday, p_leapday,
                                p_holiday, h_holiday);

                date = date.AddDays(1);
            }

            return Dates;
        }


        /// <summary>
        /// https://blogs.msdn.microsoft.com/shawnste/2006/01/24/iso-8601-week-of-year-format-in-microsoft-net/
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public int GetIso8601WeekOfYear(DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            var cal = new GregorianCalendar();
            DayOfWeek day = cal.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return cal.GetWeekOfYear(time, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        public PersianDayOfWeek? GetPersionDayOfWeek(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Saturday:
                    return PersianDayOfWeek.Shanbe;
                case DayOfWeek.Sunday:
                    return PersianDayOfWeek.Yekshanbe;
                case DayOfWeek.Monday:
                    return PersianDayOfWeek.Doshanbe;
                case DayOfWeek.Tuesday:
                    return PersianDayOfWeek.Seshanbe;
                case DayOfWeek.Wednesday:
                    return PersianDayOfWeek.Charshanbe;
                case DayOfWeek.Thursday:
                    return PersianDayOfWeek.Panjshanbe;
                case DayOfWeek.Friday:
                    return PersianDayOfWeek.Jome;
                default:
                    return null;
            }
        }

        #region Private Functions

        private bool? IsOneDayBeforePersianHoliday(int year, int month, int day, DayOfWeek dw)
        {
            var pc = new PersianCalendar();
            int lastday = pc.GetDaysInMonth(year, month);

            var NewYear = ((month == 12) & lastday == day) ? true : false;
            var PajShanbe = GetPersionDayOfWeek(dw) == PersianDayOfWeek.Panjshanbe ? true : false;

            if (NewYear == true || PajShanbe == true)
            {
                return true;
            }
            else return false;
        }
        private bool? IsOneDayBeforeHijriHoliday(int year, int month, int day)
        {
            var hc = new HijriCalendar();
            int lastday = hc.GetDaysInMonth(year, month);

            var rtn = ((month == 12) & lastday == day) ? true : false;
            if (rtn == false)
            {
                return null;
            }
            return rtn;
        }

        private string GetJalaliDayName(DayOfWeek? day)
        {
            if (day == null)
            {
                return string.Empty;
            }
            switch (day)
            {
                case DayOfWeek.Saturday:
                    return "شنبه";
                case DayOfWeek.Sunday:
                    return "یکشنبه";
                case DayOfWeek.Monday:
                    return "دوشنبه";
                case DayOfWeek.Tuesday:
                    return "سه شنبه";
                case DayOfWeek.Wednesday:
                    return "چهارشنبه";
                case DayOfWeek.Thursday:
                    return "پنج شنبه";
                case DayOfWeek.Friday:
                    return "جمعه";
                default:
                    return string.Empty;
            }

        }
        private string GetJalaliMonthName(int? month)
        {
            if (month == null)
            {
                return string.Empty;
            }
            string[] monthInYear = { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند" };
            return monthInYear[(int)month - 1];
        }
        private int? GetJalaliSeasonNumber(int? month)
        {
            if (month == null)
            {
                return null;
            }
            switch (month)
            {
                case 1:
                case 2:
                case 3:
                    return 1;
                case 4:
                case 5:
                case 6:
                    return 2;
                case 7:
                case 8:
                case 9:
                    return 3;
                case 10:
                case 11:
                case 12:
                    return 4;
                default:
                    return null;
            }
        }
        private string GetJalaliSeasonName(int? month)
        {
            if (month == null)
            {
                return string.Empty;
            }
            string[] seasonsInYear = { "بهار", "تابستان", "پاییز", "زمستان" };
            return seasonsInYear[(int)month - 1];
        }

        private string GetHijriMonthName(int? month)
        {
            if (month == null)
            {
                return string.Empty;
            }
            string[] monthInYear = { "محرم", "صفر", "ربیع الاول", "ربیع الثانی", "جمادی الاول", "جمادی الثانی", " رجب", "شعبان", " رمضان", "شوال", "ذیقعده", "ذیحجه" };
            return monthInYear[(int)month - 1];
        }
        private string GetHijriDayName(DayOfWeek? day)
        {
            if (day == null)
            {
                return string.Empty;
            }
            switch (day)
            {
                case DayOfWeek.Saturday:
                    return "السّبت";
                case DayOfWeek.Sunday:
                    return "الأحد";
                case DayOfWeek.Monday:
                    return "الإثنينِ";
                case DayOfWeek.Tuesday:
                    return "الثلاثاء";
                case DayOfWeek.Wednesday:
                    return "الأربعاء";
                case DayOfWeek.Thursday:
                    return "الخميس";
                case DayOfWeek.Friday:
                    return "الجمعة";
                default:
                    return string.Empty;
            }

        }
        private int? ToInt32(string source)
        {
            if (source == string.Empty)
            {
                return null;
            }
            int result;
            var ok = Int32.TryParse(source, out result);

            if (ok)
            {
                return result;
            }
            else return null;
        }
        #endregion

    }
    public enum PersianDayOfWeek
    {
        Shanbe = 0,
        Yekshanbe = 1,
        Doshanbe = 2,
        Seshanbe = 3,
        Charshanbe = 4,
        Panjshanbe = 5,
        Jome = 6
    }

}
