using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ImageCollection.Classes.Settings
{
    public class ProgramSettings
    {
        [JsonIgnore]
        private static ProgramSettings settings;

        [JsonIgnore]
        private static readonly string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
            "SergeyGovorunov", App.Name.Replace(" ", ""), "settings.json");

        #region Settings
        public string Theme { get; set; } = "Light";
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
            Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            using (StreamWriter streamWriter = new StreamWriter(settingsPath, false, Encoding.UTF8))
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
