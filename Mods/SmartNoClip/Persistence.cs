using Newtonsoft.Json;
using SaveSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KitchenSmartNoClip
{
    public class ConfigEntry
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public string StringValue { get; private set; }
        public ConfigEntry(string _name, string _stringValue)
        {
            Name = _name;
            if (_stringValue != null)
                StringValue = _stringValue;
            else
                StringValue = String.Empty;
        }

        public object GetValue(SettingType _type)
        {
            if (!String.IsNullOrWhiteSpace(StringValue))
            {
                switch (_type)
                {
                    case SettingType.stringValue:
                        return StringValue;
                    case SettingType.intValue:
                        {
                            if (int.TryParse(StringValue, out int value))
                            {
                                return value;
                            }
                            throw new ConfigEntryParseException("Not an int value");
                        }
                    case SettingType.boolValue:
                        {
                            if (bool.TryParse(StringValue, out bool value))
                            {
                                return value;
                            }
                            throw new ConfigEntryParseException("Not a bool value");
                        }
                    default:
                        throw new ConfigEntryParseException("No type");
                }
            }
            // Default values
            switch (_type)
            {
                case SettingType.stringValue:
                    return StringValue;
                case SettingType.intValue:
                    return 0;
                case SettingType.boolValue:
                    return false;
                default:
                    throw new ConfigEntryParseException("No type");
            }
            // NOTTODO: This method looks like a sorted mess
        }

        public void SetValue(object _value)
        {
            StringValue = _value.ToString();
        }

        public class ConfigEntryParseException : Exception
        {
            public ConfigEntryParseException(string message) : base(message)
            {
            }
        }

        public enum SettingType
        {
            stringValue = 0,
            intValue = 1,
            boolValue = 2,
        }
    }

    public class Persistence
    {
        #region MainPart
        public static Persistence Instance { get; private set; }
        List<ConfigEntry> persistentSettings = new List<ConfigEntry>();
        private string settingsPath;

        //TODO: Setup Persistence
        public Persistence(string _settingsPath)
        {
            settingsPath = Application.persistentDataPath + "/" + nameof(SmartNoClip.MOD_GUID);

            LoadCurrentSettings();
        }


        public ConfigEntry this[string i]
        {
            get
            {
                foreach (ConfigEntry setting in persistentSettings)
                {
                    if (setting.Name == i)
                    {
                        return setting;
                    }
                }
                ConfigEntry newS = new ConfigEntry(i, null);
                persistentSettings.Add(newS);
                return newS;
            }
        }

        public void SaveCurrentConfig()
        {
            string currentJson = JsonConvert.SerializeObject(persistentSettings, Formatting.Indented);
            File.WriteAllText(settingsPath + "/Settings.json", currentJson);
        }

        /// <summary>
        /// Loads the settings from a JSON
        /// </summary>
        private void LoadCurrentSettings()
        {
            try
            {
                try
                {
                    string text = System.IO.File.ReadAllText(settingsPath + "/Settings.json");
                    persistentSettings = JsonConvert.DeserializeObject<List<ConfigEntry>>(text);
                }
                catch (FileNotFoundException _fileEx)
                {
                    SmartNoClip.LogWarning("No setting file to load, probably started for the first time.");
                    persistentSettings = new List<ConfigEntry>();
                }
            }
            catch (Exception _e)
            {
                SmartNoClip.LogWarning(_e.Message);
            }
        }
        #endregion

        #region Configs



        #endregion
    }
}
