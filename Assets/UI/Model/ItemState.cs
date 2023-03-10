﻿#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using System;
using System.Linq;
using System.ComponentModel;
using OurFramework.UI.Util;
#if NOESIS
using OurFramework.Gameplay.RealWorld;
#endif

namespace OurFramework.Gameplay.Data
{
    /// <summary>
    /// State of item.
    /// </summary>
#if NOESIS
    [Serializable]
#endif
    public class ItemState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

#if NOESIS
        [SerializeField]
#endif
        string _name;

        public string Name
        {
            get => _name;
            set { _name = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public ItemState SetName(string name)
        {
            Name = name;
            return this;
        }

#if NOESIS
        [SerializeField]
#endif
        string _description;

        public string Description
        {
            get => _description;
            set { _description = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public ItemState SetDescription(string description)
        {
            Description = description;
            return this;
        }

        int _stacksCount;
        public int StacksCount
        {
            get => _stacksCount;
            set { _stacksCount = Math.Max(0, value); PropertyChanged.OnPropertyChanged(this); }
        }

        bool _isStackable;
        public bool IsStackable
        {
            get => _isStackable;
            set { _isStackable = value; PropertyChanged.OnPropertyChanged(this); }
        }

        /// <summary>
        /// Describes how the item is used - consumable, stackable, replenishable, wearable.
        /// </summary>
        ItemUsage Usage { get; set; }

        public ItemState SetConsumable()
        {
            Usage = new ConsumableItemUsage(this);
            return this;
        }

        public ItemState SetStackable(int stacksCount = 1, bool canBeUsed = true)
        {
            Usage = new StackableItemUsage(this, stacksCount, canBeUsed);
            return this;
        }

        public ItemState SetReplenishable(int stacksCount)
        {
            Usage = new ReplenishableItemUsage(this, stacksCount);
            return this;
        }

        public ItemState SetWearable(SlotType slotType)
        {
            Usage = new WearableItemUsage(this, slotType);
            return this;
        }

        public ItemState()
        {
            Name = "";
            Description = "";
            OnUseDelegate = _ => { };
            OnDropDelegate = _ => { };
            OnUpdateDelegate = _ => { };
            OnEquipDelegate = _ => { };
            OnUnequipDelegate = _ => { };
            IsStackable = false;
            Usage = new ItemUsage();
        }

        public Effect OnUseDelegate { get; protected set; }
        public virtual ItemState OnUse(Effect characterAction)
        {
            OnUseDelegate += character =>
            {
                if(Usage.TryUse(character.Inventory, this))
                {
                    characterAction(character);
                }
            };
            return this;
        }

        public Effect OnDropDelegate { get; protected set; }
        public ItemState OnDrop(Effect characterAction)
        {
            OnDropDelegate = characterAction;
            return this;
            //Debug.Log($"Dropping {Name}");
        }

        public Effect OnUpdateDelegate { get; protected set; }
        public ItemState OnUpdate(Effect characterAction)
        {
            OnUpdateDelegate = characterAction;
            return this;
            //Debug.Log($"Updating {Name}");
        }

        public Effect OnEquipDelegate { get; protected set; }
        public ItemState OnEquip(Effect characterAction)
        {
            OnEquipDelegate = characterAction;
            return this;
            //Debug.Log($"Dropping {Name}");
        }

        public Effect OnUnequipDelegate { get; protected set; }
        public ItemState OnUnequip(Effect characterAction)
        {
            OnUnequipDelegate = characterAction;
            return this;
            //Debug.Log($"Dropping {Name}");
        }

        public InventorySlot AddToInventory(Inventory inventory)
        {
            return Usage.AddToInventory(inventory, this);
        }

        public virtual InventorySlot EquipToFree(Inventory inventory, InventorySlot currentSlot)
        {
            return Usage.EquipToFree(inventory, currentSlot);
        }

        public virtual InventorySlot EquipToPosition(Inventory inventory, InventorySlot currentSlot, int slotId)
        {
            return Usage.EquipToPosition(inventory, currentSlot, slotId);
        }

        public virtual InventorySlot Unequip(Inventory inventory, InventorySlot currentSlot)
        {
            return Usage.Unequip(inventory, currentSlot);
        }

        public void RemoveFromInventory(Inventory inventory, int stacksToRemove)
        {
            Usage.RemoveFromInventory(inventory, this, stacksToRemove);
        }

        public void OnRest() 
        {
            Usage.OnRest(this);
        }

        /// <summary>
        /// Controls movement through the inventory.
        /// </summary>
        class ItemUsage
        {
            /// <summary>
            /// Puts the item to the inventory slot if the slot is not null.
            /// Returns slot.
            /// </summary>
            protected InventorySlot PutToSlot(InventorySlot slot, ItemState itemState)
            {
                if (slot == null)
                    return null;

                slot.Item = itemState;
                return slot;
            }

            public virtual InventorySlot AddToInventory(Inventory inventory, ItemState itemState)
            {
                var slot = inventory.AvailableSlot(inventory.PassiveSlots.Concat(inventory.ActiveSlots));
                return PutToSlot(slot, itemState);
            }

            /// <summary>
            /// Equips the item to a free slot corresponding to its usage.
            /// </summary>
            public virtual InventorySlot EquipToFree(Inventory inventory, InventorySlot currentSlot)
            {
                return inventory.MoveFromSlotToSlots(currentSlot, inventory.ActiveSlots);
            }

            /// <summary>
            /// Equips the item to slot correspoding to its usage and chosen position.
            /// </summary>
            public virtual InventorySlot EquipToPosition(Inventory inventory, InventorySlot currentSlot, int slotId)
            {
                return null;
            }

            public virtual InventorySlot Unequip(Inventory inventory, InventorySlot currentSlot)
            {
                return inventory.MoveFromSlotToSlots(currentSlot, inventory.PassiveSlots);
            }

            public virtual void RemoveFromInventory(Inventory inventory, ItemState itemState, int stacksToRemove)
            {
                inventory.RemoveItem(itemState);
            }

            /// <summary>
            /// Returns true if can be used.
            /// </summary>
            public virtual bool TryUse(Inventory inventory, ItemState itemState) => true;
            public virtual void OnRest(ItemState itemState) { }
        }

        class ConsumableItemUsage : ItemUsage
        {
            public ConsumableItemUsage(ItemState itemState)
            {
                itemState.IsStackable = false;
            }

            public override void OnRest(ItemState itemState)
            {
                // Does nothing
            }

            public override bool TryUse(Inventory inventory, ItemState itemState)
            {
                inventory.RemoveItem(itemState);
                return true;
            }
        }

        class StackableItemUsage : ItemUsage
        {
            bool CanBeUsed { get; }

            public StackableItemUsage(ItemState itemState, int stacksCount, bool canBeUsed)
            {
                itemState.IsStackable = true;
                itemState.StacksCount = stacksCount;
                CanBeUsed = canBeUsed;
            }

            public override InventorySlot AddToInventory(Inventory inventory, ItemState itemState)
            {
                var stackableSlot = inventory.SlotWithSameStackableItem(inventory.AllSlots, itemState.Name);
                if (stackableSlot != null)
                {
                    stackableSlot.Item.StacksCount += itemState.StacksCount;
                    return stackableSlot;
                }

                return base.AddToInventory(inventory, itemState);
            }

            public override void RemoveFromInventory(Inventory inventory, ItemState itemState, int stacksToRemove)
            {
                itemState.StacksCount -= stacksToRemove;
                if(itemState.StacksCount <= 0)
                {
                    inventory.RemoveItem(itemState);
                }
            }

            public override void OnRest(ItemState itemState)
            {
                // Does nothing
            }

            public override bool TryUse(Inventory inventory, ItemState itemState)
            {
                if (!CanBeUsed)
                    return false;

                itemState.StacksCount--;
                if (itemState.StacksCount <= 0)
                {
                    inventory.RemoveItem(itemState);
                }
                return true;
            }
        }

        class ReplenishableItemUsage : ItemUsage
        {
            int MaxStacks { get; set; }

            public ReplenishableItemUsage(ItemState itemState, int stacksCount)
            {
                itemState.IsStackable = true;
                itemState.StacksCount = stacksCount;
                MaxStacks = stacksCount;
            }

            public override InventorySlot AddToInventory(Inventory inventory, ItemState itemState)
            {
                var stackableSlot = inventory.SlotWithSameStackableItem(inventory.AllSlots, itemState.Name);
                if (stackableSlot != null)
                {
                    // Access Usage of other item to set its MaxStacks
                    // Usage has to be of correct type
                    var replenishableItemUsage = stackableSlot.Item.Usage as ReplenishableItemUsage;
                    if(replenishableItemUsage != null)
                    {
                        stackableSlot.Item.StacksCount += itemState.StacksCount;
                        replenishableItemUsage.MaxStacks += itemState.StacksCount;
                        return stackableSlot;
                    }
                }

                return base.AddToInventory(inventory, itemState);
            }

            public override void RemoveFromInventory(Inventory inventory, ItemState itemState, int stacksToRemove)
            {
                itemState.StacksCount -= stacksToRemove;
            }

            public override void OnRest(ItemState itemState)
            {
                itemState.StacksCount = MaxStacks;
            }

            public override bool TryUse(Inventory inventory, ItemState itemState)
            {
                if (itemState.StacksCount-- <= 0)
                {
                    return false;
                }
                return true;
            }
        }

        class WearableItemUsage : ItemUsage
        {
            SlotType SlotType { get; }

            public WearableItemUsage(ItemState itemState, SlotType slotType)
            {
                itemState.IsStackable = false;
                SlotType = slotType;
            }

            public override InventorySlot EquipToFree(Inventory inventory, InventorySlot currentSlot)
            {
                return inventory.MoveFromSlotToSlots(currentSlot, inventory.EquipmentSlots.Where(slot => slot.SlotType == SlotType));
            }

            public override InventorySlot EquipToPosition(Inventory inventory, InventorySlot currentSlot, int slotId)
            {
                var newSlot = inventory.EquipmentSlots.FirstOrDefault(slot => slot.SlotType == SlotType && slot.SlotId == slotId);
                if(newSlot == null)
                {
                    return null;
                }
                return inventory.ExchangeItems(currentSlot, newSlot);
            }

            public override void OnRest(ItemState itemState)
            {
                // Does nothing
            }

            public override bool TryUse(Inventory inventory, ItemState itemState)
            {
                return true;
            }
        }
    }

}
