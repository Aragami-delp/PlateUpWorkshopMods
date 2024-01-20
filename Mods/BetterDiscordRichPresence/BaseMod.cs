using Discord;
using HarmonyLib;
using Kitchen;
using Kitchen.NetworkSupport;
using KitchenMods;
using KitchenPrepUiDuringDay;
using System.Reflection;
using UnityEngine;
using KitchenBetterDiscordRichPresence.Utility;
using Unity.Entities;
using System;
using Steamworks;
using TwitchLib.Api.Core.Models.Undocumented.ChannelPanels;

// Namespace should have "Kitchen" in the beginning
namespace KitchenBetterDiscordRichPresence
{
    [HarmonyPatch(typeof(SteamRichPresenceView), "UpdateDiscordRichPresence")]
    public class Patch_Pre_SRPV
    {
        public static bool Prefix(SteamRichPresenceView.ViewData view_data, DiscordPlatform __instance)
        {
            BaseMod.LogInfo("WhyNotyHere?2");
            if (DiscordPlatform.Discord.State != PlatformState.Ready)
                return false;
            string details = "";
            if (view_data.Data.IsInGame)
            {
                if (view_data.Data.Day > 15)
                {
                    details = $"Overtime (Day {view_data.Data.Day - 15})";
                }
                else
                {
                    details = $"Restauranting (Day {view_data.Data.Day})";
                }
            }
            else
            {
                details = "Planning";
            }
            string state = "";
            if (view_data.Data.IsMultiplayer)
            {
                string str;
                switch (view_data.Data.Players)
                {
                    case 1:
                        str = "\uD83D\uDC69\u200D\uD83C\uDF73⚫⚫⚫";
                        break;
                    case 2:
                        str = "\uD83D\uDC69\u200D\uD83C\uDF73\uD83D\uDC69\u200D\uD83C\uDF73⚫⚫";
                        break;
                    case 3:
                        str = "\uD83D\uDC69\u200D\uD83C\uDF73\uD83D\uDC69\u200D\uD83C\uDF73\uD83D\uDC69\u200D\uD83C\uDF73⚫";
                        break;
                    case 4:
                        str = "\uD83D\uDC69\u200D\uD83C\uDF73\uD83D\uDC69\u200D\uD83C\uDF73\uD83D\uDC69\u200D\uD83C\uDF73\uD83D\uDC69\u200D\uD83C\uDF73";
                        break;
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                    case 11:
                    case 12:
                        str = String.Format($"{view_data.Data.Players}/\uD83D\uDC69\u200D\uD83C\uDF73");
                        break;
                    default:
                        str = "";
                        break;
                }
                state = str;
            }
            DiscordPlatform.Discord.SetActivity(state, details, view_data.Data.Players);
            return false;
        }
    }

    [HarmonyPatch(typeof(DiscordPlatform), nameof(DiscordPlatform.SetActivity))]
    public class Patch_Pre_DPSA
    {
        public static bool Prefix(string state, string details, int players, DiscordPlatform __instance)
        {
            BaseMod.LogInfo("WhyNotyHere?1");
            if (!__instance.IsReady)
            {
                return false;
            }

            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Assembly mmoKitchen = Utility.Utility.GetLoadedAssembly("MMOKitchen.dll");

            if (players == 0)
            {
                players = 1;
            }

            #region Time
            EntityQuery entityQuery = entityManager.CreateEntityQuery(typeof(SSpeedrunDuration));
            Entity my = entityQuery.GetSingletonEntity();
            SSpeedrunDuration myCom = entityManager.GetComponentData<SSpeedrunDuration>(my);
            long startTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (long)myCom.Seconds;
            #endregion

            Activity activity = new Activity
            {
                State = state,
                Details = details,
                Assets = new ActivityAssets
                {
                    LargeImage = "logosquare",
                    LargeText = "PlateUp!",
                    SmallImage = "logosquare",
                    SmallText = "PlateUp!"
                },
                Type = ActivityType.Playing,
                Timestamps = new ActivityTimestamps
                {
                    Start = startTimeStamp
                }
            };
            if (__instance.Permissions != NetworkPermissions.Private)
            {
                int maxSize = 4;
                if (mmoKitchen != null)
                {
                    maxSize = 12;
                }
                try
                {
                    activity.Secrets = new ActivitySecrets
                    {
                        Join = __instance.DiscordSDK.GetLobbyManager().GetLobbyActivitySecret(__instance.CurrentInviteLobby.Id)
                    };
                    activity.Party = new ActivityParty
                    {
                        Id = string.Format("{0}", __instance.CurrentInviteLobby.Id),
                        Size = new PartySize
                        {
                            MaxSize = maxSize,
                            CurrentSize = players
                        }
                    };
                }
                catch (ResultException)
                {
                }
            }
            __instance.DiscordSDK.GetActivityManager().UpdateActivity(activity, (ActivityManager.UpdateActivityHandler)(res => { }));
            return false; // Don't run original
        }
    }

    public class BaseMod : GenericSystemBase, IModSystem
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "aragami.plateup.mods.betterdiscordRichPresence";
        public const string MOD_NAME = "Better Discord Rich Presence";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Aragami";
        public const string MOD_GAMEVERSION = ">=1.1.7";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif

        private static readonly Harmony m_harmony = new Harmony("aragami.plateup.mods.betterdiscordRichPresence");

        protected override void Initialise()
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
            m_harmony.PatchAll(Assembly.GetExecutingAssembly());
            LogWarning($"Patched");
        }

        protected override void OnUpdate()
        {
        }

        public void PostActivate(Mod _mod)
        {
            // Patch after initializing mods
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!1");
        }
        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
