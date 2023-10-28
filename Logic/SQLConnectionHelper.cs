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
        private string _cnnSTR = string.Empty;
        private string _user = string.Empty;
        private string _password = string.Empty;

        #region Property
        public string Server { get; }
        public string Database { get; set; }
        public string TableName { get; set; }
        public bool IntegratedSecurity { get; }

        #endregion
        /// <summary>
        /// Initialise Helper with Integrated Security as Enabled
        /// </summary>
        public SQLConnectionHelper(string server)
        {
            Server = server;
            IntegratedSecurity = true;
        }
        /// <summary>
        /// Initialise Helper with Integrated Security as false therefore requires user and pass
        /// </summary>
        public SQLConnectionHelper(string server, string username, string password)
        {
            IntegratedSecurity = false;
            Server = server;
            _user = username;
            _password = password;
        }

        public SqlConnection GetSqlConnection(ConnectionStringConfig instanceType, bool encrypted, bool trustCert)
        {
            SqlConnectionStringBuilder cnstr = GetConnectionStringBuilder(encrypted, trustCert);

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

        private SqlConnectionStringBuilder GetConnectionStringBuilder(bool encrypted, bool trustCert)
        {
            SqlConnectionStringBuilder cnstr;
            if (IntegratedSecurity)
            {
                cnstr = new SqlConnectionStringBuilder
                {
                    DataSource = Server,
                    IntegratedSecurity = IntegratedSecurity,
                    Encrypt = encrypted, //Either have encyption set to false or trust certificate
                    TrustServerCertificate = trustCert // in new version when server doesnt have certificate or some problem
                };
            }
            else
            {
                cnstr = new SqlConnectionStringBuilder
                {
                    DataSource = Server,
                    IntegratedSecurity = IntegratedSecurity,
                    UserID = _user,
                    Password = _password,
                    Encrypt = encrypted, //Either have encyption set to false or trust certificate
                    TrustServerCertificate = trustCert // in new version when server doesnt have certificate or some problem
                };
            }

            return cnstr;
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

        public bool CheckCredential()
        {
            try
            {
                var d = GetSqlConnection(ConnectionStringConfig.OnlyServer, true, true);
                d.Open();
                d.Close();
                d.Dispose();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }

    public enum ConnectionStringConfig
    {
        OnlyServer,
        WithDatabase
    }

}
