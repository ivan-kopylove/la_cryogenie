using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace La_cryogenie
{
    public static class Sqlite
    {
        private const string dataSourceFile = @"C:\Users\i.kopylov\Desktop\Antiphishing\antiphishing.sqlite";

        public static DataTable executeSearch(string sqliteCommand)
        {
            SQLiteConnectionStringBuilder connBuilder = new SQLiteConnectionStringBuilder();
            connBuilder.DataSource = dataSourceFile;
            connBuilder.Version = 3;
            connBuilder.DefaultTimeout = 400;

            SQLiteConnection connection = new SQLiteConnection(connBuilder.ToString());
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = sqliteCommand;
                command.CommandType = CommandType.Text;
                SQLiteDataReader reader = command.ExecuteReader();

                DataTable dt = new DataTable();
                dt.Load(reader);
                return dt;
            }
        }

        public static void executeVoid(string sqliteCommand)
        {
            SQLiteConnectionStringBuilder connBuilder = new SQLiteConnectionStringBuilder();
            connBuilder.DataSource = dataSourceFile;
            connBuilder.Version = 3;
            connBuilder.DefaultTimeout = 400;

            SQLiteConnection connection = new SQLiteConnection(connBuilder.ToString());
            connection.Open();

            using (SQLiteCommand command = new SQLiteCommand(connection))
            {

                command.CommandText = sqliteCommand;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }

            connection.Close();
            connection.Dispose();

        }
    }
}
