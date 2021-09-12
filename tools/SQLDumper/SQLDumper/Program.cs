using System;
using System.Collections.Generic;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.IO;
using SshNet;
using Renci.SshNet;


namespace SQLDumper
{
    class SQLManager
    {

        private MySqlConnection connection = null;

        private bool sitesChecked = false;
        private bool locamChecked = false;
        private bool exageisChecked = false;

        public struct DomainToDBName
        {
            public string domain;
            public string dbName;
        }

        public MySqlConnection GetConnection()
        {
            return connection;
        }

        public List<DomainToDBName> ParseForDBNames(string filePath)
        {
            var domainsToDBNames = new List<DomainToDBName>();
            long emptyDBNameCount = 0;

            using (StreamReader reader = System.IO.File.OpenText(filePath))
            {
                using (StreamWriter writer = new StreamWriter("emptyDbNames.txt"))
                {
                    reader.ReadLine();
                    while (reader.Peek() >= 0)
                    {
                        try
                        {
                            string line = reader.ReadLine();
                            string[] attributes = line.Split(";");

                            Console.WriteLine(attributes[1] + " - " + attributes[2]);

                            if (attributes[2] == "")
                            {
                                emptyDBNameCount++;
                                writer.WriteLine("Database name manquante pour : " + attributes[1]);
                            }
                            else
                            {
                                DomainToDBName lineExample;
                                lineExample.domain = attributes[1];
                                lineExample.dbName = attributes[2];
                                domainsToDBNames.Add(lineExample);
                            }


                        }
                        catch (NullReferenceException) { Console.WriteLine("Pas celui là"); }
                    }
                }

            }
            Console.WriteLine("Empty db name count : " + emptyDBNameCount);
            return domainsToDBNames;
        }

        public bool Connect(string dbName)
        {
            try
            {
                string connstring = "Server=xxxxx" + ";" +
                                    "Database=" + dbName + ";" +
                                    "UID=xxxxx" + ";" +
                                    "Password=xxxxxxx" + ";";

                connection = new MySqlConnection(connstring);

                connection.Open();
            }
            catch (Exception)
            {
                Console.WriteLine("La connexion n'a pas pu avoir lieu");
                return false;
            }

            return true;
        }

        public bool Disconnect()
        {
            if (connection == null)
            {
                Console.WriteLine("La connexion n'a pas été établie, le disconnect n'a pas pu avoir lieu");
                return false;
            }

            connection.Close();
            connection = null;

            return true;
        }

        public void GetAllDatabaseNames()
        {
            string strCommand = "SELECT * FROM TABLES";
            var sqlCommand = new MySqlCommand(strCommand, connection);
            MySqlDataReader reader = sqlCommand.ExecuteReader();

            string result = "";

            while (reader.Read())
            {
                result += reader.GetValue(0).ToString();
                result += "\n";
            }

            Console.WriteLine("Result size : " + result.Length + "\n" + result);

        }

        public string GetWPPrefixFromSftp(SftpClient sftp, string location, string domain)
        {
            string filePath = "/" + location + "/" + domain + "/website/wp-config.php";
            Console.WriteLine(filePath);
            using (var remoteFileStream = sftp.OpenRead(filePath))
            {
                var textReader = new System.IO.StreamReader(remoteFileStream);
                while (textReader.Peek() >= 0)
                {
                    var line = textReader.ReadLine();
                    if (line.Length > 13)
                    {
                        if (line.Substring(0, 13) == "$table_prefix")
                        {
                            string prefix = line.Split("=")[1].Replace(" ", "").Replace(";", "").Replace("'", "");
                            Console.WriteLine("Prefix is : " + prefix);

                            return prefix;
                        }
                    }

                }
            }

            return "wp_";
        }

        public string GetWPPrefix(string domain)
        {
            using (var sftp = new SftpClient("ip", "username", "password"))
            {
                try
                {
                    sftp.Connect();

                    try { return GetWPPrefixFromSftp(sftp, "sites", domain); }
                    catch (Exception)
                    {
                        try { return GetWPPrefixFromSftp(sftp, "locam", domain); }
                        catch (Exception)
                        {
                            try { return GetWPPrefixFromSftp(sftp, "exaegis", domain); }
                            catch (Exception)
                            {
                                Console.WriteLine("Prefix introuvable");
                            }
                        }
                    }

                    sftp.Disconnect();
                }
                catch (Exception e)
                {
                    sftp.Disconnect();
                }
            }

            return "wp_";
        }

        public string GetDBNameFromWPConfig(SftpClient sftp, string location, string domain)
        {
            string filePath = "/" + location + "/" + domain + "/website/wp-config.php";
            Console.WriteLine(filePath);
            using (var remoteFileStream = sftp.OpenRead(filePath))
            {
                var textReader = new System.IO.StreamReader(remoteFileStream);
                while (textReader.Peek() >= 0)
                {
                    var line = textReader.ReadLine();
                    if (line.Length > 17)
                    {
                        if (line.Substring(0, 17) == "define( 'DB_NAME'")
                        {
                            string dbName = line.Split(",")[1].Replace(" ", "").Replace(";", "").Replace(")", "").Replace("'", "");
                            Console.WriteLine("Prefix is : " + dbName);

                            return dbName;
                        }
                    }

                }
            }

            return " ";
        }

        public string GetDBName(string domain)
        {
            using (var sftp = new SftpClient("xxxxxx", "xxxxx", "xxxxx"))
            {
                try
                {
                    sftp.Connect();

                    try { return GetDBNameFromWPConfig(sftp, "sites", domain); }
                    catch (Exception)
                    {
                        try { return GetDBNameFromWPConfig(sftp, "locam", domain); }
                        catch (Exception)
                        {
                            try { return GetDBNameFromWPConfig(sftp, "exaegis", domain); }
                            catch (Exception)
                            {
                                Console.WriteLine("DBName introuvable");
                            }
                        }
                    }

                    sftp.Disconnect();
                }
                catch (Exception e)
                {
                    sftp.Disconnect();
                }
            }

            return " ";
        }

        public bool HasCaptcha(string domain)
        {
            string prefix = GetWPPrefix(domain);

            string strCommand = "SELECT option_id FROM " + prefix + "options WHERE option_name='et_core_api_spam_options'";
            var sqlCommand = new MySqlCommand(strCommand, connection);
            MySqlDataReader reader = sqlCommand.ExecuteReader();

            string result = "";

            while (reader.Read())
            {
                result += reader.GetValue(0).ToString();
                result += "\n";
            }

            reader.Close();

            if (result.Length == 0) { return false; }

            string optionValue = "{s:9:\"recaptcha\";a:0:{}}";
            strCommand = "SELECT option_id FROM " + prefix + "options WHERE option_name='et_core_api_spam_options' AND option_value LIKE '%" + optionValue + "%'";
            sqlCommand = new MySqlCommand(strCommand, connection);
            reader = sqlCommand.ExecuteReader();

            result = "";

            while (reader.Read())
            {
                result += reader.GetValue(0).ToString();
                result += "\n";
            }

            reader.Close();

            Console.WriteLine("Result size : " + result.Length + "\n" + result);

            if (result.Length > 0) { return false; }

            return true;
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            SQLManager manager = new SQLManager();

            var domainToDbName = manager.ParseForDBNames("sites_wp.csv");
            var indexToDic = new Dictionary<int, SQLManager.DomainToDBName>();
            for (int i = 0; i < domainToDbName.Count; i++)
            {
                indexToDic.Add(i, domainToDbName[i]);
            
                Console.WriteLine(i + " : " + domainToDbName[i].domain + " : " + domainToDbName[i].dbName);
            }
            
            int recaptchaOn  = 0;
            int recaptchaOff = 0;
            
            using (StreamWriter writer = new StreamWriter("captchaResult2.csv"))
            {
                for (int i = 0; i < indexToDic.Count; i++)
                {
                    string correctDBName = indexToDic[i].dbName;
                    Console.WriteLine(i + ";" + indexToDic[i].dbName + ";" + indexToDic[i].domain + ";");
                    Console.WriteLine("recaptchaOn=" + recaptchaOn + ";" + "recaptchaOff=" + recaptchaOff);
                    try
                    {
                        string wpConfigDBName = manager.GetDBName(indexToDic[i].domain);
                        if (indexToDic[i].dbName != wpConfigDBName && wpConfigDBName != " ") { correctDBName = wpConfigDBName; }
                        manager.Connect(correctDBName);
                        if (manager.HasCaptcha(indexToDic[i].domain))
                        {
                            writer.WriteLine(i + ";" + correctDBName + ";" + indexToDic[i].domain + ";" + "CAPTCHA ON");
                            recaptchaOn++;
                        }
                        else
                        {
                            writer.WriteLine(i + ";" + correctDBName + ";" + indexToDic[i].domain + ";" + "CAPTCHA OFF");
                            recaptchaOff++;
                        }
                        manager.Disconnect();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Console.WriteLine(correctDBName + " : " + i + "eme esquivé, au suivant");
                        writer.WriteLine(correctDBName + " : " + i + "eme esquivé, au suivant");
                        manager.Disconnect();
                    }
                }
            
                writer.WriteLine("ON=" + recaptchaOn + ";" + "OFF=" + recaptchaOff);
            }
        }

    }
}
