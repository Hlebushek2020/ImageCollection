using ImageCollection.Classes.Collections;
using ImageCollection.Structures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ImageCollection.Classes.Settings
{
    public class StoreSettings
    {
        [JsonIgnore]
        public string BaseDirectory { get; private set; }
        [JsonIgnore]
        public bool IsChanged { get; private set; }

        public string DistributionDirectory { get; set; }
        /// <summary>
        /// Горячие клавиши (НАПРЯМУЮ НЕРАБОТАЕМ)
        /// </summary>
        public Dictionary<Hotkey, string> CollectionHotkeys { get; set; } = new Dictionary<Hotkey, string>();

        public StoreSettings(string baseDirectory) =>
            BaseDirectory = baseDirectory;

        /// <summary>
        /// Устанавливает директорию для распределения в качестве базовой директории
        /// </summary>
        public void SetDistributionDirectoryAsBase()
        {
            BaseDirectory = DistributionDirectory;
            DistributionDirectory = null;
            IsChanged = true;
        }

        #region Hotkeys
        public void AddHotkey(CollectionKeyInformation keyInformation)
        {
            CollectionHotkeys.Add(keyInformation.Hotkey, keyInformation.CollectionName);
            IsChanged = true;
        }

        public void RemoveHotkey(Hotkey key)
        {
            CollectionHotkeys.Remove(key);
            IsChanged = true;
        }

        public void RemoveHotkey(string collection)
        {
            List<Hotkey> deleteHotkeys = CollectionHotkeys.Where(x => x.Value.Equals(collection)).Select(x => x.Key).ToList();
            foreach (Hotkey key in deleteHotkeys)
            {
                CollectionHotkeys.Remove(key);
            }
            IsChanged = true;
        }

        public void RemoveAllHotkeys()
        {
            CollectionHotkeys.Clear();
            IsChanged = true;
        }

        public void SetHotkeyCollection(CollectionKeyInformation keyInformation)
        {
            CollectionHotkeys[keyInformation.Hotkey] = keyInformation.CollectionName;
            IsChanged = true;
        }

        public void SetHotkeyCollection(string collectionOld, string collectionNew)
        {
            List<Hotkey> deleteHotkeys = CollectionHotkeys.Where(x => x.Value.Equals(collectionOld)).Select(x => x.Key).ToList();
            foreach (Hotkey key in deleteHotkeys)
            {
                CollectionHotkeys[key] = collectionNew;
            }
            IsChanged = true;
        }
        #endregion

        /// <summary>
        /// Сохраняет текущие настройки хранилища
        /// </summary>
        public void Save()
        {
            string settings = Path.Combine(BaseDirectory, CollectionStore.DataDirectoryName, "settings.json");
            using (StreamWriter streamWriter = new StreamWriter(settings, false, Encoding.UTF8))
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            IsChanged = false;
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
                result = JsonConvert.DeserializeObject<StoreSettings>(json);
                result.BaseDirectory = baseDirectory;
            }
            return result;
        }
    }
}