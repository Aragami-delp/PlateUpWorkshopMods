using HarmonyLib;
using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KitchenSmartNoClip
{
    [HarmonyPatch(typeof(ApplianceView), nameof(HeldApplianceView.SetPosition))]
    public class NoClipPatch_ApplianceView_SetPosition
    {
        /// <summary>
        /// Called when an appliance is placed (moved?) or placed down
        /// </summary>
        [HarmonyPostfix]
        public static void SetPosition_DisableCollision(ApplianceView __instance, bool ___SkipRotationAnimation, UpdateViewPositionData pos, Animator ___Animator)
        {
            // Only when tiles get picket up or placed this is called
            SmartNoClipMono.Instance.ApplianceView_SetPosition_Postfix(); 
        }
    }

    [HarmonyPatch(typeof(PlayerWalkingComponent), nameof(PlayerWalkingComponent.UpdateMovement))]
    public class NoClipPatch_PlayerWalkingComponent_UpdateMovement
    {
        /// <summary>
        /// Called when the player moves
        /// </summary>
        [HarmonyPrefix]
        public static void Initialise_InitPlayerValues(ref float base_speed, bool ___IsMyPlayer, Rigidbody ___Rigidbody)
        {
            if (___Rigidbody.collisionDetectionMode != CollisionDetectionMode.ContinuousSpeculative)
            {
                ___Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // TODO: Find more effective way, to not do it every frame
            }

            base_speed *= SmartNoClipMono.NoClipActive ? SmartNoClipMono.Instance.SpeedIncrease : 1f;
        }
    }

    [HarmonyPatch(typeof(PlayerView), "Update")]
    public class NoClipPatch_PlayerView_Update
    {
        /// <summary>
        /// Called every frame
        /// </summary>
        [HarmonyPrefix]
        public static void Update_CheckPrepState()
        {
            SmartNoClipMono.Instance.PlayerView_Update_Prefix();
        }
    }

    [HarmonyPatch(typeof(EnforcePlayerBounds), "OnUpdate")]
    public class BoundariesPatch
    {
        /// <summary>
        /// Allow for out of bounds players
        /// </summary>
        [HarmonyPrefix]
        public static bool OnUpdate_DisableBounds(EnforcePlayerBounds __instance)
        {
            return !(GameInfo.IsPreparationTime
                        && GameInfo.CurrentScene == SceneType.Kitchen
                        && Persistence.Instance["bAllow_Players_Outside"].BoolValue);
        }
    }
}
