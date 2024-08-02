// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    InconsolableCellist (inconsolablecellist@dfworkshop.net)
// 
// Notes:
//

using UnityEngine;
using System;
using System.Collections;
using DaggerfallWorkshop.Game;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop.Utility
{
    /// <summary>
    /// Time and date implementation for Daggerfall's specific calendar system.
    /// Daggerfall has fixed 30-day months. See below link for more information.
    /// http://www.uesp.net/wiki/Lore:Calendar#Daggerfall_Calendar
    /// </summary>
    [Serializable]
    public class DaggerfallDateTime
    {
        #region Fields

        const ulong classicEpochInSeconds = 12566016000;        // Converts from DaggerfallDateTime epoch to classic Daggerfall epoch
        const uint classicGameStartTime = 523530;               // Game start time in minutes from classic epoch

        // Time multipliers
        public const int SecondsPerMinute = 60;
        public const int MinutesPerHour = 60;
        public const int MinutesPerDay = 1440;
        public const int HoursPerDay = 24;
        public const int DaysPerWeek = 7;
        public const int DaysPerMonth = 30;
        public const int MonthsPerYear = 12;
        public const int DaysPerYear = DaysPerMonth * MonthsPerYear;
        public const int SecondsPerHour = SecondsPerMinute * MinutesPerHour;
        public const int SecondsPerDay = SecondsPerHour * HoursPerDay;
        public const int SecondsPerWeek = SecondsPerDay * DaysPerWeek;
        public const int SecondsPerMonth = SecondsPerDay * DaysPerMonth;
        public const int SecondsPerYear = SecondsPerMonth * MonthsPerYear;

        // Time common events take place
        public const int BaseDawnHour = 6;
        public const int BaseDuskHour = 18;
        // public const int LightsOffHour = 8;
        // public const int LightsOnHour = 17;
        public const int MiddayHour = 12;
        public const int MidnightHour = 0;
        public const int MidMorningHour = 10;
        public const int MidAfternoonHour = 15;

        // Season starting date in day of year
        public const int SpringStart = 81;
        public const int SummerStart = 171;
        public const int FallStart = 261;
        public const int WinterStart = 351;

        // Latitude values of significant lines
        public const int ArcticCircle = 550;
        public const int UpperTropic = 3450;
        public const int LowerTropic = 5950;
        public const int Equator = 4700;
        public const int MiddleTemperate = 2000;

        // Time values by unit for easy use
        public int Year = 405;
        public int Month = 5;
        public int Day = 0; // Day of the month
        public int Hour = 12;
        public int Minute = 0;
        public float Second = 0;

        #endregion

        #region Properties

        /// <summary>
        /// Gets current day as string.
        /// </summary>
        public string DayName
        {
            get { return GetDayName(); }
        }

        /// <summary>
        /// Gets DawnHour (in minutes) based on equinox/solstice and latitude.
        /// </summary>
        public int DawnHour
        {
            get { return GetDawnHour(); }
        }

        /// <summary>
        /// Gets DuskHour (in minutes) based on equinox/solstice and latitude.
        /// </summary>
        public int DuskHour
        {
            get { return GetDuskHour(); }
        }

        /// <summary>
        /// Gets LightsOffHour based on DawnHour.
        /// </summary>
        public int LightsOffHour
        {
            get { return GetLightsOffHour(); }
        }

        /// <summary>
        /// Gets LightsOnHour based on DuskHour.
        /// </summary>
        public int LightsOnHour
        {
            get { return GetLightsOnHour(); }
        }

        /// <summary>
        /// Gets current day enum value.
        /// </summary>
        public Days DayValue
        {
            get { return (Days)Day; }
        }

        /// <summary>
        /// Gets current month as string.
        /// </summary>
        public string MonthName
        {
            get { return GetMonthName(); }
        }

        /// <summary>
        /// Gets current month enum value.
        /// </summary>
        public Months MonthValue
        {
            get { return (Months)Month; }
        }

        /// <summary>
        /// Gets birth sign name for current month.
        /// </summary>
        public string BirthSignName
        {
            get { return GetBirthSignName(); }
        }

        /// <summary>
        /// Gets current birth sign enum value.
        /// </summary>
        public BirthSigns BirthSignValue
        {
            get { return (BirthSigns)Month; }
        }

        /// <summary>
        /// Gets current season as string.
        /// </summary>
        public string SeasonName
        {
            get { return GetSeasonName(); }
        }

        /// <summary>
        /// Gets current season enum value.
        /// </summary>
        public Seasons SeasonValue
        {
            get { return GetSeasonValue(); }
        }

        /// <summary>
        /// Gets season enum value based on latitude and altitude.
        /// </summary>
        public Seasons ActualSeasonValue
        {
            get { return GetSeasonValue(true); }
        }

        /// <summary>
        /// Gets current lunar phase for Massar.
        /// Uses same logic as Enhanced Sky mod so phases should be in sync.
        /// </summary>
        public LunarPhases MassarLunarPhase
        {
            get { return GetLunarPhase(true); }
        }

        /// <summary>
        /// Gets current lunar phase for Secunda.
        /// Uses same logic as Enhanced Sky mod so phases should be in sync.
        /// </summary>
        public LunarPhases SecundaLunarPhase
        {
            get { return GetLunarPhase(false); }
        }

        /// <summary>
        /// True from slightly before dusk until slightly after dawn.
        /// Daggerfall NPCs never sleep.
        /// </summary>
        public bool IsCityLightsOn
        {
            get { return (MinuteOfDay >= LightsOnHour || MinuteOfDay < LightsOffHour) ? true : false; }
        }

        /// <summary>
        /// True during the day.
        /// </summary>
        public bool IsDay
        {
            get { return (MinuteOfDay >= DawnHour && MinuteOfDay < DuskHour) ? true : false; }
        }

        /// <summary>
        /// True when full night has fallen.
        /// </summary>
        public bool IsNight
        {
            get { return (MinuteOfDay < DawnHour || MinuteOfDay >= DuskHour) ? true : false; }
        }

        /// <summary>
        /// Gets minute of day 0-1339
        /// </summary>
        public int MinuteOfDay
        {
            get { return GetMinuteOfDay(); }
        }

        /// <summary>
        /// Gets day of month 1-30
        /// </summary>
        public int DayOfMonth
        {
            get { return GetDayOfMonth(); }
        }

        /// <summary>
        /// Gets day of month with suffix (ex: 1st, 2nd, ... 30th)
        /// </summary>
        public string DayOfMonthWithSuffix
        {
            get { return DayOfMonth.ToString() + GetSuffix(Day + 1); }
        }

        /// <summary>
        /// Gets day of year 1-360.
        /// </summary>
        public int DayOfYear
        {
            get { return GetDayOfYear(); }
        }

        /// <summary>
        /// Gets month of year 1-12.
        /// </summary>
        public int MonthOfYear
        {
            get { return GetMonthOfYear(); }
        }

        #endregion

        #region Enums

        /// <summary>
        /// Days of week.
        /// </summary>
        public enum Days
        {
            Sundas,
            Morndas,
            Tirdas,
            Middas,
            Turdas,
            Fredas,
            Loredas,
        }

        /// <summary>
        /// Months of year.
        /// </summary>
        public enum Months
        {
            MorningStar,
            SunsDawn,
            FirstSeed,
            RainsHand,
            SecondSeed,
            Midyear,
            SunsHeight,
            LastSeed,
            Hearthfire,
            Frostfall,
            SunsDusk,
            EveningStar,
        }

        /// <summary>
        /// Birthsigns by month.
        /// </summary>
        public enum BirthSigns
        {
            TheRitual,
            TheLover,
            TheLord,
            TheMage,
            TheShadow,
            TheSteed,
            TheApprentice,
            TheWarrior,
            TheLady,
            TheTower,
            TheAtronach,
            TheThief,
        }

        /// <summary>
        /// Season, ordered as in game.
        /// </summary>
        public enum Seasons
        {
            Fall,
            Spring,
            Summer,
            Winter,
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DaggerfallDateTime()
        {
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="source">Source time to copy from.</param>
        public DaggerfallDateTime(DaggerfallDateTime source)
        {
            Year = source.Year;
            Month = source.Month;
            Day = source.Day;
            Hour = source.Hour;
            Minute = source.Minute;
            Second = source.Second;
        }

        /// <summary>
        /// Construct from time components.
        /// </summary>
        public DaggerfallDateTime(int year, int month, int day, int hour, int minute, float second)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;
            Minute = minute;
            Second = second;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Raise time by seconds. Partial seconds are supported.
        /// </summary>
        /// <param name="seconds">Amount in seconds to raise time values.</param>
        public void RaiseTime(float seconds)
        {
            if (seconds < 0f)
            {
                throw new InvalidOperationException(string.Format("Time increases should always be positive. Got {0}", seconds));
            }
            // Increment seconds by any amount
            Second += seconds;

            // Push remainder up the scale
            if (Second >= SecondsPerMinute)
            {
                int minutes = (int)(Second / SecondsPerMinute);
                Second -= minutes * SecondsPerMinute;
                Minute += minutes;
            }
            if (Minute >= MinutesPerHour)
            {
                int hours = (int)(Minute / MinutesPerHour);
                Minute -= hours * MinutesPerHour;
                Hour += hours;
            }
            if (Hour >= HoursPerDay)
            {
                int days = (int)(Hour / HoursPerDay);
                Hour -= days * HoursPerDay;
                Day += days;
            }
            if (Day >= DaysPerMonth)
            {
                int months = (int)(Day / DaysPerMonth);
                Day -= months * DaysPerMonth;
                Month += months;
            }
            if (Month >= MonthsPerYear)
            {
                int years = (int)(Month / MonthsPerYear);
                Month -= years * MonthsPerYear;
                Year += years;
            }
        }

        /// <summary>
        /// Gets minimum time string of HH:MM.
        /// </summary>
        public string MinTimeString()
        {
            return string.Format("{0:00}:{1:00}", Hour, Minute);
        }

        /// <summary>
        /// Gets a short time string of HH:MM:SS.
        /// </summary>
        public string ShortTimeString()
        {
            return string.Format("{0:00}:{1:00}:{2:00}", Hour, Minute, Second);
        }

        /// <summary>
        /// Gets a mid time string of HH:MM:SS DD MonthName 3EYYY
        /// </summary>
        public string MidDateTimeString()
        {
            return string.Format("{0:00}:{1:00}:{2:00} {3:00} {4:00} 3E{5}", Hour, Minute, Second, Day + 1, MonthName, Year);
        }

        /// <summary>
        /// Gets a long date time string of "HH:MM:SS on Dayname, xth of MonthName, 3EYYY"
        /// </summary>
        public string LongDateTimeString()
        {
            string suffix = GetSuffix(Day + 1);
            string longDateTimeFormatString = TextManager.Instance.GetLocalizedText("longDateTimeFormatString");
            return string.Format(longDateTimeFormatString, Hour, Minute, Second, DayName, Day + 1, suffix, MonthName, Year);
        }

        /// <summary>
        /// Gets a date time string of "HH:MM:SS on xth of MonthName, 3EYYY"
        /// </summary>
        public string DateTimeString()
        {
            string suffix = GetSuffix(Day + 1);
            string dateTimeFormatString = TextManager.Instance.GetLocalizedText("dateTimeFormatString");
            return string.Format(dateTimeFormatString, Hour, Minute, Second, Day + 1, suffix, MonthName, Year);
        }

        /// <summary>
        /// Get date string in format of Day Name the xth of Month Name
        /// </summary>
        public string DateString()
        {
            string suffix = GetSuffix(Day+1);
            string dateFormatString = TextManager.Instance.GetLocalizedText("dateFormatString");
            return string.Format(dateFormatString, DayName, Day + 1, suffix, MonthName);
        }


        /// <summary>
        /// Gets the current time in seconds since year zero.
        /// </summary>
        public ulong ToSeconds()
        {
            long final =
                Convert.ToInt64(SecondsPerYear) * Year +
                SecondsPerMonth * Month +
                SecondsPerDay * Day +
                SecondsPerHour * Hour +
                SecondsPerMinute * Minute +
                (int)Second;

            return (ulong)final;
        }

        /// <summary>
        /// Sets the time using a time expressed in seconds since year zero.
        /// </summary>
        public void FromSeconds(ulong time)
        {
            ulong dayclock, dayno;
            Month = 0;
            Year = 0;

            dayclock = time % SecondsPerDay;
            dayno = time / SecondsPerDay;

            Second = dayclock % SecondsPerMinute;
            Minute = (int)(dayclock % SecondsPerHour) / SecondsPerMinute;
            Hour = (int)dayclock / SecondsPerHour;
            while (dayno >= DaysPerYear)
            {
                dayno -= DaysPerYear;
                Year++;
            }
            while (dayno >= DaysPerMonth)
            {
                dayno -= DaysPerMonth;
                Month++;
            }

            Day = (int)dayno;
        }

        /// <summary>
        /// Gets current time from classic Daggerfall minutes (e.g. found in vanilla save files)
        /// </summary>
        public uint ToClassicDaggerfallTime()
        {
            return (uint)((ToSeconds() - classicEpochInSeconds) / SecondsPerMinute);
        }

        /// <summary>
        /// Sets current time from classic Daggerfall minutes (e.g. found in vanilla save files)
        /// </summary>
        public void FromClassicDaggerfallTime(uint time)
        {
            FromSeconds(classicEpochInSeconds + (time * SecondsPerMinute));
        }

        /// <summary>
        /// Sets the classic game start time of 13:30 4th Morning Star 3E405.
        /// </summary>
        public void SetClassicGameStartTime()
        {
            FromClassicDaggerfallTime(classicGameStartTime);
        }

        /// <summary>
        /// True when date times are equal.
        /// </summary>
        public bool Equals(DaggerfallDateTime other)
        {
            if (other.ToSeconds() == this.ToSeconds())
                return true;
            else
                return false;
        }

        /// <summary>
        /// True when this date time is less than another date time.
        /// </summary>
        public bool LessThan(DaggerfallDateTime other)
        {
            if (this.ToSeconds() < other.ToSeconds())
                return true;
            else
                return false;
        }

        /// <summary>
        /// True when this date time is greater than another date time.
        /// </summary>
        public bool GreaterThan(DaggerfallDateTime other)
        {
            if (this.ToSeconds() > other.ToSeconds())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Clone time to a new instance.
        /// </summary>
        /// <returns>DaggerfallDateTime clone.</returns>
        public DaggerfallDateTime Clone()
        {
            DaggerfallDateTime clone = new DaggerfallDateTime();
            clone.Year = Year;
            clone.Month = Month;
            clone.Day = Day;
            clone.Hour = Hour;
            clone.Minute = Minute;
            clone.Second = Second;

            return clone;
        }

        #endregion

        #region Private Methods

        private string GetDayName()
        {
            if (Day < 0 || Day >= DaysPerMonth)
                RaiseTime(0);

            uint presentTime = ToClassicDaggerfallTime();

            int day = (int)((presentTime / MinutesPerDay) % DaysPerWeek);
            // int day = (int)(Day - (week * DaysPerWeek));

            return TextManager.Instance.GetLocalizedTextList("dayNames")[day];
        }

        private int GetDawnHour()
        {
            (int, Seasons) adjustment = GetDawnDuskAdjustment();

            switch (adjustment.Item2)
            {
                case Seasons.Spring:
                case Seasons.Summer:
                    // Debug.Log("Dawn Hour (Spring/Summer): " + ((BaseDawnHour * SecondsPerHour - adjustment.Item1 / 3) / SecondsPerMinute));
                    return ((BaseDawnHour * SecondsPerHour - adjustment.Item1 / 3) / SecondsPerMinute);
                case Seasons.Fall:
                case Seasons.Winter:
                default:
                    // Debug.Log("Dawn Hour (Fall/Winter): " + ((BaseDawnHour * SecondsPerHour - adjustment.Item1 / 2) / SecondsPerMinute));
                    return ((BaseDawnHour * SecondsPerHour - adjustment.Item1 / 2) / SecondsPerMinute);
            }
        }

        private int GetDuskHour()
        {
            (int, Seasons) adjustment = GetDawnDuskAdjustment();

            switch (adjustment.Item2)
            {
                case Seasons.Spring:
                case Seasons.Summer:
                    // Debug.Log("Dusk Hour (Spring/Summer):" + ((BaseDuskHour * SecondsPerHour + (adjustment.Item1 / 3 * 2)) / SecondsPerMinute));
                    return ((BaseDuskHour * SecondsPerHour + (adjustment.Item1 / 3 * 2)) / SecondsPerMinute);
                case Seasons.Fall:
                case Seasons.Winter:
                default:
                    // Debug.Log("Dusk Hour (Fall/Winter):" + ((BaseDuskHour * SecondsPerHour + adjustment.Item1 / 2) / SecondsPerMinute));
                    return ((BaseDuskHour * SecondsPerHour + adjustment.Item1 / 2) / SecondsPerMinute);
            }
        }

        private int GetLightsOffHour()
        {
            return GetDawnHour() + MinutesPerHour;
        }

        private int GetLightsOnHour()
        {
            return GetDuskHour() - MinutesPerHour;
        }

        private (int, Seasons) GetDawnDuskAdjustment()
        {
            int latitude = GameManager.Instance.PlayerGPS.CurrentMapPixel.Y;
            (int, Seasons) result;

            if (DayOfYear >= SpringStart && DayOfYear < SummerStart)
            {
                result = ((DayOfYear - SpringStart) / 10 * (Equator - latitude), Seasons.Spring);
            }
            else if (DayOfYear >= SummerStart && DayOfYear < FallStart)
            {
                result = ((FallStart - DayOfYear) / 10 * (Equator - latitude), Seasons.Summer);
            }
            else if (DayOfYear >= FallStart && DayOfYear < WinterStart)
            {
                result = (((DayOfYear - FallStart) * -1) / 10 * (Equator - latitude), Seasons.Fall);
            }
            else if (DayOfYear >= WinterStart)
            {
                result = ((DayOfYear - (SpringStart + DaysPerYear)) / 10 * (Equator - latitude), Seasons.Winter);
            }
            else{
                result = ((DayOfYear - SpringStart) / 10 * (Equator - latitude), Seasons.Winter);
            }

            return result;
        }

        private string GetMonthName()
        {
            if (Month < 0 || Month >= MonthsPerYear)
                RaiseTime(0);

            return TextManager.Instance.GetLocalizedTextList("monthNames")[Month];
        }

        private string GetBirthSignName()
        {
            if (Month < 0 || Month >= MonthsPerYear)
                RaiseTime(0);

            return TextManager.Instance.GetLocalizedTextList("birthSignNames")[Month];
        }

        private Seasons GetSeasonValue(bool modifiedSeason = false)
        {
            if (DayOfYear < 0 || DayOfYear >= DaysPerYear)
                RaiseTime(0);

            // {Daggerfall seems to roll over seasons part way through final month.}
            // {Using clean month boundaries here for simplicity.}
            // {Could use DayOfYear ranges instead to be more accurate.}
            // Obsolete, now actually using DayOfYear, plus season start/end date changes based on latitude and altitude:
            // Standard length seasons (halfway between tropic and arctic): y == 2000, hottest latitude (Nirn equator): y == 4700,
            // "Tropic of Cancer" (Tropic of Mage?): y == 3450, "Tropic of Capricorn" (Tropic of Tower?): y == 5950,
            // "Arctic Circle": y == 550 (this last one was determined mathematically, it could need some adjustment).

            int month, day;
            int winterStart = WinterStart;
            int springStart = SpringStart;
            int summerStart = SummerStart;
            int fallStart = FallStart;
            int latitude;
            int altitude;
            int totalDiffFactor, latitudeDiffFactor, altitudeDiffFactor;

            if (modifiedSeason)
            {
                DFPosition position = GameManager.Instance.PlayerGPS.CurrentMapPixel;
                // TODO: Write the function for the Austral Emisphere.
                latitude = position.Y;
                if (latitude > UpperTropic && latitude <= Equator)
                    latitude = UpperTropic;

                altitude = WoodsData.GetHeightMapValue(position.X, position.Y);
                latitudeDiffFactor = (latitude - MiddleTemperate) / 16;
                altitudeDiffFactor = (int)Math.Pow((altitude / 15), 2);

                totalDiffFactor = latitudeDiffFactor - altitudeDiffFactor;
                if (totalDiffFactor < 180) totalDiffFactor = -180;
                if (totalDiffFactor > 180) totalDiffFactor = 180;

                if (totalDiffFactor >= 0) // hotter
                {
                    winterStart += totalDiffFactor / 2;                    
                    springStart -= totalDiffFactor / 2;                    
                    summerStart -= totalDiffFactor;                    
                    fallStart += totalDiffFactor;                    
                }
                else if (totalDiffFactor < 0) // colder
                {
                    winterStart += totalDiffFactor;
                    springStart -= totalDiffFactor;
                    summerStart -= totalDiffFactor / 2;
                    fallStart += totalDiffFactor / 2;
                }

                int winterStartCorr = CorrectDayOfYear(winterStart);
                int springStartCorr = CorrectDayOfYear(springStart);
                int summerStartCorr = CorrectDayOfYear(summerStart);
                int fallStartCorr = CorrectDayOfYear(fallStart);

                if (totalDiffFactor >= 0)
                {
                    // summerStart stays the same;
                    if (springStartCorr > summerStartCorr) springStartCorr = summerStartCorr;
                    if (fallStartCorr > springStartCorr && fallStart > DaysPerYear) fallStartCorr = springStartCorr;
                    if (winterStartCorr > springStartCorr && winterStart > DaysPerYear) winterStartCorr = springStartCorr;

                    if (summerStartCorr == fallStartCorr && fallStartCorr == winterStartCorr && winterStartCorr == springStartCorr) return Seasons.Summer;
                    if (fallStartCorr == winterStartCorr && winterStartCorr == springStartCorr)
                    {
                        if (DayOfYear >= summerStartCorr || DayOfYear < fallStartCorr) return Seasons.Summer;
                        else return Seasons.Spring;
                    }
                    if (winterStartCorr == springStartCorr)
                    {
                        if (DayOfYear >= summerStartCorr || DayOfYear < fallStartCorr) return Seasons.Summer;
                        if (DayOfYear >= springStartCorr && DayOfYear < summerStartCorr) return Seasons.Spring;
                        if ((fallStart > DaysPerYear && DayOfYear >= fallStartCorr && DayOfYear < winterStartCorr) || (fallStart <= DaysPerYear && (DayOfYear >= fallStartCorr || DayOfYear < winterStartCorr))) return Seasons.Fall;
                    }                    

                    if (DayOfYear >= summerStartCorr && DayOfYear < fallStartCorr) return Seasons.Summer;
                    if (DayOfYear >= springStartCorr && DayOfYear < summerStartCorr) return Seasons.Spring;
                    if (winterStart > DaysPerYear)
                    {
                        if (DayOfYear >= fallStartCorr || DayOfYear < winterStartCorr) return Seasons.Fall;
                        if (DayOfYear >= winterStartCorr && DayOfYear < springStartCorr) return Seasons.Winter;
                    }
                    if (winterStart <= DaysPerYear)
                    {
                        if (DayOfYear >= fallStartCorr && DayOfYear < winterStartCorr) return Seasons.Fall;
                        if (DayOfYear >= winterStartCorr || DayOfYear < springStartCorr) return Seasons.Winter;
                    }
                }
                if (totalDiffFactor < 0)
                {
                    // winterStart stays the same;
                    if (fallStartCorr > winterStartCorr) fallStartCorr = winterStartCorr;
                    if (springStartCorr > fallStartCorr) springStartCorr = fallStartCorr;
                    if (summerStartCorr > fallStartCorr) summerStartCorr = fallStartCorr;

                    if (DayOfYear >= winterStartCorr || DayOfYear < springStartCorr) return Seasons.Winter;
                    if (DayOfYear >= fallStartCorr && DayOfYear < winterStartCorr) return Seasons.Fall;
                    if (DayOfYear >= springStartCorr && DayOfYear < summerStartCorr) return Seasons.Spring;
                    if (DayOfYear >= summerStartCorr && DayOfYear < fallStartCorr) return Seasons.Summer;
                }
            }
            
            // month = DayOfYear / DaysPerMonth;
            // day = DayOfYear % DaysPerMonth;

            if (DayOfYear < springStart || DayOfYear >= winterStart)
                return Seasons.Winter;
            if (DayOfYear < summerStart && DayOfYear >= springStart)
                return Seasons.Spring;
            if (DayOfYear < fallStart && DayOfYear >= summerStart)
                return Seasons.Summer;
            if (DayOfYear < winterStart && DayOfYear >= fallStart)
                return Seasons.Fall;

            // switch (month)
            // {
            //     case 0:
            //     case 1:
            //         return Seasons.Winter;
            //     case 2:
            //     case 3:
            //     case 4:
            //         return Seasons.Spring;
            //     case 5:
            //     case 6:
            //     case 7:
            //         return Seasons.Summer;
            //     case 8:
            //     case 9:
            //     case 10:
            //         return Seasons.Fall;
            //     default:
            //         return Seasons.Summer;
            Debug.Log("Unable to select season properly, returning default Spring");
            return Seasons.Spring;
        }

        private int CorrectDayOfYear(int dayOfYear)
        {
            if (dayOfYear > DaysPerYear)
                dayOfYear -= DaysPerYear;
            if (dayOfYear < 0)
                dayOfYear += DaysPerYear;
            return dayOfYear;
        }

        private string GetSeasonName()
        {
            if (Month < 0 || Month >= MonthsPerYear)
                RaiseTime(0);

            return TextManager.Instance.GetLocalizedTextList("seasonNames")[(int)GetSeasonValue()];
        }

        private int GetMinuteOfDay()
        {
            RaiseTime(0);
            return (Hour * MinutesPerHour) + Minute;
        }

        private int GetDayOfMonth()
        {
            RaiseTime(0);
            return Day + 1;
        }

        private int GetDayOfYear()
        {
            RaiseTime(0);
            return (Month * DaysPerMonth) + (Day + 1);
        }

        private int GetMonthOfYear()
        {
            RaiseTime(0);
            return Month + 1;
        }

        private string GetSuffix(int day)
        {
            string suffix = "th";
            if (day == 1 || day == 21)
                suffix = "st";
            else if (day == 2 || day == 22)
                suffix = "nd";
            else if (day == 3 || day == 23)
                suffix = "rd";

            return suffix;
        }

        // Borrowed this code from Lypyl's Enhanched Sky mod and cleaned up just to return phase value
        // This should return same value for lunar phases as Enhanced Sky so lycanthropes will see full moon on days they are forced to change
        // Moon phases are also used by "extra spell pts" item power during "full moon", "half moon", "new moon"
        private LunarPhases GetLunarPhase(bool isMasser)
        {
            // Validate
            if (Year < 0)
            {
                Debug.LogError("GetLunarPhase: Year < 0 not supported.");
                return LunarPhases.None;
            }

            // 3 aligns full moon with vanilla DF for Masser, -1 for secunda
            int offset = (isMasser) ? 3 : -1;

            // Find the lunar phase for current day
            int moonRatio = (DayOfYear + Year * MonthsPerYear * DaysPerMonth + offset) % 32;
            LunarPhases phase = LunarPhases.None;
            if (moonRatio == 0)
                phase = LunarPhases.Full;
            else if (moonRatio == 16)
                phase = LunarPhases.New;
            else if (moonRatio <= 5)
                phase = LunarPhases.ThreeWane;
            else if (moonRatio <= 10)
                phase = LunarPhases.HalfWane;
            else if (moonRatio <= 15)
                phase = LunarPhases.OneWane;
            else if (moonRatio <= 22)
                phase = LunarPhases.OneWax;
            else if (moonRatio <= 28)
                phase = LunarPhases.HalfWax;
            else if (moonRatio <= 31)
                phase = LunarPhases.ThreeWax;

            return phase;
        }

        #endregion
    }
}