using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Text;

class Database
{
    private MySqlConnection connection;
    private string server;
    private string database;
    private string uid;
    private string password;


    public Database()
    {
        Initialise();
    }


    private void Initialise()
    {
        server = "localhost";
        database = "motie";
        uid = "administrator";
        password = "123";
        string connectionString = "Server=" + server + ";" + "Database=" +
        database + ";" + "UID=" + uid + ";" + "Password=" + password + ";";

        connection = new MySqlConnection(connectionString);
    }

    private bool OpenConnection()
    {
        try
        {
            connection.Open();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    private bool CloseConnection()
    {
        try
        {
            connection.Clone();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public bool TruncateTable()
    {
        try
        {
            if (OpenConnection())
            {
                MySqlCommand sql = new MySqlCommand("TRUNCATE TABLE motie;", connection);
                sql.ExecuteNonQuery();
                Console.WriteLine("Succesfully trancuted table");
            }
            else
            {
                Console.WriteLine("Connection was closed");
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public bool InsertInto(string qr)
    {
        try
        {
            if (OpenConnection() == true)
            {
                try
                {
                    MySqlCommand command = new MySqlCommand(qr, connection);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
                catch (MySqlException MysqlEx)
                {
                    if (MysqlEx.Number.Equals(1062))
                    {
                        Console.WriteLine("Motie was already implemented");
                        return false;
                    }
                    Console.WriteLine(MysqlEx);
                    this.CloseConnection();
                    return false;
                }
                Console.WriteLine("Inserting was a success");
                return true;
            }
            else
            {
                Console.WriteLine("Connection was closed");
                return false;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

}
