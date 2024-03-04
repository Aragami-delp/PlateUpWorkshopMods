using Newtonsoft.Json;
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

        [JsonIgnore]
        public bool BoolValue
        {
            get
            {
                if (bool.TryParse(StringValue, out bool value))
                {
                    return value;
                }
                throw new ConfigEntryParseException("Not a bool value");
            }
        }

        [JsonIgnore]
        public int IntValue
        {
            get
            {
                if (int.TryParse(StringValue, out int value))
                {
                    return value;
                }
                throw new ConfigEntryParseException("Not an int value");
            }
        }

        [JsonIgnore]
        public float FloatValue
        {
            get
            {
                if (float.TryParse(StringValue, out float value))
                {
                    return value;
                }
                throw new ConfigEntryParseException("Not a float value");
            }
        }

        #region OldCode
        //public object GetValue(SettingType _type)
        //{
        //    if (!String.IsNullOrWhiteSpace(StringValue))
        //    {
        //        switch (_type)
        //        {
        //            case SettingType.STRING:
        //                return StringValue;
        //            case SettingType.INT:
        //                {
        //                    if (int.TryParse(StringValue, out int value))
        //                    {
        //                        return value;
        //                    }
        //                    throw new ConfigEntryParseException("Not an int value");
        //                }
        //            case SettingType.BOOL:
        //                {
        //                    if (bool.TryParse(StringValue, out bool value))
        //                    {
        //                        return value;
        //                    }
        //                    throw new ConfigEntryParseException("Not a bool value");
        //                }
        //            case SettingType.FLOAT:
        //                {
        //                    if (float.TryParse(StringValue, out float value))
        //                    {
        //                        return value;
        //                    }
        //                    throw new ConfigEntryParseException("Not a float value");
        //                }
        //            default:
        //                throw new ConfigEntryParseException("No type");
        //        }
        //    }
        //// Default values
        //switch (_type)
        //{
        //    case SettingType.STRING:
        //        return StringValue;
        //    case SettingType.INT:
        //        return default(int);
        //    case SettingType.BOOL:
        //        return default(bool);
        //    case SettingType.FLOAT:
        //        return default(float);
        //    default:
        //        throw new ConfigEntryParseException("No type");
        //}
        //// NOTTODO: This method looks like a sorted mess
        ///
        //}
        #endregion

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
    }

    public class Persistence
    {
        #region MainPart
        public static Persistence Instance { get; private set; }
        List<ConfigEntry> persistentSettings = new List<ConfigEntry>();
        private string settingsFilePath;

        //TODO: Setup Persistence
        public Persistence() 
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;

            SmartNoClip.LogError("Init Persistence");
            settingsFilePath = Application.persistentDataPath + "/" + SmartNoClip.MOD_NAME + ".json";

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
            File.WriteAllText(settingsFilePath, currentJson);
        }

        /// <summary>
        /// Loads the settings from a JSON
        /// </summary>
        private void LoadCurrentSettings()
        {

            try
            {
                string text = System.IO.File.ReadAllText(settingsFilePath);
                persistentSettings = JsonConvert.DeserializeObject<List<ConfigEntry>>(text);
            }
            catch (Exception _e)
            {
                SmartNoClip.LogWarning(_e.Message);

                SmartNoClip.LogWarning("No setting file to load, probably started for the first time.");

                persistentSettings = new List<ConfigEntry>();
                CreateDefaultConfig();
            }

            //// Since there were some save problems i didn't figure out yet
            //persistentSettings = new List<ConfigEntry>();
            //CreateDefaultConfig();
        }

        private void CreateDefaultConfig()
        {
            //this["bGeneral_Mod_Active"].SetValue(true);
            this["bActive_Prep"].SetValue(true);
            this["bActive_Day"].SetValue(false);
            this["bActive_HQ"].SetValue(false);
            this["fSpeed_Value"].SetValue(1.5f);
            this["bAllow_Players_Outside"].SetValue(true);

            SaveCurrentConfig();
        }
        #endregion
    }
}
