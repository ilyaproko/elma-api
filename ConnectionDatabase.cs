using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace ELMA_API;
class ConnectionDatabase
{
    // TrustServerCertificate=true обязательно!! иначе будет ошибка TLS протокола
    SqlConnection sqlConnection;

    public ConnectionDatabase(string dataSource, string initialCatalog)
    {
        this.sqlConnection =
            new SqlConnection($"Data Source={dataSource};Initial Catalog={initialCatalog};Integrated Security=True; TrustServerCertificate=true");
    }

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