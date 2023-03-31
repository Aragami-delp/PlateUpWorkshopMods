using Kitchen;
using KitchenMods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace OrderUI
{
    public class GetGroupsOrderUISystem : DaySystem, IModSystem
    {
        private EntityQuery ActiveOrderGroups;
        private EntityQuery LeavingGroups;
        private HashSet<Entity> StoredGroups = new HashSet<Entity>();

        struct CHasBeenSetOnFire : IModComponent { }

        protected override void Initialise()
        {
            base.Initialise();
            ActiveOrderGroups = GetEntityQuery(new QueryHelper()
                    .All(
                        typeof(CAssignedTable),
                        typeof(CPosition),
                        typeof(CHasItemCollectionIndicator),
                        typeof(CCustomerSettings),
                        typeof(CGroupMealPhase),
                        typeof(CWaitingForItem),
                        typeof(CCustomerGroup),
                        typeof(CGroupPhaseFood),
                        typeof(CAtTable))
                    .None(typeof(CGroupLeaving)
                    ));
            LeavingGroups = GetEntityQuery(new QueryHelper()
                    .All(
                        typeof(CPosition),
                        typeof(CCustomerSettings),
                        typeof(CGroupMealPhase),
                        typeof(CCustomerGroup)
                    )
                    .Any(
                        typeof(CGroupLeaving),
                        typeof(CGroupStartLeaving),
                        typeof(CGroupEating)
                    ));
        }

        protected override void OnUpdate()
        {
            var activeOrderGroups = ActiveOrderGroups.ToEntityArray(Allocator.TempJob);
            foreach (var activeOrderGroup in activeOrderGroups)
            {
                if (!StoredGroups.Contains(activeOrderGroup))
                {
                    StoredGroups.Add(activeOrderGroup);
                    OrderUIMonoManager.Instance.NewGroupOrder(activeOrderGroup);
                    Debug.LogError("Group " + activeOrderGroup.Index.ToString() + " takes Order!");
                }
            }
            activeOrderGroups.Dispose();

            var leavingGroups = LeavingGroups.ToEntityArray(Allocator.TempJob);
            foreach (var leavingGroup in leavingGroups)
            {
                if (StoredGroups.Contains(leavingGroup))
                {
                    StoredGroups.Remove(leavingGroup);
                    OrderUIMonoManager.Instance.GroupOrderFinished(leavingGroup);
                    Debug.LogError("Group " + leavingGroup.Index.ToString() + " leaves!");
                }
            }
            leavingGroups.Dispose();

            // TODO: cases for ending without all customers leaving (something like game over)
        }

        //protected override void OnDestroy()
        //{
        //    base.OnDestroy();
        //    ActiveOrderGroups.Dispose();
        //    LeavingGroups.Dispose();
        //}
    }
}
