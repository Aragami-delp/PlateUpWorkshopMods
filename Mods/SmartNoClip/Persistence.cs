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
    /// <summary>
    /// Saves a single config using object.ToString, and is retrieved the other way around
    /// </summary>
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
                return false;
                //throw new ConfigEntryParseException("Not a bool value");
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
                return 1;
                //throw new ConfigEntryParseException("Not an int value");
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
                return 1f;
                //throw new ConfigEntryParseException("Not a float value");
            }
        }

        public void SetValue(object _value)
        {
            StringValue = _value.ToString();
        }

        //public class ConfigEntryParseException : Exception
        //{
        //    public ConfigEntryParseException(string message) : base(message)
        //    {
        //    }
        //}
    }

    /// <summary>
    /// Handles saving settings
    /// </summary>
    public class Persistence
    {
        #region MainPart
        public static Persistence Instance { get; private set; }
        List<ConfigEntry> persistentSettings = new List<ConfigEntry>();
        /// <summary>
        /// Settings file is placed next to the directorty of plateup saves
        /// </summary>
        private string settingsFilePath;

        /// <summary>
        /// Creates a new persistence handling
        /// </summary>
        public Persistence() 
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;

            SmartNoClip.LogInfo("Init Persistence");
            settingsFilePath = Application.persistentDataPath + "/" + SmartNoClip.MOD_NAME + ".json";

            LoadCurrentSettings();
        }


        /// <summary>
        /// Get an entry for a setting
        /// </summary>
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

        /// <summary>
        /// Saves the current config to hard disk as JSON
        /// </summary>
        public void SaveCurrentConfig()
        {
            string currentJson = JsonConvert.SerializeObject(persistentSettings, Formatting.Indented);
            File.WriteAllText(settingsFilePath, currentJson);
        }

        /// <summary>
        /// Loads the settings from hard disk as JSON
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

        /// <summary>
        /// Creates a new config file on hard disk an provides default values to all current settings
        /// </summary>
        private void CreateDefaultConfig()
        {
            //this["bGeneral_Mod_Active"].SetValue(true);
            this["bActive_Prep"].SetValue(true);
            this["bActive_Day"].SetValue(false);
            this["bActive_HQ"].SetValue(false);
            this["fSpeed_Value"].SetValue(1.5f);
            this["bAllow_Players_Outside"].SetValue(true);
            this["bResetOverrideOnChange"].SetValue(true);

            SaveCurrentConfig();
        }
        #endregion
    }
}
