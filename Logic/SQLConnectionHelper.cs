using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DateTimeTable.Logic
{
    public class SQLConnectionHelper
    {
        private string _cnnSTR;

        #region Property
        public string Server { get; }
        public string Database { get; set; }
        public string TableName { get; set; }
        public bool IntegratedSecurity { get; }
        #endregion

        public SQLConnectionHelper(string server, bool integrated)
        {
            Server = server;
            IntegratedSecurity = integrated;
        }

        public SqlConnection GetSqlConnection(ConnectionStringConfig instanceType, bool encrypted, bool trustCert)
        {
            var cnstr = new SqlConnectionStringBuilder
            {
                DataSource = Server,
                IntegratedSecurity = IntegratedSecurity,
                Encrypt = encrypted, //Either have encyption set to false or trust certificate
                TrustServerCertificate = trustCert // in new version when server doesnt have certificate or some problem
            };

            SqlConnection sqlConnection;
            switch (instanceType)
            {
                case ConnectionStringConfig.OnlyServer:
                    {
                        _cnnSTR = cnstr.ToString();
                        sqlConnection = new SqlConnection(_cnnSTR);
                        return sqlConnection;
                    }
                case ConnectionStringConfig.WithDatabase:
                    {
                        cnstr.InitialCatalog = Database;
                        _cnnSTR = cnstr.ToString();
                        sqlConnection = new SqlConnection(_cnnSTR);
                        return sqlConnection;
                    }
                default:
                    throw new Exception("No Instance");
            }
        }
        public IEnumerable<object> GetDatabases()
        {
            using (var _sqlcnn = GetSqlConnection(ConnectionStringConfig.OnlyServer, true, true))
            {
                _sqlcnn.Open();
                var d = _sqlcnn.GetSchema("Databases");
                _sqlcnn.Close();

                return (from DataRow rows in d.Rows select rows[0]).ToList();
            }
        }
        public void CreateTable(string createCommand)
        {
            using (var sqlcnn = GetSqlConnection(ConnectionStringConfig.WithDatabase, true, true))
            {
                sqlcnn.Open();
                using (var sqlCmd = new SqlCommand(createCommand, sqlcnn))
                {
                    sqlCmd.ExecuteNonQuery();
                }
                sqlcnn.Close();
            }
        }
        public DataTable GetTableScheme()
        {
            var dt = new DataTable();
            using (var sqlcnn = GetSqlConnection(ConnectionStringConfig.WithDatabase, true, true))
            {
                sqlcnn.Open();
                using (var adaptor = new SqlDataAdapter(string.Concat("select * from ", TableName), sqlcnn))
                {
                    adaptor.FillSchema(dt, SchemaType.Source);
                }
            }
            return dt;
        }
        public void BulkInsertData(DataTable dataTable)
        {
            using (var sqlcnn = GetSqlConnection(ConnectionStringConfig.WithDatabase, true, true))
            {
                sqlcnn.Open();
                using (var bulk = new SqlBulkCopy(sqlcnn))
                {
                    bulk.DestinationTableName = TableName;

                    bulk.WriteToServer(dataTable);

                }
                sqlcnn.Close();
            }

        }
    }

    public enum ConnectionStringConfig
    {
        OnlyServer,
        WithDatabase
    }

}
