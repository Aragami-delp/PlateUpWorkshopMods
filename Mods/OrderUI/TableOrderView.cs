using Kitchen;
using KitchenMods;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;

namespace KitchenOrderUI
{
    public class TableOrderView : UpdatableObjectView<TableOrderView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Views;

            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(new QueryHelper()
                                           .All(typeof(CCustomerSettings), typeof(CMyComponent))
                                       );
            }

            protected override void OnUpdate()
            {
                using var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                using var components = Views.ToComponentDataArray<CMyComponent>(Allocator.Temp);

                for (var i = 0; i < views.Length; i++)
                {
                    var view = views[i];
                    var data = components[i];

                    SendUpdate(view, new ViewData
                    {
                        MySentData1 = data.MyValue1,
                        MySentData2 = data.MyValue2
                    }, MessageType.SpecificViewUpdate);
                }
            }
        }

        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            public int MySentData1;
            public int MySentData2;

            // this tells the game how to find this subview within a prefab
            // GetSubView<T> is a cached method that looks for the requested T in the view and its children
            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<TableOrderView>();

            // this is used to determine if the data needs to be sent again
            public bool IsChangedFrom(ViewData check) => MySentData1 != check.MySentData1 ||
                                                         MySentData2 != check.MySentData2;
        }

        // this receives the updated data from the ECS backend whenever a new update is sent
        // in general, this should update the state of the view to match the values in view_data
        // ideally ignoring all current state; it's possible that not all updates will be received so
        // you should avoid relying on previous state where possible
        protected override void UpdateData(ViewData view_data)
        {
            // perform the update here
            // this is a Unity MonoBehavior so we can do normal Unity things here
        }
    }
