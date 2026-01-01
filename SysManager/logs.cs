using System;
using System.IO;

namespace SysManager
{
    public static class Logs
    {
        private static readonly string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

        public static void Write(string message)
        {
            try
            {
                if (!Directory.Exists(logDirectory))
                    Directory.CreateDirectory(logDirectory);

                string fileName = $"system_{DateTime.Now:dd.MM.yyyy}.txt";
                string filePath = Path.Combine(logDirectory, fileName);

                string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(filePath, logEntry);
            }
            catch
            {
                // Evităm aruncarea excepțiilor din log — nu vrem crash doar din cauza unui fișier blocat
            }
        }

        public static void Write(Exception ex)
        {
            Write($"ERROR: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }
    }
}
