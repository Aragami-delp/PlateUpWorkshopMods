using Kitchen;
using KitchenData;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace OrderUI
{
    [UpdateAfter(typeof(SelectFishOfDay))]
    [UpdateBefore(typeof(CreateTwitchMenuOptions))]
    public class CreateTableMenuOptions : RestaurantSystem
    {
        private EntityQuery MenuItems;
        private EntityQuery Ingredients;
        private HashSet<int> TempIngredients = new HashSet<int>();
        private HashSet<ItemList> SelectedOptions = new HashSet<ItemList>();


        protected override void Initialise()
        {
            base.Initialise();
            this.MenuItems = this.GetEntityQuery((EntityQueryDesc)new QueryHelper().All((ComponentType)typeof(CMenuItem)).None((ComponentType)typeof(CDisabledMenuItem)));
            this.Ingredients = this.GetEntityQuery((ComponentType)typeof(CAvailableIngredient));
        }
        protected override void OnUpdate()
        {
            if (this.Has<SIsNightTime>() || !this.Has<STwitchOrderingActive>())
            {
                
            }
            else
            {
                if (false || this.Has<SIsDayFirstUpdate>())
                    return;
                this.SelectedOptions.Clear();
                int index1 = 0;
                for (int index2 = 0; index2 < 20; ++index2)
                {
                    if (this.CreateOption(index1))
                    {
                        ++index1;
                        if (index1 >= 3)
                            break;
                    }
                }
            }
        }

        private bool CreateOption(int index)
        {
            Debug.LogError(index);
            using (NativeArray<Entity> entityArray = this.MenuItems.ToEntityArray(Allocator.TempJob))
            {
                using (NativeArray<CMenuItem> componentDataArray1 = this.MenuItems.ToComponentDataArray<CMenuItem>(Allocator.TempJob))
                {
                    using (NativeArray<CAvailableIngredient> componentDataArray2 = this.Ingredients.ToComponentDataArray<CAvailableIngredient>(Allocator.TempJob))
                    {
                        if (entityArray.Length == 0)
                            return false;
                        int index1 = this.PickRandomMenuItem(componentDataArray1, MenuPhase.Main);
                        Debug.LogError(index1);
                        if (index1 == -1)
                            return false;
                        CMenuItem cmenuItem = componentDataArray1[index1];
                        NativeArray<CAvailableIngredient> nativeArray = componentDataArray2;
                        this.TempIngredients.Clear();
                        for (int index2 = 0; index2 < nativeArray.Length; ++index2)
                        {
                            CAvailableIngredient cavailableIngredient = nativeArray[index2];
                            if (cavailableIngredient.MenuItem == cmenuItem.Item)
                                this.TempIngredients.Add(cavailableIngredient.Ingredient);
                        }
                        int num = cmenuItem.Item;
                        Item output;
                        if (!this.Data.TryGet<Item>(num, out output, true))
                            return false;
                        ItemList other = output is ItemGroup ? this.Data.ItemSetView.GetRandomConfiguration(num, this.TempIngredients) : new ItemList(num);
                        foreach (ItemList selectedOption in this.SelectedOptions)
                        {
                            if (selectedOption.IsEquivalent(other))
                                return false;
                        }
                        this.SelectedOptions.Add(other);
                        Entity entity = this.EntityManager.CreateEntity();
                        this.EntityManager.AddComponentData<CRequiresView>(entity, new CRequiresView()
                        {
                            Type = ViewType.TwitchOrderOption,
                            ViewMode = ViewMode.Screen
                        });
                        EntityManager entityManager = this.EntityManager;
                        //entityManager.AddComponentData<C>(entity, new CTwitchOrderOption()
                        //{
                        //    Index = index + 1
                        //});
                        entityManager = this.EntityManager;
                        entityManager.AddComponentData<CPosition>(entity, new CPosition(new Vector3(0.0f, 1f, 0.0f)));
                        entityManager = this.EntityManager;
                        entityManager.AddComponentData<CItem>(entity, new CItem()
                        {
                            ID = num,
                            Items = other
                        });
                        return true;
                    }
                }
            }
        }

        public int PickRandomMenuItem(NativeArray<CMenuItem> items, MenuPhase phase)
        {
            float maxInclusive = 0.0f;
            foreach (CMenuItem cmenuItem in items)
            {
                if (cmenuItem.Phase == phase)
                    maxInclusive += cmenuItem.Weight;
            }
            float num = Random.Range(0.0f, maxInclusive);
            for (int index = 0; index < items.Length; ++index)
            {
                if (items[index].Phase == phase)
                {
                    num -= items[index].Weight;
                    if ((double)num <= 0.0)
                        return index;
                }
            }
            return -1;
        }
    }
}
