using System;
using System.Data.SqlClient;
using System.Linq;
using MySql.Data.MySqlClient;

class Database
{
    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

    public Database()
    {
        builder.DataSource = "localhost";
        builder.UserID = "SA";
        builder.Password = "*&gFaevH#2dba47";
        builder.InitialCatalog = "Climate_Change_Dashboard";
    }


    public void TruncateDatabase()
    {

        try
        {
            string qr = "TRUNCATE TABLE motie;";
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(qr, connection))
                {
                    connection.Open();

                    command.CommandText = qr;
                    command.Prepare();
                    command.ExecuteNonQuery();
                    
                    connection.Close();

                }
            }
            Console.WriteLine("Truncte was a success");
        }
        catch (MySqlException MysqlEx)
        {
            Console.WriteLine(MysqlEx);
        }

    }


    public bool InsertInto(string qr)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(builder.ConnectionString))
            {
                using (SqlCommand command = new SqlCommand(qr, connection))
                {
                    connection.Open();

                    command.CommandText = qr;
                    command.Prepare();
                    command.ExecuteNonQuery();

                    Console.WriteLine("Motie Implemented");

                    connection.Close();

                    return true;
                }
            }

        }
        catch (MySqlException MysqlEx)
        {
            if (MysqlEx.Number.Equals(1062))
            {
                Console.WriteLine("Motie was already implemented");
                return false;
            }
            Console.WriteLine(MysqlEx);
            return false;
        }
        Console.WriteLine("Inserting was a success");
        return true;
    }

}
