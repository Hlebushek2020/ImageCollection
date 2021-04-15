using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ImageCollection.Classes
{
    public class ProgramSettings
    {
        [JsonIgnore]
        private static ProgramSettings settings;

        [JsonIgnore]
        private static readonly string settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
            App.Name.Replace(" ", ""), "settings.json");

        #region Settings
        public string Theme { get; set; } = "None";
        #endregion

        public static ProgramSettings GetInstance()
        {
            if (settings == null)
            {
                if (File.Exists(settingsPath))
                    settings = JsonConvert.DeserializeObject<ProgramSettings>(File.ReadAllText(settingsPath, Encoding.UTF8));
                else
                    settings = new ProgramSettings();
            }
            return settings;
        }

        public void Save()
        {
            using (StreamWriter streamWriter = new StreamWriter(settingsPath, false, Encoding.UTF8))
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
