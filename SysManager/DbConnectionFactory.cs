// DbConnectionFactory.cs
using System;
using System.IO;
using FirebirdSql.Data.FirebirdClient;

namespace SysManager
{
    public static class DbConnectionFactory
    {
        // Folosim System.AppDomain explicit
        private static readonly string AppFolder = System.AppDomain.CurrentDomain.BaseDirectory;

        private static readonly string DatabasePath = Path.Combine(AppFolder, "data", "data.fdb");

        private static readonly string _connectionString = new FbConnectionStringBuilder
        {
            DataSource = "localhost",
            Port = 3052,
            Database = DatabasePath,
            UserID = "SYSDBA",
            Password = "masterkey",
            Charset = "UTF8",
            Pooling = true
        }.ToString();

        public static FbConnection GetOpenConnection()
        {
            var conn = new FbConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
