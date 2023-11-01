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
            CreateFunctions();
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

        private DataTable CreateSchemeTable()
        {
            var createCommand = Properties.Resources.Table.Replace("[dbo].[DimDate]", $"[dbo].[{_sqlhelper.TableName}]").Replace("Pk_DimDateKey", $"Pk_{_sqlhelper.TableName}Key");
            _sqlhelper.CreateTable(createCommand);

            return _sqlhelper.GetTableScheme();
        }

        private void CreateFunctions()
        {
            if (_sqlhelper.HasRows("SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetShamsi]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' );") == false)
            {
                var getShamsi_cmd = Properties.Resources.GetShamsi.Replace("FROM dbo.DimDate", $"FROM dbo.{_sqlhelper.TableName}");
                _sqlhelper.CreateFunction(getShamsi_cmd);
            }
            if (_sqlhelper.HasRows("SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMiladi]') AND type IN ( N'FN', N'IF', N'TF', N'FS', N'FT' )") == false)
            {
                var getMiladi_cmd = Properties.Resources.GetMiladi.Replace("FROM dbo.DimDate", $"FROM dbo.{_sqlhelper.TableName}");
                _sqlhelper.CreateFunction(getMiladi_cmd);
            }
        }

    }
}
