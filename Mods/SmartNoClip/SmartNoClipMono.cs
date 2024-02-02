using Kitchen;
using KitchenMods;
using UnityEngine;
using HarmonyLib;
using TMPro;
using System.Linq;
using System;
using Shapes;
using System.Collections.Generic;

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

        private bool m_isPrepTime = false;
        private SceneType m_sceneType = SceneType.Null;
        public bool NoclipEnabled = false;

        public float SpeedIncrease = 1f;
        public static SmartNoClipMono Instance { get; private set; }
        public HashSet<Rigidbody> AllMyPlayerRigidbodies = new();
        public CollisionDetectionMode OriginalCollisionMode;

        private void Start()
        {
            if (Instance != null)
            {
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                NoclipEnabled = !NoclipEnabled;
                SetNoClip();
            }
        }

        private static void DisableCollisions(bool enable, string gameObjectName)
        {
            Collider[] playerColliders;
            Collider[] targetColliders;
            try
            {
                playerColliders = GameObject.FindObjectOfType<PlayerView>().GetComponents<Collider>();
                targetColliders = GameObject.FindObjectsOfType<Collider>()?.Where(x => x.gameObject.activeSelf && x.gameObject.name == gameObjectName).ToArray();
            }
            catch (Exception)
            {
                throw; // These should find a problem, just in case i f something up
            }
            if (playerColliders != null && playerColliders.Length > 0 && targetColliders != null && targetColliders.Length > 0)
            {
                foreach (var item in targetColliders)
                {
                    Physics.IgnoreCollision(playerColliders[0], item, enable);
                    Physics.IgnoreCollision(playerColliders[1], item, enable);
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
            //if (GameInfo.IsPreparationTime != m_isPrepTime)
            //{
            //    m_isPrepTime = GameInfo.IsPreparationTime;
            //    SetNoClip();
            //}
        }

        public static bool NoClipActive
        {
            get => GameInfo.IsPreparationTime && GameInfo.CurrentScene == SceneType.Kitchen && SmartNoClipMono.Instance.NoclipEnabled;
        }

        private void ChangeCollisionMode()
        {
            #region CollisionMode
            // Not sure if its necessary to change back to original, but it's the slightest bit more performant
            if (SmartNoClipMono.NoClipActive)
            {
                foreach (Rigidbody rig in AllMyPlayerRigidbodies)
                {
                    if (rig is not null) // if player still exists
                        rig.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }
            }
            else
            {
                foreach (Rigidbody rig in AllMyPlayerRigidbodies)
                {
                    if (rig is not null) // if player still exists
                        rig.collisionDetectionMode = OriginalCollisionMode;
                }
            }
            #endregion
        }

        public void SetNoClip()
        {
            SmartNoClip.LogError($"Enabled: {NoclipEnabled}; PrepTime: {GameInfo.IsPreparationTime}; Scene: {GameInfo.CurrentScene}");
            //rigidbody.detectCollisions = !enable;
            SpeedIncrease = NoClipActive ? 2f : 1f;
            //DisableCollisions(enable, LARGEWALL);
            DisableCollisions(NoClipActive, SHORTWALL);
            DisableCollisions(NoClipActive, HATCH);
            // Thoses two are disabled by the default layer below
            //DisableCollisions(NoClipActive, DOOR); 
            //DisableCollisions(NoClipActive, LARGEDOOR);

            // Same same but different

            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Players"), LayerMask.NameToLayer("Default"), NoClipActive); // Player?! // Outer walls?

            #region OutDoorMovementBlocker
            if (NoclipEnabled) // OutDoorMovementBlocker should be collision by default, after "Default" layer is disabled this should still be on
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
        }
    }
}
