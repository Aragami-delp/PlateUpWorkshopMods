using Kitchen;
using KitchenMods;
using UnityEngine;
using HarmonyLib;
using TMPro;
using System.Linq;
using System;
using Shapes;
using System.Collections.Generic;
using UnityEngine.InputSystem;

// Namespace should have "Kitchen" in the beginning
namespace KitchenSmartNoClip
{
    public class SmartNoClipMono : MonoBehaviour
    {
        private const string LARGEWALL = "Wall Section(Clone)";
        private const string SHORTWALL = "Short Wall Section(Clone)";
        private const string HATCH = "Hatch Wall Section(Clone)";
        private const string DOOR = "Door Section(Clone)";
        private const string LARGEDOOR = "External Door Section(Clone)";
        private const string APPLIANCE = "Appliance(Clone)";
        private const string OUTDOORMOVEMENTBLOCKER = "Outdoor Movement Blocker(Clone)";

        private int LAYER_PLAYERS;
        private int LAYER_DEFAULT;

        private bool m_isPrepTime = false;
        private SceneType m_sceneType = SceneType.Null;
        public bool NoclipKeyEnabled = true;

        public float SpeedIncrease = 1f;
        public static SmartNoClipMono Instance { get; private set; }
        public HashSet<Rigidbody> AllMyPlayerRigidbodies = new HashSet<Rigidbody>();

        private void Start()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);

            LAYER_PLAYERS = LayerMask.NameToLayer("Players");
            LAYER_DEFAULT = LayerMask.NameToLayer("Default");
        }
        private void Update()
        {
            // Logical order would be to have the conifg call first, but i suspect the unity internal call costs less
            if (Input.GetKeyDown(KeyCode.N)/* && Persistence.Instance["bGeneral_Mod_Active"].BoolValue*/)
            {
                NoclipKeyEnabled = !NoclipKeyEnabled;
                SmartNoClip.LogError($"Key: Enabled: {NoclipKeyEnabled}; PrepTime: {GameInfo.IsPreparationTime}; Scene: {GameInfo.CurrentScene}");
                SetNoClip();
            }
        }

        private static void DisableCollisions(bool ignore, string gameObjectName)
        {
            //SmartNoClip.LogError($"DisableCollisions. Ignore: {ignore}; Name: {gameObjectName}");
            Collider[] playerColliders;
            Collider[] targetColliders;
            try
            {
                playerColliders = GameObject.FindObjectOfType<PlayerView>().GetComponents<Collider>();
                targetColliders = GameObject.FindObjectsOfType<Collider>()?.Where(x => x.gameObject.activeSelf && x.gameObject.name == gameObjectName).ToArray();
            }
            catch (Exception)
            {
                throw; // These shouldn't find a problem, just in case i f something up
            }
            if (playerColliders != null && playerColliders.Length > 0 && targetColliders != null && targetColliders.Length > 0)
            {
                foreach (var item in targetColliders)
                {
                    Physics.IgnoreCollision(playerColliders[0], item, ignore);
                    Physics.IgnoreCollision(playerColliders[1], item, ignore);
                }
            }
        }

        public void PlayerView_Update_Prefix()
        {
            // Check for any kind of data change and execute noclip update
            if (GameInfo.IsPreparationTime != m_isPrepTime)
            {
                m_isPrepTime = GameInfo.IsPreparationTime;
                SetNoClip();
                return;
            }
            if (GameInfo.CurrentScene != m_sceneType)
            {
                m_sceneType = GameInfo.CurrentScene;
                SetNoClip();
                return;
            }
        }

        public void ApplianceView_SetPosition_Postfix()
        {
            if (NoClipActive)
            {
                DisableCollisions(true, HATCH); // In case a door gets replaced by a hatch
            }
        }

        public static bool NoClipActive
        {
            get {
                try {
                    return SmartNoClipMono.Instance.NoclipKeyEnabled &&
                        (
                           NoClipActive_AllowedInPrep
                        ||
                           NoClipActive_AllowedInDay
                        ||
                           NoClipActive_AllowedInHQ
                        )
                    ; }
                catch (Exception e )
                {
                    SmartNoClip.LogError(e.Message + " | " + e.StackTrace);
                    return false;
                }
                }
        }

        #region NoClipActiveRules
        private static bool NoClipActive_AllowedInPrep => GameInfo.IsPreparationTime
                        && GameInfo.CurrentScene == SceneType.Kitchen
                        && Persistence.Instance["bActive_Prep"].BoolValue;

        private static bool NoClipActive_AllowedInDay => !GameInfo.IsPreparationTime
                        && GameInfo.CurrentScene == SceneType.Kitchen
                        && Persistence.Instance["bActive_Day"].BoolValue;

        private static bool NoClipActive_AllowedInHQ => GameInfo.CurrentScene == SceneType.Franchise
                        && Persistence.Instance["bActive_HQ"].BoolValue;
        #endregion

        private void ChangeCollisionMode()
        {
            #region CollisionMode
            // Not sure if its necessary to change back to original, but it's the slightest bit more performant
            if (SmartNoClipMono.NoClipActive)
            {
                foreach (Rigidbody rig in AllMyPlayerRigidbodies)
                {
                    if (rig is not null) // if player still exists
                    {
                        try
                        {
                            rig.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                        }
                        catch (Exception e)
                        {
                            SmartNoClip.LogError(e.InnerException.Message + "\n" + e.StackTrace);
                        }
                    }
                    else
                        SmartNoClip.LogError("No collision change");
                }
            }
            else
            {
                foreach (Rigidbody rig in AllMyPlayerRigidbodies)
                {
                    if (rig is not null) // if player still exists
                    {
                        try
                        {
                            rig.collisionDetectionMode = CollisionDetectionMode.Discrete;
                        }
                        catch (Exception e)
                        {
                            SmartNoClip.LogError(e.InnerException.Message + "\n" + e.StackTrace);
                        }
                        SmartNoClip.LogError("No collision change");
                    }
                }
            }
            #endregion
        }

        public void PostConfigUpdated(string _changedValue)
        {
            Persistence.Instance?.SaveCurrentConfig();
            SetNoClip();
        }

        public void SetNoClip()
        {
            SmartNoClip.LogError($"Enabled: {NoclipKeyEnabled}; PrepTime: {GameInfo.IsPreparationTime}; Scene: {GameInfo.CurrentScene}");
            SpeedIncrease = NoClipActive ? Persistence.Instance["fSpeed_Value"].FloatValue : 1f;
            //DisableCollisions(enable, LARGEWALL);
            DisableCollisions(NoClipActive, SHORTWALL);
            DisableCollisions(NoClipActive, HATCH);
            // Thoses two are disabled by the default layer below
            //DisableCollisions(NoClipActive, DOOR); 
            //DisableCollisions(NoClipActive, LARGEDOOR);

            // Same same but different

            SmartNoClip.LogError(Physics.GetIgnoreLayerCollision(LAYER_PLAYERS, LAYER_DEFAULT));
            // Klappt irgendwie nicht mit false (wieder collision aktiv machen), also wird richtig gesetzt aber immer noch keine collision
            Physics.IgnoreLayerCollision(LAYER_PLAYERS, LAYER_DEFAULT, NoClipActive);
            SmartNoClip.LogError(Physics.GetIgnoreLayerCollision(LAYER_PLAYERS, LAYER_DEFAULT));

            #region OutDoorMovementBlocker
            if (NoClipActive) // OutDoorMovementBlocker should be collision by default, after "Default" layer is disabled this should still be on
            {
                Collider[] playerColliders;
                List<Collider> targetColliders = new List<Collider>();
                try
                {
                    playerColliders = GameObject.FindObjectOfType<PlayerView>().GetComponents<Collider>();
                    GameObject.FindObjectsOfType<ApplianceView>()?.Where(x => x.gameObject.activeSelf
                    && x.gameObject.name == APPLIANCE
                    && (x.transform.Find("Container")?.Find(OUTDOORMOVEMENTBLOCKER)?.name != OUTDOORMOVEMENTBLOCKER)
                    ).ForEach(appliance => targetColliders.AddRange(appliance.GetComponentsInChildren<Collider>()));
                }
                catch (Exception)
                {
                    throw; // These should find a problem, just in case i f something up
                }
                if (playerColliders != null && playerColliders.Length > 0 && targetColliders != null && targetColliders.Count > 0)
                {
                    foreach (var item in targetColliders)
                    {
                        Physics.IgnoreCollision(playerColliders[0], item, NoClipActive);
                        Physics.IgnoreCollision(playerColliders[1], item, NoClipActive);
                    }
                }
            }
            #endregion
            // Just active all
            else
            {
                // IgnoreCollisionLayer fix weil das irgendwie nicht geht:
                Collider[] playerColliders;
                List<Collider> targetColliders = new List<Collider>();
                try
                {
                    playerColliders = GameObject.FindObjectOfType<PlayerView>().GetComponents<Collider>();
                    GameObject.FindObjectsOfType<ApplianceView>()?.Where(x => x.gameObject.activeSelf
                    && x.gameObject.name == APPLIANCE
                    ).ForEach(appliance => targetColliders.AddRange(appliance.GetComponentsInChildren<Collider>()));
                }
                catch (Exception)
                {
                    throw; // These should find a problem, just in case i f something up
                }
                if (playerColliders != null && playerColliders.Length > 0 && targetColliders != null && targetColliders.Count > 0)
                {
                    foreach (var item in targetColliders)
                    {
                        Physics.IgnoreCollision(playerColliders[0], item, NoClipActive);
                        Physics.IgnoreCollision(playerColliders[1], item, NoClipActive);
                    }
                }
            }
        }
    }
}
