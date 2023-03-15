using Kitchen;
using KitchenData;
using KitchenMods;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Linq;
using static Kitchen.StartPracticePopup;
using System.Net;

namespace KitchenDragNDropDesigner
{
    public class Mod : GenericSystemBase, IModSystem, IModInitializer
    {
        public const string MOD_GUID = "aragami.plateup.mods.dragndropdesigner";
        public const string MOD_NAME = "DragNDropDesigner";
        public const string MOD_VERSION = "0.1.0";
        //public const string MOD_AUTHOR = "Aragami";
        //public const string MOD_GAMEVERSION = ">=1.1.3";

        private readonly HarmonyLib.Harmony m_harmony = new HarmonyLib.Harmony("aragami.plateup.harmony.dragndropdesigner");

        public Mod() : base() { }

        protected override void Initialise()
        {
            base.Initialise();
            // For log file output so the official plateup support staff can identify if/which a mod is being used
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            World.GetExistingSystem<PickUpAndDropAppliance>().Enabled = false;
        }

        protected override void OnUpdate()
        {

        }

        #region Logging
        // You can remove this, I just prefer a more standardized logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion

        public void PostActivate(KitchenMods.Mod mod)
        {
            m_harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void PreInject()
        {

        }

        public void PostInject()
        {

        }
    }

    public static class Helper
    {
        /// <summary>
        /// Mouse position in world at Y=0
        /// </summary>
        /// <returns>Position on plane; otherwise 0,0,0</returns>
        public static Vector3 MousePlanePos()
        {
            Plane plane = new Plane(Vector3.down, 0);
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (plane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Gets a MethodInfo of a given class using Reflection, that doesn't have parameters
        /// </summary>
        /// <param name="_typeOfOriginal">Type of class to find a Method on</param>
        /// <param name="_name">Name of the Method to find</param>
        /// <param name="_genericT">Type of Method</param>
        /// <returns>MethodInfo if found</returns>
        public static MethodInfo GetMethod(Type _typeOfOriginal, string _name, Type _genericT = null)
        {
            MethodInfo retVal = _typeOfOriginal.GetMethod(_name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (_genericT != null)
            {
                retVal = retVal.MakeGenericMethod(_genericT);
            }
            return retVal;
        }

        /// <summary>
        /// Gets a MethodInfo of a given class using Reflection, that has Parameters
        /// </summary>
        /// <param name="_typeOfOriginal">Type of class to find a Method on</param>
        /// <param name="_name">Name of the Method to find</param>
        /// <param name="_paramTypes">Types of parameters of the Method in right order</param>
        /// <param name="_genericT">Type of Method</param>
        /// <returns>MethodInfo if found</returns>
        public static MethodInfo GetMethod(Type _typeOfOriginal, string _name, Type[] _paramTypes, Type _genericT = null)
        {
            MethodInfo retVal = _typeOfOriginal.GetMethod(_name, BindingFlags.NonPublic | BindingFlags.Instance, null, _paramTypes, null);
            if (_genericT != null)
            {
                retVal = retVal.MakeGenericMethod(_genericT);
            }
            return retVal;
        }
    }

    #region Systems
    [UpdateBefore(typeof(PickUpAndDropAppliance))]
    public class MousePickUpAndDropAppliance : ApplianceInteractionSystem
    {
        private Bounds CachedBounds;
        private bool MadeChanges;
        private CPosition Position;
        private CItemHolder Holder;
        public static bool isPickedUpByMouse { get; private set; } = false;

        protected override InteractionType RequiredType => InteractionType.Grab;

        protected override bool UseImmediateContext => true;

        protected override bool BeforeRun()
        {
            base.BeforeRun();
            if (!Has<SLayout>())
                return false;
            CachedBounds = Bounds;
            CachedBounds.Encapsulate(GetFrontDoor() + new Vector3(0.0f, 0.0f, -2f));
            CachedBounds.Expand(0.5f);
            MadeChanges = false;
            return true;
        }

        protected override void AfterRun()
        {
            base.AfterRun();
            if (!MadeChanges)
                return;
            EntityManager.CreateEntity((ComponentType)typeof(CRequireStartDayWarningRecalculation));
            if (!HasSingleton<SPerformTableUpdate>())
                EntityManager.CreateEntity((ComponentType)typeof(SPerformTableUpdate));
        }

        protected override bool IsPossible(ref InteractionData data) => Require<CPosition>(data.Interactor, out Position) && Require<CItemHolder>(data.Interactor, out Holder) && Perform(ref data, false);

        protected override void Perform(ref InteractionData data)
        {
            Perform(ref data, data.ShouldAct);
        }

        protected bool Perform(ref InteractionData data, bool should_act)
        {
            bool isMouseInteraction = false;
            if (Mouse.current.leftButton.IsPressed())
            {
                data.Attempt.Location = Helper.MousePlanePos();
                isMouseInteraction = true;
            }

            CAttemptingInteraction attempt = data.Attempt;
            bool flag1 = false;
            bool flagChangesDone = !(Holder.HeldItem != new Entity()) ? flag1 | PerformPickUp(data.Context, data.Interactor, ref attempt, in Position, data.ShouldAct & should_act, OccupancyLayer.Default, isMouseInteraction) : flag1 | PerformDrop(data.Context, data.Interactor, ref attempt, Holder, Position, data.ShouldAct & should_act, isMouseInteraction);
            if (should_act)
                MadeChanges |= flagChangesDone;
            return flagChangesDone;
        }

        private bool IsHeldItemFloorOccupier(Entity item)
        {
            CAppliance comp;
            return Require<CAppliance>(item, out comp) && comp.Layer == OccupancyLayer.Floor;
        }

        private bool PerformDrop(
          EntityContext ctx,
          Entity player,
          ref CAttemptingInteraction interact,
          CItemHolder player_holder,
          CPosition pos,
          bool should_act,
          bool isMouseInteraction)
        {
            EntityManager entityManager = EntityManager;
            bool flagFalse = false;
            if ((!CanReach((Vector3)pos, interact.Location) && !isMouseInteraction) || GetFrontDoor().IsSameTile(interact.Location) || (GetFrontDoor(true).IsSameTile(interact.Location) || !CachedBounds.Contains(interact.Location)))
                return false;
            CLayoutRoomTile tile = GetTile(interact.Location);
            Vector3 position = interact.Location.Rounded();
            Quaternion quaternion = Quaternion.identity;
            Entity heldItem = player_holder.HeldItem;
            CAppliance component;
            if (!entityManager.RequireComponent<CAppliance>(heldItem, out component))
                return false;
            OccupancyLayer layer = component.Layer;
            bool flag2 = false;
            foreach (Orientation preferredRotation in OrientationHelpers.PreferredRotations)
            {
                Vector3 offset = preferredRotation.ToOffset();
                if (GetRoom(position + offset) != tile.RoomID)
                {
                    flag2 = !tile.CanReach(preferredRotation);
                    quaternion = (Quaternion)preferredRotation.ToRotation();
                    break;
                }
            }
            bool flag3 = tile.RoomID == 0;
            if (flag3 && HasComponent<CUnsellableAppliance>(heldItem))
                return false;
            if (!flag3 && HasComponent<CMustHaveWall>(heldItem) && !flag2)
                return flagFalse;
            bool flag4 = false;
            Entity occupant1 = GetOccupant(position, layer);
            if (occupant1 != new Entity() && !HasComponent<CAllowPlacingOver>(occupant1))
            {
                if (!PerformPickUp(ctx, player, ref interact, in pos, false, layer, isMouseInteraction))
                    return false;
                flag4 = true;
            }
            if (layer == OccupancyLayer.Floor && !flag4)
            {
                Entity occupant2 = GetOccupant(position);
                if (occupant2 != new Entity() && !HasComponent<CAllowPlacingOver>(occupant2) && !PerformPickUp(ctx, player, ref interact, in pos, false, OccupancyLayer.Default, isMouseInteraction))
                    return false;
                flag4 = true;
            }
            bool flag5 = true;
            if (should_act)
            {
                CPosition data = CPosition.Rounded(position);
                data.Rotation = (quaternion)quaternion;
                ctx.Remove<CHeldAppliance>(heldItem);
                ctx.Remove<CHeldBy>(heldItem);
                ctx.Add<CRemoveView>(heldItem);
                ctx.Set<CRequiresView>(heldItem, new CRequiresView()
                {
                    Type = ViewType.Appliance
                });
                ctx.Set<CPosition>(heldItem, data);
                if (flag4)
                    PerformPickUp(ctx, player, ref interact, in pos, true, layer, isMouseInteraction);
                else
                    ctx.Set<CItemHolder>(player, new CItemHolder());
                SetOccupant(position, heldItem);
                isPickedUpByMouse = flag4;
            }
            return flag5;
        }

        private bool PerformPickUp(
          EntityContext ctx,
          Entity player,
          ref CAttemptingInteraction interact,
          in CPosition pos,
          bool should_act,
          OccupancyLayer layer,
          bool isMouseInteraction)
        {
            EntityManager entityManager = EntityManager;
            bool flag = false;
            Entity entity1 = new Entity();
            if (!CanReach((Vector3)pos, interact.Location) && !isMouseInteraction)
                return flag;
            Entity entity2 = layer != OccupancyLayer.Default ? GetOccupant(interact.Location, layer) : GetPrimaryOccupant(interact.Location);
            if (entity2 == new Entity())
            {
                entity2 = interact.Target;
                CPosition comp;
                if (!Require<CPosition>(entity2, out comp) || !comp.Position.IsSameTile(interact.Location))
                    return false;
            }
            CAppliance component;
            if (entityManager.RequireComponent<CAppliance>(entity2, out component) && !HasComponent<CHeldAppliance>(entity2) && !HasComponent<CImmovable>(entity2))
            {
                if (should_act)
                {
                    ctx.Add<CHeldAppliance>(entity2);
                    ctx.Add<CHeldBy>(entity2);
                    ctx.Add<CRemoveView>(entity2);
                    ctx.Set<CRequiresView>(entity2, new CRequiresView()
                    {
                        Type = ViewType.HeldAppliance
                    });
                    ctx.Set<CHeldBy>(entity2, new CHeldBy()
                    {
                        Holder = player
                    });
                    ctx.Set<CPosition>(entity2, CPosition.Hidden);
                    ctx.Set<CItemHolder>(player, new CItemHolder()
                    {
                        HeldItem = entity2
                    });
                    SetOccupant(interact.Location, new Entity(), component.Layer);
                    isPickedUpByMouse = true; // or isPickedUpByMouse = flag; two lines below
                }
                flag = true;
                interact.Result = should_act ? InteractionResult.Performed : InteractionResult.Possible;
            }
            return flag;
        }

        protected override void OnCreateForCompiler() => base.OnCreateForCompiler();
    }
    #endregion

    #region Patches
    // Prefer a harmony patch, since the system is partially overwritten by a KitchenLib System
    // Can't patch "IsPossible()" since it's inline
    [HarmonyPatch(typeof(RotateAppliances), "IsPossible")]
    class RotateAppliancesIsPossible_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(RotateAppliances __instance, ref InteractionData data, ref CPosition ___Position, ref bool __result)
        {
            data.Attempt.Location = Helper.MousePlanePos();

            MethodInfo mthHas0 = null;
            MethodInfo mthHas1 = null;
            MethodInfo mthHas2 = null;

            MethodInfo mthHasComponent = null;
            MethodInfo mthGetComponent = null;

            try { mthHas0 = Helper.GetMethod(typeof(RotateAppliances), "Has", new Type[] { typeof(Entity) }, typeof(CMustHaveWall)); } catch (Exception _ex) { Mod.LogError("Hi1"); }
            try { mthHas1 = Helper.GetMethod(typeof(RotateAppliances), "Has", new Type[] { typeof(Entity) }, typeof(CFixedRotation)); } catch (Exception _ex) { Mod.LogError("Hi2"); }
            try { mthHas2 = Helper.GetMethod(typeof(RotateAppliances), "Has", new Type[] { typeof(Entity) }, typeof(CAppliance)); } catch (Exception _ex) { Mod.LogError("Hi3"); }

            try { mthHasComponent = Helper.GetMethod(typeof(RotateAppliances), "HasComponent", new Type[] { typeof(Entity) }, typeof(CPosition)); } catch (Exception _ex) { Mod.LogError("Hi4"); }
            try { mthGetComponent = Helper.GetMethod(typeof(RotateAppliances), "GetComponent", new Type[] { typeof(Entity) }, typeof(CPosition)); } catch (Exception _ex) { Mod.LogError("Hi5"); }

            bool require;
            //
            ___Position = default(CPosition);
            if (!(bool)mthHasComponent.Invoke(__instance, new object[] { data.Target }))
            {
                require = false;
            }
            else
            {
                bool isZeroSized = TypeManager.GetTypeInfo(TypeManager.GetTypeIndex<CPosition>()).IsZeroSized;
                ___Position = isZeroSized ? new CPosition() : (CPosition)mthGetComponent.Invoke(__instance, new object[] { data.Target });
                require = true;
            }
            //

            bool resRequire = require;
            //_instance.Require<CPosition>(data.Target, out this.Position) && !_instance.Has<CMustHaveWall>(data.Target) && (!_instance.Has<CFixedRotation>(data.Target) && _instance.Has<CAppliance>(data.Target));
            bool resHas0 = (bool)mthHas0.Invoke(__instance, new object[] { data.Target });
            bool resHas1 = (bool)mthHas1.Invoke(__instance, new object[] { data.Target });
            bool resHas2 = (bool)mthHas2.Invoke(__instance, new object[] { data.Target });

            __result = resRequire && !resHas0 && (!resHas1 && resHas2);
            __result = true;
            ___Position = Helper.MousePlanePos();

            return true;
        }
    }
    [HarmonyPatch(typeof(RotateAppliances), "Perform")]
    class RotateAppliancesPerform_Patch
    {
        [HarmonyPrefix]
        static void Prefix(ref InteractionData data, ref CPosition ___Position)
        {
            Mod.LogError("Rotate");
            data.Attempt.Location = ___Position;
        }
    }

    [HarmonyPatch]
    public static class ManageApplianceGhostsOriginalLambdaBodyPatch
    {
        static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(ManageApplianceGhosts), t => t.Name.Contains("c__DisplayClass_OnUpdate_LambdaJob1"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        static void Prefix(ref CAttemptingInteraction interact)
        {
            if (MousePickUpAndDropAppliance.isPickedUpByMouse)
            {
                interact.Location = Helper.MousePlanePos();
            }
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> _instructions)
        {
            var codes = new List<CodeInstruction>(_instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldloc_0 &&
                    codes[i + 1].opcode == OpCodes.Ldarg_0 &&
                    codes[i + 2].opcode == OpCodes.Ldfld &&
                    codes[i + 3].opcode == OpCodes.Ldarg_3 &&
                    codes[i + 4].opcode == OpCodes.Ldobj &&
                    codes[i + 5].opcode == OpCodes.Call &&
                    codes[i + 6].opcode == OpCodes.Ldarg_2 &&
                    codes[i + 7].opcode == OpCodes.Ldfld &&
                    codes[i + 8].opcode == OpCodes.Ldc_I4_0 &&
                    codes[i + 9].opcode == OpCodes.Call &&
                    codes[i + 10].opcode == OpCodes.And &&
                    codes[i + 11].opcode == OpCodes.Stloc_0)
                {
                    codes[i + 0].opcode = OpCodes.Ldc_I4_1;
                    codes[i + 1].opcode = OpCodes.Stloc_0;
                    for (int j = 2; j < 12; j++)
                    {
                        codes[i + j].opcode = OpCodes.Nop;
                    }
                }
            }

            return codes.AsEnumerable();
        }
    }
    #endregion
}
