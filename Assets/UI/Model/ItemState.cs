#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using ContentGeneration.Assets.UI.Util;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;

namespace ContentGeneration.Assets.UI.Model
{
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

#if NOESIS
        [SerializeField]
#endif
        string _description;

        public string Description
        {
            get => _description;
            set { _description = value; PropertyChanged.OnPropertyChanged(this); }
        }

        int _stacksCount;
        public int StacksCount
        {
            get => _stacksCount;
            set { _stacksCount = value; PropertyChanged.OnPropertyChanged(this); }
        }

        bool _isStackable;
        public bool IsStackable
        {
            get => _isStackable;
            set { _isStackable = value; PropertyChanged.OnPropertyChanged(this); }
        }

        ItemUsage Usage { get; set; }

        public ItemState SetConsumable()
        {
            Usage = new ConsumableItemUsage(this);
            return this;
        }

        public ItemState SetStackable(int stacksCount)
        {
            Usage = new StackableItemUsage(this, stacksCount);
            return this;
        }

        public ItemState SetReplenishable(int stacksCount)
        {
            Usage = new ReplenishableItemUsage(this, stacksCount);
            return this;
        }

        public ItemState()
        {
            Name = "";
            Description = "";
            OnUseDelegate = _ => { };
            OnDropDelegate = _ => { };
            OnUpdateDelegate = _ => { };
            Usage = new ItemUsage();
        }

        public delegate void CharacterAction(CharacterState state);

        public CharacterAction OnUseDelegate { get; protected set; }
        public virtual ItemState OnUse(CharacterAction characterAction)
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

        public CharacterAction OnDropDelegate { get; protected set; }
        public ItemState OnDrop(CharacterAction characterAction)
        {
            OnDropDelegate = characterAction;
            return this;
            //Debug.Log($"Dropping {Name}");
        }

        public CharacterAction OnUpdateDelegate { get; protected set; }
        public ItemState OnUpdate(CharacterAction characterAction)
        {
            OnUpdateDelegate = characterAction;
            return this;
            //Debug.Log($"Updating {Name}");
        }

        public InventorySlot AddToInventory(Inventory inventory, IEnumerable<InventorySlot> slots)
        {
            return Usage.AddToInventory(inventory, slots, this);
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
            public virtual InventorySlot AddToInventory(Inventory inventory, IEnumerable<InventorySlot> slots, ItemState itemState)
            {
                var slot = inventory.AvailableSlot(slots);
                if (slot != null)
                {
                    slot.Item = itemState;
                    return slot;
                }
                else
                {
                    return null;
                }
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

            public override InventorySlot AddToInventory(Inventory inventory, IEnumerable<InventorySlot> slots, ItemState itemState)
            {
                return base.AddToInventory(inventory, slots, itemState);
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
            public StackableItemUsage(ItemState itemState, int stacksCount)
            {
                itemState.IsStackable = true;
                itemState.StacksCount = stacksCount;
            }

            public override InventorySlot AddToInventory(Inventory inventory, IEnumerable<InventorySlot> slots, ItemState itemState)
            {
                var stackableSlot = inventory.SlotWithSameStackableItem(slots, itemState.Name);
                if (stackableSlot != null)
                {
                    stackableSlot.Item.StacksCount = itemState.StacksCount;
                    return stackableSlot;
                }

                return base.AddToInventory(inventory, slots, itemState);
            }

            public override void OnRest(ItemState itemState)
            {
                // Does nothing
            }

            public override bool TryUse(Inventory inventory, ItemState itemState)
            {
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

            public override InventorySlot AddToInventory(Inventory inventory, IEnumerable<InventorySlot> slots, ItemState itemState)
            {
                var stackableSlot = inventory.SlotWithSameStackableItem(slots, itemState.Name);
                if (stackableSlot != null)
                {
                    stackableSlot.Item.StacksCount += itemState.StacksCount;
                    MaxStacks += itemState.StacksCount;
                    return stackableSlot;
                }

                return base.AddToInventory(inventory, slots, itemState);
            }

            public override void OnRest(ItemState itemState)
            {
                itemState.StacksCount = MaxStacks;
            }

            public override bool TryUse(Inventory inventory, ItemState itemState)
            {
                itemState.StacksCount = Mathf.Max(0, itemState.StacksCount - 1);
                if (itemState.StacksCount < 0)
                {
                    return false;
                }
                return true;
            }
        }
    }

}
