using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace CloudServer.Database
{
    /// <summary>
    /// Deprecated - Forum Integration
    /// </summary>
    public class Connect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string username;
        private string password;

        public Connect()
        {
            #region Database Login Details
            server = "127.0.0.1";
            database = "cloud";
            username = "cloud";
            password = "@UrDrive337859185";
            #endregion

            connection = new MySqlConnection("SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + username + ";" + "PASSWORD=" + password + ";");

            connection.Open();
        }

        public string Version()
        {
            string Version = "";

            //Handle Manual Products
            using (MySqlCommand command = new MySqlCommand("SELECT * FROM version", connection))
            {
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Console.WriteLine("Reading Version");

                        Version = ((!reader.IsDBNull(0)) ? reader.GetString(0) : "0.00");
                    }
                }
            }

            return Version;
        }

        /// <summary>
        /// Checks Database for Login and returns true if it was found
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="HWID"></param>
        /// <returns></returns>
        public string Login(string Username, string Password, string HWID)
        {
            MySqlCommand com = new MySqlCommand("SELECT Count(*) FROM Logins WHERE Username=@Username AND Password=@Password AND HWID=@HWID", connection);
            com.Parameters.AddWithValue("@Username", Username);
            com.Parameters.AddWithValue("@Password", Password);
            com.Parameters.AddWithValue("@HWID", HWID);

            int count = (int)com.ExecuteScalar();

            if (count > 0)
            {
                return "Login Found";
            }
            else
            {
                com = new MySqlCommand("SELECT Count(*) FROM Logins WHERE Username=@Username AND Password=@Password", connection);
                com.Parameters.AddWithValue("@Username", Username);
                com.Parameters.AddWithValue("@Password", Password);

                count = (int)com.ExecuteScalar();

                Console.WriteLine("There are " + count.ToString() + " Accounts linked to these credentials.");

                if (count != 0)
                {
                    //If Invalid HWID - Check if HWID Reset is triggered
                    com = new MySqlCommand("SELECT HWReset FROM Logins WHERE Username=@Username", connection);
                    com.Parameters.AddWithValue("@Username", Username);

                    if ((int)com.ExecuteScalar() > 0)
                    {
                        //Reset HWID
                        com = new MySqlCommand("UPDATE Logins SET HWReset=0, HWID=@HWID WHERE Username=@Username", connection);
                        com.Parameters.AddWithValue("@Username", Username);
                        com.Parameters.AddWithValue("@HWID", HWID);

                        if ((int)com.ExecuteNonQuery() > 0)
                        {
                            return "Login Found";
                        }
                        else
                        {
                            return "Failed to Reset HWID";
                        }
                    }
                    else
                    {
                        return "Invalid HWID";
                    }
                }
                else
                {
                    return "Invalid Password";
                }
            }
        }

        /// <summary>
        /// Registers Username, Password and HWID
        /// </summary>
        /// <param name="Username"></param>
        /// <param name="Password"></param>
        /// <param name="HWID"></param>
        /// <returns></returns>
        public string Register(string Username, string Password, string Salt, string HWID, string IP)
        {
            MySqlCommand com = new MySqlCommand("SELECT Count(*) FROM Logins WHERE Username=@Username", connection);
            com.Parameters.AddWithValue("@Username", Username);

            int count = (int)com.ExecuteScalar();

            if (count > 0)
                return "Username Taken";

            com = new MySqlCommand("SELECT Count(*) FROM Logins WHERE HWID=@HWID", connection);
            com.Parameters.AddWithValue("@HWID", HWID);

            count = (int)com.ExecuteScalar();

            if (count > 0)
            {
                com = new MySqlCommand("SELECT * FROM Logins WHERE HWID=@HWID", connection);
                com.Parameters.AddWithValue("@HWID", HWID);

                using (MySqlDataReader reader = com.ExecuteReader())
                    if (reader.Read())
                        return "HWID Taken by (" + reader.GetString(reader.GetOrdinal("Username")) + ")";
                    else
                        return "HWID Taken";
            }

            com = new MySqlCommand("INSERT INTO Logins (Username, Password, Salt, HWID, IP, Banned, Admin) VALUES (@Username, @Password, @Salt, @HWID, @IP, 0, 0)", connection);
            com.Parameters.AddWithValue("@Username", Username);
            com.Parameters.AddWithValue("@Password", Password);
            com.Parameters.AddWithValue("@Salt", Salt);
            com.Parameters.AddWithValue("@HWID", HWID);
            com.Parameters.AddWithValue("@IP", IP);
            com.ExecuteNonQuery();

            return "Registeration Successful";
        }

        public bool SyncFolder()
        {
            return true;
        }

        public bool SyncFile()
        {
            return true;
        }

        public void Close()
        {
            connection.Close();
        }
    }
}
