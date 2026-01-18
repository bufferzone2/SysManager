// Locație: SysManager/Managers/ConfigManager.cs

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SysManager.Managers
{
    /// <summary>
    /// Manager pentru citirea și gestionarea configurației din fișierul config.ini
    /// Pattern: Singleton (o singură instanță în toată aplicația)
    /// </summary>
    public class ConfigManager
    {
        #region === SINGLETON PATTERN ===

        private static ConfigManager _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// Instanța unică a ConfigManager (Singleton)
        /// Accesează astfel: ConfigManager.Instance.Database.SrvAddress
        /// </summary>
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ConfigManager();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region === PROPRIETĂȚI CONFIGURAȚIE ===

        /// <summary>
        /// Configurări bază de date
        /// </summary>
        public DatabaseConfig Database { get; private set; }

        /// <summary>
        /// Configurări generale aplicație
        /// </summary>
        public AppConfig Config { get; private set; }

        /// <summary>
        /// Configurări AMEF (Aparat Marcat Electronice Fiscale)
        /// </summary>
        public AmefConfig Amef { get; private set; }

        /// <summary>
        /// Tipuri de plată disponibile
        /// </summary>
        public TipPlataConfig TipPlata { get; private set; }

        /// <summary>
        /// Configurări server web
        /// </summary>
        public WebConfig Web { get; private set; }

        /// <summary>
        /// Calea completă către fișierul config.ini
        /// </summary>
        public string ConfigFilePath { get; private set; }

        /// <summary>
        /// Verifică dacă fișierul de configurare există
        /// </summary>
        public bool ConfigFileExists => File.Exists(ConfigFilePath);

        #endregion

        #region === CONSTRUCTOR PRIVAT (SINGLETON) ===

        /// <summary>
        /// Constructor privat - se apelează doar intern pentru Singleton
        /// </summary>
        private ConfigManager()
        {
            // Determină calea către fișierul config.ini
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string configDirectory = Path.Combine(appDirectory, "config");
            ConfigFilePath = Path.Combine(configDirectory, "config.ini");

            Logs.Write($"ConfigManager: Cale config.ini = {ConfigFilePath}");

            // Încarcă configurația
            LoadConfiguration();
        }

        #endregion

        #region === ÎNCĂRCARE CONFIGURAȚIE ===

        /// <summary>
        /// Încarcă configurația din fișierul INI
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                {
                    Logs.Write($"⚠️ WARNING: Fișierul config.ini nu există la {ConfigFilePath}");
                    Logs.Write("⚠️ Se vor folosi valori default");
                    CreateDefaultConfig();
                    return;
                }

                Logs.Write("ConfigManager: Începe citirea config.ini");

                // Inițializează obiectele de configurare
                Database = new DatabaseConfig();
                Config = new AppConfig();
                Amef = new AmefConfig();
                TipPlata = new TipPlataConfig();
                Web = new WebConfig();

                // Citește fișierul
                string[] lines = File.ReadAllLines(ConfigFilePath, Encoding.UTF8);
                string currentSection = "";

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();

                    // Ignoră linii goale și comentarii
                    if (string.IsNullOrWhiteSpace(trimmedLine) ||
                        trimmedLine.StartsWith(";") ||
                        trimmedLine.StartsWith("#"))
                    {
                        continue;
                    }

                    // Detectează secțiune [NUME_SECTIUNE]
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2).ToUpper();
                        continue;
                    }

                    // Parsează pereche cheie=valoare
                    int separatorIndex = trimmedLine.IndexOf('=');
                    if (separatorIndex > 0)
                    {
                        string key = trimmedLine.Substring(0, separatorIndex).Trim();
                        string value = trimmedLine.Substring(separatorIndex + 1).Trim();

                        // Atribuie valoarea la secțiunea corespunzătoare
                        AssignValue(currentSection, key, value);
                    }
                }

                Logs.Write("✅ ConfigManager: Configurație încărcată cu succes");
                LogConfiguration();
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la încărcarea configurației:");
                Logs.Write(ex);
                CreateDefaultConfig();
            }
        }

        /// <summary>
        /// Atribuie valoarea citită la proprietatea corespunzătoare
        /// </summary>
        private void AssignValue(string section, string key, string value)
        {
            switch (section)
            {
                case "DATABASE":
                    AssignDatabaseValue(key, value);
                    break;
                case "CONFIG":
                    AssignConfigValue(key, value);
                    break;
                case "AMEF":
                    AssignAmefValue(key, value);
                    break;
                case "TIP_PLATA":
                    AssignTipPlataValue(key, value);
                    break;
                case "WEB":
                    AssignWebValue(key, value);
                    break;
                default:
                    Logs.Write($"⚠️ Secțiune necunoscută: [{section}] {key}={value}");
                    break;
            }
        }

        private void AssignDatabaseValue(string key, string value)
        {
            switch (key.ToUpper())
            {
                case "SRVNAME":
                    Database.SrvName = value;
                    break;
                case "SRVADDRESS":
                    Database.SrvAddress = value;
                    break;
                case "SRVPORT":
                    Database.SrvPort = int.TryParse(value, out int port) ? port : 3052;
                    break;
                case "DATAUSER":
                    Database.DataUser = value;
                    break;
                case "DATAKEY":
                    Database.DataKey = value;
                    break;
                case "DATAADDRESS":
                    Database.DataAddress = value;
                    break;
            }
        }

        private void AssignConfigValue(string key, string value)
        {
            switch (key.ToUpper())
            {
                case "CNTSOUND":
                    Config.CntSound = int.TryParse(value, out int sound) ? sound : 0;
                    break;
                case "CNTWINERROR":
                    Config.CntWindError = int.TryParse(value, out int err) ? err : 0;
                    break;
            }
        }

        private void AssignAmefValue(string key, string value)
        {
            switch (key.ToUpper())
            {
                case "MODEL":
                    Amef.Model = value;
                    break;
                case "CALEBON":
                    Amef.CaleBon = value;
                    break;
            }
        }

        private void AssignTipPlataValue(string key, string value)
        {
            int tipValue = int.TryParse(value, out int val) ? val : 0;

            switch (key.ToUpper())
            {
                case "NUMERAR":
                    TipPlata.Numerar = tipValue;
                    break;
                case "CARD":
                    TipPlata.Card = tipValue;
                    break;
                case "TICHET":
                    TipPlata.Tichet = tipValue;
                    break;
                case "MODERN":
                    TipPlata.Modern = tipValue;
                    break;
                case "CEC":
                    TipPlata.Cec = tipValue;
                    break;
            }
        }

        private void AssignWebValue(string key, string value)
        {
            switch (key.ToUpper())
            {
                case "CALESERVER":
                    Web.CaleServer = value;
                    break;
            }
        }

        #endregion

        #region === VALORI DEFAULT ===

        /// <summary>
        /// Creează configurația cu valori default
        /// </summary>
        private void CreateDefaultConfig()
        {
            Database = new DatabaseConfig
            {
                SrvName = "FIREBIRD DRIVER",
                SrvAddress = "127.0.0.1",
                SrvPort = 3052,
                DataUser = "",
                DataKey = "",
                DataAddress = @"D:\SysManager\data\data.fdb"
            };

            Config = new AppConfig
            {
                CntSound = 0,
                CntWindError = 0
            };

            Amef = new AmefConfig
            {
                Model = "DATECS",
                CaleBon = @"C:\Fisco\Bonuri\bf.inp"
            };

            TipPlata = new TipPlataConfig
            {
                Numerar = 0,
                Card = 1,
                Tichet = 3,
                Modern = 6,
                Cec = 5
            };

            Web = new WebConfig
            {
                CaleServer = @"D:\xampp"
            };

            Logs.Write("✅ Configurație DEFAULT creată");
        }

        #endregion

        #region === SALVARE CONFIGURAȚIE ===

        /// <summary>
        /// Salvează configurația curentă în fișierul INI
        /// </summary>
        public bool SaveConfiguration()
        {
            try
            {
                // Verifică dacă directorul există
                string configDirectory = Path.GetDirectoryName(ConfigFilePath);
                if (!Directory.Exists(configDirectory))
                {
                    Directory.CreateDirectory(configDirectory);
                    Logs.Write($"✅ Director config creat: {configDirectory}");
                }

                // Construiește conținutul fișierului
                var sb = new StringBuilder();

                sb.AppendLine("[DATABASE]");
                sb.AppendLine($"SrvName={Database.SrvName}");
                sb.AppendLine($"SrvAddress={Database.SrvAddress}");
                sb.AppendLine($"SrvPort={Database.SrvPort}");
                sb.AppendLine($"DataUser={Database.DataUser}");
                sb.AppendLine($"DataKey={Database.DataKey}");
                sb.AppendLine($"DataAddress={Database.DataAddress}");

                sb.AppendLine("[CONFIG]");
                sb.AppendLine($"CntSound={Config.CntSound}");
                sb.AppendLine($"CntWindError={Config.CntWindError}");

                sb.AppendLine("[AMEF]");
                sb.AppendLine($"MODEL={Amef.Model}");
                sb.AppendLine($"CALEBON={Amef.CaleBon}");

                sb.AppendLine("[TIP_PLATA]");
                sb.AppendLine($"NUMERAR={TipPlata.Numerar}");
                sb.AppendLine($"CARD={TipPlata.Card}");
                sb.AppendLine($"TICHET={TipPlata.Tichet}");
                sb.AppendLine($"MODERN={TipPlata.Modern}");
                sb.AppendLine($"CEC={TipPlata.Cec}");

                sb.AppendLine("[WEB]");
                sb.AppendLine($"CaleServer={Web.CaleServer}");

                // Salvează în fișier
                File.WriteAllText(ConfigFilePath, sb.ToString(), Encoding.UTF8);

                Logs.Write($"✅ Configurație salvată în {ConfigFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Logs.Write("❌ EROARE la salvarea configurației:");
                Logs.Write(ex);
                return false;
            }
        }

        #endregion

        #region === LOGGING ===

        /// <summary>
        /// Afișează configurația curentă în log
        /// </summary>
        private void LogConfiguration()
        {
            Logs.Write("═══════════════════════════════════════");
            Logs.Write("CONFIGURAȚIE ÎNCĂRCATĂ:");
            Logs.Write("═══════════════════════════════════════");

            Logs.Write($"[DATABASE]");
            Logs.Write($"  SrvName     = {Database.SrvName}");
            Logs.Write($"  SrvAddress  = {Database.SrvAddress}");
            Logs.Write($"  SrvPort     = {Database.SrvPort}");
            Logs.Write($"  DataUser    = {(string.IsNullOrEmpty(Database.DataUser) ? "(gol)" : "***")}");
            Logs.Write($"  DataKey     = {(string.IsNullOrEmpty(Database.DataKey) ? "(gol)" : "***")}");
            Logs.Write($"  DataAddress = {Database.DataAddress}");

            Logs.Write($"[CONFIG]");
            Logs.Write($"  CntSound     = {Config.CntSound}");
            Logs.Write($"  CntWindError = {Config.CntWindError}");

            Logs.Write($"[AMEF]");
            Logs.Write($"  Model   = {Amef.Model}");
            Logs.Write($"  CaleBon = {Amef.CaleBon}");

            Logs.Write($"[TIP_PLATA]");
            Logs.Write($"  Numerar = {TipPlata.Numerar}");
            Logs.Write($"  Card    = {TipPlata.Card}");
            Logs.Write($"  Tichet  = {TipPlata.Tichet}");
            Logs.Write($"  Modern  = {TipPlata.Modern}");
            Logs.Write($"  Cec     = {TipPlata.Cec}");

            Logs.Write($"[WEB]");
            Logs.Write($"  CaleServer = {Web.CaleServer}");

            Logs.Write("═══════════════════════════════════════");
        }

        #endregion

        #region === RELOAD CONFIGURAȚIE ===

        /// <summary>
        /// Reîncarcă configurația din fișier (util pentru refresh)
        /// </summary>
        public void ReloadConfiguration()
        {
            Logs.Write("ConfigManager: Reîncărcare configurație...");
            LoadConfiguration();
        }

        #endregion
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CLASE DE CONFIGURARE (MODELE)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Configurări bază de date
    /// </summary>
    public class DatabaseConfig
    {
        public string SrvName { get; set; }
        public string SrvAddress { get; set; }
        public int SrvPort { get; set; }
        public string DataUser { get; set; }
        public string DataKey { get; set; }
        public string DataAddress { get; set; }
    }

    /// <summary>
    /// Configurări generale aplicație
    /// </summary>
    public class AppConfig
    {
        public int CntSound { get; set; }
        public int CntWindError { get; set; }

        public bool IsSoundEnabled => CntSound == 1;
        public bool IsWindowErrorEnabled => CntWindError == 1;
    }

    /// <summary>
    /// Configurări AMEF (Aparat Marcat Electronice Fiscale)
    /// </summary>
    public class AmefConfig
    {
        public string Model { get; set; }
        public string CaleBon { get; set; }
    }

    /// <summary>
    /// Coduri tipuri de plată pentru casă de marcat
    /// </summary>
    public class TipPlataConfig
    {
        public int Numerar { get; set; }
        public int Card { get; set; }
        public int Tichet { get; set; }
        public int Modern { get; set; }
        public int Cec { get; set; }
    }

    /// <summary>
    /// Configurări server web
    /// </summary>
    public class WebConfig
    {
        public string CaleServer { get; set; }
    }
}