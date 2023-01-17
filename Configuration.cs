using System.Diagnostics;
using System.Text.Json;

namespace GithubSponsorsWebhook
{

    public static class Configuration
    {
        #region Types.
        public class ConfigurationData
        {
            /// <summary>
            /// True if the data has been setup. This must be set manually within the configuration file to eliminate not setup warnings.
            /// </summary>
            public bool Setup { get; set; }
            /// <summary>
            /// Minimum amount required to unlock downloads.
            /// </summary>
            public long MinimumSpendInCent { get; set; }
            /// <summary>
            /// Amount spent in total.
            /// </summary>
            public long LifetimeSpendInCent { get; set; }
            /// <summary>
            /// Amount to contribute to Lifetime when mitigating from another platform.
            /// </summary>
            public long MigrationIncentiveInCent { get; set; }
            /// <summary>
            /// Token for GitHub.
            /// </summary>
            public string? GithubToken { get; set; }
            /// <summary>
            /// Shhh, I cannot tell you what this one does, it's a SECRET.
            /// </summary>
            public string? GithubSecret { get; set; }
        }
        #endregion

        #region Private.
        /// <summary>
        /// Configuration data to use.
        /// </summary>
        private static ConfigurationData? _data;
        #endregion

        public static ConfigurationData GetConfiguration()
        {
            if (_data != null)
                return _data;

            string configName = "Config.cfg";
            //Path.combine would be ideal but it does not work sometimes for unknown reasons.
            string path = $"{AppDomain.CurrentDomain.BaseDirectory}{configName}";
            //Create if does not exist.
            try
            {
                if (!File.Exists(path))
                {
                    //Serialize as a json into path.
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    string json = JsonSerializer.Serialize(new ConfigurationData(), options);
                    File.WriteAllText(path, json);                    
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Configuration file could not be created at {path}. Ex {ex.Message}");
                return null;
            }

            //Load the file.
            try
            {
                string allText = File.ReadAllText(path);
                _data = JsonSerializer.Deserialize<ConfigurationData>(allText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Configuration file could not be loaaded as a JSON. Ex {ex.Message}");
                return null;
            }

            //If not initialized.
            if (!_data!.Setup)
            {
                Debug.WriteLine($"Configuration file is not setup.");
                return null;
            }

            return _data;
        }

    }


}
