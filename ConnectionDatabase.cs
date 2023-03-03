using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ELMA_API
{
    class ConnectionDatabase
    {
        // TrustServerCertificate=true обязательно!! иначе будет ошибка TLS протокола
        SqlConnection sqlConnection = new SqlConnection(@"Data Source=10.0.0.26;Initial Catalog=Деканат;Integrated Security=True; TrustServerCertificate=true");

        public ConnectionDatabase() 
        {
            // Logging 
            Log.Success(SuccessTitle.loginDB, "connection to database is successful");
        }

        public void openConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Closed)
            {
                sqlConnection.Open();
                Log.Success("login-db", "connection to database ms sql server is successful");
            }
        }

        public void closeConnection()
        {
            if (sqlConnection.State == System.Data.ConnectionState.Open)
            {
                sqlConnection.Close();
            }
        }

        public SqlConnection getConnection()
        {
            return sqlConnection;
        }
    }
}
