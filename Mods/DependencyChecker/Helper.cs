using Newtonsoft.Json;
using Steamworks.Ugc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;
using System.Net.Http;
using System.Web;
using System.Text.RegularExpressions;
using Steamworks.Data;
using Microsoft.Win32;

namespace KitchenDependencyChecker
{
    public static class Helper
    {
        private static readonly Regex MODDEPENDENCIESLINE = new("<a href=\"https:\\/\\/steamcommunity.com\\/workshop\\/filedetails\\/\\?id=\\d+\" target=\"_blank\">", RegexOptions.IgnoreCase);
        private static readonly string MODCACHEFOLDER = Application.persistentDataPath + "/DependencyChecker";
        private static readonly string MODCACHEFILE = "LastMods.json";

        public static string MODCACHEFOLDERANDFILE => MODCACHEFOLDER + "/" + MODCACHEFILE;
        private static List<Item> m_installedItems = null;


        public static List<Item> GetInstalledModItems()
        {
            if (m_installedItems != null)
            {
                return m_installedItems;
            }

            string steamPath = String.Empty;
            try
            {
                using RegistryKey key = Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Valve\\Steam");
                if (key != null)
                {
                    object o = key?.GetValue("InstallPath") as string;
                    if (o != null)
                    {
                        steamPath = o.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                DependencyCheckerMain.LogError("Error reading steam install path");
            }

            if (string.IsNullOrEmpty(steamPath))
                return Task.Run(() => GetSubscribedModItems()).GetAwaiter().GetResult();


            string[] libraryfoldersVDF = File.ReadAllLines(steamPath + "\\steamapps\\libraryfolders.vdf");
            string currentPath = string.Empty;
            foreach (string line in libraryfoldersVDF)
            {
                if (line.Contains("\"path\""))
                {
                    string[] parts = line.Split(new char[] {'"'}, StringSplitOptions.RemoveEmptyEntries);
                    currentPath = parts[3].Replace("\\\\", "\\");
                }
                if (line.Contains("\"1599600\""))
                {
                    break;
                }
            }
            if (string.IsNullOrEmpty(currentPath))
            {
                DependencyCheckerMain.LogError("No valid library path found");
            }
            currentPath = currentPath + "\\steamapps\\workshop\\content\\1599600";
            string[] foundItemStringIds = Directory.GetDirectories(currentPath);
            HashSet<PublishedFileId> foundItemIds = new();
            foreach (string foundItemStringId in foundItemStringIds)
            {
                if (ulong.TryParse(Path.GetFileName(foundItemStringId), out ulong _result))
                {
                    foundItemIds.Add(_result);
                }
                else
                {
                    DependencyCheckerMain.LogError("Can't read workshop item id");
                }
            }
            m_installedItems = Task.Run(() => GetModItems(foundItemIds)).GetAwaiter().GetResult();
            return m_installedItems;
        }

        public static async Task<List<Item>> GetSubscribedModItems()
        {
            List<Item> items = new();
            int page_number = 1;
            int result_count = 0;
            ResultPage value;
            do
            {
                ResultPage? page = await Query.Items.WhereUserSubscribed().GetPageAsync(page_number);
                if (!page.HasValue)
                {
                    break;
                }

                value = page.Value;
                items.AddRange(value.Entries);
                result_count += value.ResultCount;
                page_number++;
            }
            while (value.ResultCount != 0 && result_count < value.TotalCount);

            return items;
        }

        public static async Task<List<Item>> GetModItems(HashSet<PublishedFileId> _ids)
        {
            List<Item> items = new();
            int page_number = 1;
            int result_count = 0;
            ResultPage value;
            do
            {
                ResultPage? page = await Query.Items.WithFileId(_ids.ToArray()).GetPageAsync(page_number);
                if (!page.HasValue)
                {
                    break;
                }

                value = page.Value;
                items.AddRange(value.Entries);
                result_count += value.ResultCount;
                page_number++;
            }
            while (value.ResultCount != 0 && result_count < value.TotalCount);

            return items;
        }

        public static void SaveCurrentModList(List<Item> _items = null)
        {
            _items ??= Task.Run(() => GetSubscribedModItems()).GetAwaiter().GetResult();

            HashSet<long> itemIDs = new();
            foreach (Item item in _items)
            {
                itemIDs.Add((long)item.Id.Value);
            }
            SaveCurrentModList(itemIDs);
        }

        private static void SaveCurrentModList(HashSet<long> _items)
        {
            if (!Directory.Exists(MODCACHEFOLDER))
                Directory.CreateDirectory(MODCACHEFOLDER);
            string currentJson = JsonConvert.SerializeObject(_items, Formatting.Indented);
            File.WriteAllText(MODCACHEFOLDERANDFILE, currentJson);
        }

        public static HashSet<PublishedFileId> LoadLastModList()
        {
            if (!Directory.Exists(MODCACHEFOLDER))
                Directory.CreateDirectory(MODCACHEFOLDER);

            HashSet<long> itemIDs = new();
            try
            {
                try
                {
                    string text = System.IO.File.ReadAllText(MODCACHEFOLDERANDFILE);
                    itemIDs = JsonConvert.DeserializeObject<HashSet<long>>(text);
                }
                catch (FileNotFoundException _fileEx)
                {
                    DependencyCheckerMain.LogWarning("No LastMods.json file to load, probably started for the first time.");
                }
            }
            catch (Exception _e)
            {
                DependencyCheckerMain.LogError(_e.Message);
            }
            HashSet<PublishedFileId> convertedIDs = new();
            foreach (long itemId in itemIDs)
            {
                PublishedFileId tmp = (ulong)itemId;
                convertedIDs.Add(tmp);
            }
            return convertedIDs;
        }

        public static HashSet<PublishedFileId> GetAllModDependencies(List<Item> _items)
        {
            HashSet<PublishedFileId> depItems = new();
            foreach (Item item in _items)
            {
                depItems.UnionWith(Task.Run(() => GetSingleModDependencies(item)).GetAwaiter().GetResult());
            }
            return depItems;
        }

        private static async Task<HashSet<PublishedFileId>> GetSingleModDependencies(Item _item)
        {
            // Originally made by ZekNikZ for KitchenLib to check for Change Notes
            HttpClient client = new();
            using HttpResponseMessage response = await client.GetAsync(_item.Url);
            using HttpContent content = response.Content;
            string pageContent = await content.ReadAsStringAsync();
            List<string> extractedContent = MODDEPENDENCIESLINE.Matches(pageContent).Cast<Match>().Select(match => match.Value).ToList();
            HashSet<PublishedFileId> retVal = new();
            foreach (string match in extractedContent)
            {
                string resultString = Regex.Match(match, @"\d+").Value;
                if (ulong.TryParse(resultString, out ulong _ret))
                {
                    PublishedFileId tmp = _ret;
                    retVal.Add(tmp);
                }
                else
                {
                    DependencyCheckerMain.LogWarning("Cannot Parse \"" + resultString + "\"");
                }
            }
            return retVal;
        }

        //public static List<Item> GetItems(HashSet<PublishedFileId> _itemIDs)
        //{
        //    List<Item> installedMods = Task.Run(() => GetSubscribedModItems()).GetAwaiter().GetResult();

        //    foreach (Item item in installedMods)
        //    {
        //        if (_itemIDs.Contains(item.Id))
        //        {
        //            _itemIDs.Remove(item.Id);
        //        }
        //    }

        //    return Task.Run(() => GetModItems(_itemIDs)).GetAwaiter().GetResult();
        //}

        public static async Task<int> InstallItems(List<Item> _items)
        {
            foreach (Item itemsToInstall in _items)
            {
                await itemsToInstall.Subscribe();
                itemsToInstall.Download(true);
            }
            return 1;
        }
    }
}
