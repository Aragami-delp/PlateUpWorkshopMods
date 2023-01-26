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

namespace KitchenDragNDropDesigner
{
    public class Mod : GenericSystemBase, IModSystem
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
            m_harmony.PatchAll(Assembly.GetExecutingAssembly());
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
                isPickedUpByMouse = false;
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
                    isPickedUpByMouse = true;
                }
                flag = true;
                interact.Result = should_act ? InteractionResult.Performed : InteractionResult.Possible;
            }
            return flag;
        }

        protected override void OnCreateForCompiler() => base.OnCreateForCompiler();
    }
    #endregion

    #region Add SaveSystem to pause menu
    [HarmonyPatch]
    public static class ManageApplianceGhostsOriginalLambdaBodyPatch
    {
        static MethodBase TargetMethod()
        {
            Type type = AccessTools.FirstInner(typeof(ManageApplianceGhosts), t => t.Name.Contains("c__DisplayClass_OnUpdate_LambdaJob1"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OriginalLambdaBody"));
        }

        static void Prefix(ref CAttemptingInteraction interact) // Not sure what the other "CanReach" in the method does - but it works - it shows in other rooms and should be patched
        {
            if (MousePickUpAndDropAppliance.isPickedUpByMouse)
            {
                interact.Location = Helper.MousePlanePos();
            }
        }
    }
    #endregion
}
