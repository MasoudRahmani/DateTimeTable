using System;
using System.Data;
using System.Windows;

namespace DateTimeTable.Logic
{

    public class DateCreator
    {
        private SQLConnectionHelper _sqlhelper;
        public DateCreator(string server, string database, string tablename)
        {
            _sqlhelper = new SQLConnectionHelper(server)
            {
                Database = database,
                TableName = tablename
            };

        }
        public void Start(DateTime start, DateTime end)
        {
            var dtscheme = CreateSchemeTable();
            var calculator = new DateCalculator();
            var dt = calculator.CalculateDates(dtscheme, start, end);

            try
            {
                _sqlhelper.BulkInsertData(dt);
            }
            catch (Exception w)
            {
                MessageBox.Show(w.InnerException.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>table Scheme</returns>
        private DataTable CreateSchemeTable()
        {
            var createCommand = string.Concat("CREATE TABLE dbo.[", _sqlhelper.TableName, "]( 	[DateKey] [INT] NOT NULL, 	[GregorianDate] [DATE] NULL, 	[GregorianYearInt] [SMALLINT] NULL, 	[GregorianMonthNo] [TINYINT] NULL, 	[GregorianDayInMonth] [TINYINT] NULL, 	[GregorianMonthDayInt] [SMALLINT] NULL, 	[GregorianDayOfWeekInt] [TINYINT] NULL, 	[GregorianMonthName] [NVARCHAR](20) NULL, 	[GregorianStr] [CHAR](10) NULL, 	[GregorianYearMonthInt] [INT] NULL, 	[GregorianYearMonthStr] [CHAR](7) NULL, 	[GregorianDayOfWeekName] [NVARCHAR](20) NULL, 	[GrgorianWeekOfYearName] [NVARCHAR](20) NULL, 	[GregorianWeekOfYearNo] [INT] NULL, 	[PersianInt] [INT] NULL, 	[PersianYearInt] [SMALLINT] NULL, 	[PersianMonthNo] [TINYINT] NULL, 	[PersianDayInMonth] [TINYINT] NULL, 	[PersianMonthDayInt] [SMALLINT] NULL, 	[PersianDayOfWeekInt] [TINYINT] NULL, 	[PersianMonthName] [NVARCHAR](20) NULL, 	[PersianStr] [CHAR](10) NULL, 	[PersianYearMonthInt] [INT] NULL, 	[PersianYearMonthStr] [CHAR](7) NULL, 	[PersianDayOfWeekName] [NVARCHAR](20) NULL, 	[PersianWeekOfYearName] [NVARCHAR](20) NULL, 	[PersianWeekOfYearNo] [INT] NULL, 	[PersianFullName] [NVARCHAR](60) NULL, 	[HijriInt] [INT] NULL, 	[HijriYearInt] [SMALLINT] NULL, 	[HijriMonthNo] [TINYINT] NULL, 	[HijriDayInMonth] [TINYINT] NULL, 	[HijriMonthDayInt] [SMALLINT] NULL, 	[HijriDayOfWeekInt] [TINYINT] NULL, 	[HijriMonthName] [NVARCHAR](20) NULL, 	[HijriStr] [CHAR](10) NULL, 	[HijriYearMonthInt] [INT] NULL, 	[HijriYearMonthStr] [CHAR](7) NULL, 	[HijriDayOfWeekName] [NVARCHAR](20) NULL, 	[HijriWeekOfYearName] [NVARCHAR](20) NULL, 	[HijriWeekOfYearNo] [INT] NULL, 	[SeasonCode] [TINYINT] NULL, 	[PersianSeasonName] [NVARCHAR](50) NULL, 	[IsGregorianLeap] [BIT] NULL, 	[IsPersianLeap] [BIT] NULL, 	[IsOneDayBefore_PersianHoliday] [BIT] NULL, 	[IsOneDayBefore_HijriHoliday] [BIT] NULL,  CONSTRAINT [Pk_", _sqlhelper.TableName, "Key] PRIMARY KEY CLUSTERED  ( 	[DateKey] ASC ) ) ON [PRIMARY]");

            _sqlhelper.CreateTable(createCommand);

            return _sqlhelper.GetTableScheme();
        }

    }
}
