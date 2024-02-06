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
        [HarmonyPostfix]
        public static void SetPosition_DisableCollision(ApplianceView __instance, bool ___SkipRotationAnimation, UpdateViewPositionData pos, Animator ___Animator)
        {
            // Only when tiles get picket up or placed this is called
            SmartNoClipMono.Instance.ApplianceView_SetPosition_Postfix();
        }
    }

    [HarmonyPatch(typeof(PlayerView), nameof(PlayerView.Initialise))]
    public class NoClipPatch_PlayerView_Initialise
    {
        [HarmonyPrefix]
        public static void Initialise_InitPlayerValues(PlayerView __instance, bool ___IsMyPlayer, Rigidbody ___Rigidbody)
        {
            // TODO: Maybe add to mod active options
            ___Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // Do for all players to avoid that effect when they collider with something, even when own noclip is disabled
        }
    }

    [HarmonyPatch(typeof(PlayerView), "Update")]
    public class NoClipPatch_PlayerView_Update
    {
        public static float GetPlayerSpeedMultipler // Has to stay in this class to make transpiler easier
        {
            get
            {
                return SmartNoClipMono.NoClipActive ? SmartNoClipMono.Instance.SpeedIncrease : 1f;
            }
        }

        // Adjusts player speed by multiplying, instead of setting a fixed value
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            var code = new List<CodeInstruction>(instructions);
            var myStaticPropertyGetter = AccessTools.PropertyGetter(typeof(NoClipPatch_PlayerView_Update), nameof(GetPlayerSpeedMultipler));

            int insertionIndex = -1;
            for (int i = 0; i < code.Count - 9; i++) // -9 since we will be checking i + 9
            {
                //if (code[i].opcode == OpCodes.Ldc_I4 && (int)code[i].operand == 566 && code[i + 1].opcode == OpCodes.Ret)
                if (code[i].opcode == OpCodes.Ldarg_0
                    && code[i + 1].opcode == OpCodes.Ldfld
                    && code[i + 2].opcode == OpCodes.Ldloc_1
                    && code[i + 3].opcode == OpCodes.Mul
                    && code[i + 4].opcode == OpCodes.Stloc_S
                    && code[i + 5].opcode == OpCodes.Ldarg_0
                    && code[i + 6].opcode == OpCodes.Call
                    && code[i + 7].opcode == OpCodes.Callvirt
                    && code[i + 8].opcode == OpCodes.Ldloc_S
                    && code[i + 9].opcode == OpCodes.Call)
                {
                    insertionIndex = i + 4;
                    break;
                }
            }

            if (insertionIndex != -1)
            {
                var instructionsToInsert = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, myStaticPropertyGetter),
                    new CodeInstruction(OpCodes.Mul)
                };
                code.InsertRange(insertionIndex, instructionsToInsert);
            }
            return code;
        }

        [HarmonyPrefix]
        public static void Update_CheckPrepState()
        {
            SmartNoClipMono.Instance.PlayerView_Update_Prefix();
        }
    }

    [HarmonyPatch(typeof(EnforcePlayerBounds), "OnUpdate")]
    public class BoundariesPatch
    {
        [HarmonyPrefix]
        public static bool OnUpdate_DisableBounds(EnforcePlayerBounds __instance)
        {
            return !Persistence.Instance["bAllow_Players_Outside"].BoolValue;
        }
    }
}
