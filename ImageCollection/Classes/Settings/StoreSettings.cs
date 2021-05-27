using ImageCollection.Classes.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageCollection.Classes.Settings
{
    public class StoreSettings
    {
        [JsonIgnore]
        public string BaseDirectory { get; private set; }
        public string DistributionDirectory { get; set; }

        public StoreSettings(string baseDirectory) =>
            BaseDirectory = baseDirectory;

        /// <summary>
        /// Устанавливает директорию для распределения в качестве базовой директории
        /// </summary>
        public void SetDistributionDirectoryAsBase()
        {
            BaseDirectory = DistributionDirectory;
            DistributionDirectory = null;
        }

        /// <summary>
        /// Сохраняет текущие настройки хранилища
        /// </summary>
        public void Save()
        {
            string settings = Path.Combine(BaseDirectory, CollectionStore.DataDirectoryName, "settings.json");
            using (StreamWriter streamWriter = new StreamWriter(settings, false, Encoding.UTF8))
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        /// <summary>
        /// Загружает настройки хранилища для коллекций если таковые есть
        /// </summary>
        /// <param name="baseDirectory">Базовая директория</param>
        /// <returns>Загруженные настройки хранилища</returns>
        public static StoreSettings Load(string baseDirectory)
        {
            StoreSettings result = new StoreSettings(baseDirectory);
            string settings = Path.Combine(baseDirectory, CollectionStore.DataDirectoryName, "settings.json");
            if (File.Exists(settings))
            {
                string json = File.ReadAllText(settings, Encoding.UTF8);
                StoreSettings fromJson = JsonConvert.DeserializeObject<StoreSettings>(json);
                result.DistributionDirectory = fromJson.DistributionDirectory;
            }
            return result;
        }
    }
}