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

namespace Workshopupdater
{
    public static class Helper
    {
        private static readonly Regex MODDEPENDENCIESLINE = new Regex("<a href=\"https:\\/\\/steamcommunity.com\\/workshop\\/filedetails\\/\\?id=\\d+\" target=\"_blank\">", RegexOptions.IgnoreCase);

        public static async Task<List<Item>> GetSubscribedModItems()
        {
            List<Item> items = new List<Item>();
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

        public static void SaveCurrentModList(List<Item> _items = null)
        {
            if (_items == null)
            {
                _items = Task.Run(() => GetSubscribedModItems()).GetAwaiter().GetResult();
            }

            HashSet<long> itemIDs = new HashSet<long>();
            foreach (Item item in _items)
            {
                itemIDs.Add((long)item.Id.Value);
            }
            SaveCurrentModList(itemIDs);
        }

        private static void SaveCurrentModList(HashSet<long> _items)
        {
            string currentJson = JsonConvert.SerializeObject(_items, Formatting.Indented);
            File.WriteAllText(Application.persistentDataPath + "/DependencyChecker/LastMods.json", currentJson);
        }

        public static HashSet<long> LoadLastModList()
        {
            HashSet<long> itemIDs = new HashSet<long>();
            try
            {
                try
                {
                    string text = System.IO.File.ReadAllText(Application.persistentDataPath + "/DependencyChecker/LastMods.json");
                    itemIDs = JsonConvert.DeserializeObject<HashSet<long>>(text);
                }
                catch (FileNotFoundException _fileEx)
                {
                    WorkshopupdaterMain.LogWarning("No LastMods.json file to load, probably started for the first time.");
                }
            }
            catch (Exception _e)
            {
                WorkshopupdaterMain.LogError(_e.Message);
            }
            return itemIDs;
        }

        public static HashSet<long> GetAllModDependencies(HashSet<Item> _items)
        {
            HashSet<long> depItems = new HashSet<long>();
            foreach (Item item in _items)
            {
                depItems.UnionWith(Task.Run(() => GetSingleModDependencies(item)).GetAwaiter().GetResult());
            }
            return depItems;
        }

        public static async Task<HashSet<long>> GetSingleModDependencies(Item _item)
        {
            // Originally made by ZekNikZ for KitchenLib to check for Change Notes
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(_item.Url);
            HttpContent content = response.Content;
            string pageContent = await content.ReadAsStringAsync();
            List<string> extractedContent = MODDEPENDENCIESLINE.Matches(pageContent).Cast<Match>().Select(match => match.Value).ToList();
            HashSet<long> retVal = new HashSet<long>();
            foreach (string match in extractedContent)
            {
                string resultString = Regex.Match(match, @"\d+").Value;
                if (long.TryParse(resultString, out long _ret))
                {
                    retVal.Add(_ret);
                }
                else
                {
                    WorkshopupdaterMain.LogWarning("Cannot Parse \"" + resultString + "\"");
                }
            }
            return retVal;
        }

        //public static List<Item> GetItems(HashSet<long> _itemIDs)
        //{
        //    List<Item> installedMods = Task.Run(() => GetSubscribedModItems()).GetAwaiter().GetResult();
        //    foreach (Item item in installedMods)
        //    {
        //        if (_itemIDs.Contains((long)item.Id.Value))
        //        {
        //            _itemIDs.Remove((long)item.Id.Value);
        //        }
        //    }
        //}
    }
}
