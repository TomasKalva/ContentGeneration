#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using ContentGeneration.Assets.UI.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace ContentGeneration.Assets.UI.Model
{
    public enum SlotType
    {
        Active,
        Passive,
        LeftWeapon,
        RightWeapon,
    }
    static class SlotTypeExtensions
    {
        public static bool IsWeapon(this SlotType slotType)
        {
            return slotType == SlotType.LeftWeapon || slotType == SlotType.RightWeapon;
        }
    }

    public class InventorySlot : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ItemState _item;
        public ItemState Item
        {
            get => _item;
            set
            {
                _item = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        bool _cursor;
        public bool Cursor
        {
            get => _cursor;
            set
            {
                _cursor = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        public SlotType SlotType { get; }
        public int SlotId { get; }

        public InventorySlot(SlotType slotType, int slotId)
        {
            SlotType = slotType;
            SlotId = slotId;
            Selected = false;
        }
    }

    public interface IInventory
    {
        InventorySlot AddItem(IEnumerable<InventorySlot> slots, ItemState item);
        void UseItem();

        void Update();
    }

    public class Inventory : INotifyPropertyChanged, IInventory
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// To be able to trigger property change from subclasses.
        /// </summary>
        protected void OnPropertyChanged(INotifyPropertyChanged thisInstance, [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(thisInstance, new PropertyChangedEventArgs(name));
        }

        ObservableCollection<InventorySlot> _passiveSlots;
        public ObservableCollection<InventorySlot> PassiveSlots
        {
            get => _passiveSlots;
            private set
            {
                _passiveSlots = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        ObservableCollection<InventorySlot> _activeSlots;
        public ObservableCollection<InventorySlot> ActiveSlots
        {
            get => _activeSlots;
            private set
            {
                _activeSlots = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        InventorySlot _leftWeaponSlot;
        public InventorySlot LeftWeaponSlot
        {
            get => _leftWeaponSlot;
            private set
            {
                _leftWeaponSlot = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        InventorySlot _rightWeaponSlot;
        public InventorySlot RightWeaponSlot
        {
            get => _rightWeaponSlot;
            private set
            {
                _rightWeaponSlot = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        InventorySlot _selectedSlot;
        public InventorySlot SelectedSlot
        {
            get => _selectedSlot;
            protected set
            {

                if (_selectedSlot != null)
                    _selectedSlot.Selected = false;

                _selectedSlot = null;
                OnPropertyChanged(this);

                _selectedSlot = value;
                if (_selectedSlot != null)
                    _selectedSlot.Selected = true;
                OnPropertyChanged(this);
            }
        }

        protected CharacterState character;

        public Inventory(CharacterState character)
        {
            this.character = character;

            PassiveSlots = new ObservableCollection<InventorySlot>();
            for (int i = 0; i < 15; i++)
            {
                PassiveSlots.Add(new InventorySlot(SlotType.Passive, i));
            }

            ActiveSlots = new ObservableCollection<InventorySlot>();
            for (int i = 0; i < 5; i++)
            {
                ActiveSlots.Add(new InventorySlot(SlotType.Active, i));
            }

            LeftWeaponSlot = new InventorySlot(SlotType.LeftWeapon, 0);
            RightWeaponSlot = new InventorySlot(SlotType.RightWeapon, 0);
        }

        public InventorySlot AvailableSlot(IEnumerable<InventorySlot> slots)
        {
            return slots.Where(slot => slot.Item == null).FirstOrDefault();
        }

        public InventorySlot SlotWithSameStackableItem(IEnumerable<InventorySlot> slots, string itemName)
        {
            return slots.Where(slot => slot.Item?.Name == itemName).FirstOrDefault();
        }

        public IEnumerable<InventorySlot> AllSlots()
        {
            return PassiveSlots.Concat(
                        ActiveSlots.Concat(
                            new InventorySlot[2] { LeftWeaponSlot, RightWeaponSlot }));
        }

        public IEnumerable<InventorySlot> GetSlots(SlotType slotType)
        {
            return AllSlots().Where(slot => slot.SlotType == slotType);
        }

        /// <summary>
        /// Adds item and returns slot the item is put to.
        /// </summary>
        public InventorySlot AddItem(IEnumerable<InventorySlot> slots, ItemState item)
        {
            return item.AddToInventory(this, slots);
        }

        public InventorySlot EquipWeapon(SlotType slotType, ItemState weapon)
        {
            if(slotType == SlotType.RightWeapon)
            {
                RightWeaponSlot.Item = weapon;
                return RightWeaponSlot;
            }
            else if (slotType == SlotType.LeftWeapon)
            {
                LeftWeaponSlot.Item = weapon;
                return LeftWeaponSlot;
            }
            else
            {
#if NOESIS
                Debug.LogError($"{slotType} should be a weapon slot!");
#endif
                return null;
            }
        }

        public void EquipItem(InventorySlot itemSlot)
        {
            var item = itemSlot.Item;
            MoveFromSlotToSlots(itemSlot, ActiveSlots);
        }

        public void UnequipItem(InventorySlot itemSlot)
        {
            var item = itemSlot.Item;
            MoveFromSlotToSlots(itemSlot, PassiveSlots);
        }

#if NOESIS
        public void RemoveItems(IEnumerable<ItemState> items)
        {
            items.ForEach(item => RemoveItem(item));
        }

        public void RemoveStacksOfItems(ItemState items, int stacksToRemove)
        {
            items.RemoveFromInventory(this, stacksToRemove);
        }
#endif

        public void RemoveItem(ItemState item)
        {
            var itemSlot = AllSlots().Where(slot => slot.Item == item);
            if (itemSlot.Any())
            {
                itemSlot.First().Item = null;
            }
            else
            {
#if NOESIS
                Debug.LogError("Item doesn't exist");
#endif
            }
        }

        bool MoveFromSlotToSlots(InventorySlot from, IEnumerable<InventorySlot> to)
        {
            var newSlot = AvailableSlot(to);
            if (from.Item == null || newSlot == null)
                return false;

            newSlot.Item = from.Item;
            from.Item = null;
            return true;
        }

        public void Update()
        {
            var activeItems = from slot in ActiveSlots where slot.Item != null select slot.Item;
            foreach (var item in activeItems)
            {
                item.OnUpdateDelegate(character);
            }
        }
#if NOESIS
        /// <summary>
        /// Assumes that items are stackable.
        /// </summary>
        public bool HasItems(string name, int count, out ItemState foundItems)
        {
            foundItems = AllSlots().SelectNN(slot => slot.Item).FirstOrDefault(item => item.Name == name);
            return foundItems != null && foundItems.StacksCount >= count;
            /*if (foundItems != null && foundItems.StacksCount >= count)
            {
                //foundItems = foundItems.Take(count);
                return true;
            }
            else
            {
                foundItems = null;
                return false;
            }*/
        }
        
#endif
        public virtual void UseItem() {}
    }

    public class PlayerInventory : Inventory
    {
        InventorySlot _cursorSlot;
        public InventorySlot CursorSlot
        {
            get => _cursorSlot;
            private set
            {
                if(_cursorSlot != null)
                    _cursorSlot.Cursor = false;
                _cursorSlot = value;
                if (_cursorSlot != null)
                    _cursorSlot.Cursor = true;
                OnPropertyChanged(this);
            }
        }

        int ColumnsCount = 5;

        bool _active;
        public bool Active
        {
            get => _active;
            set
            {
                _active = value;
                OnPropertyChanged(this);
            }
        }

        public PlayerInventory(CharacterState character) : base(character)
        {

#if !NOESIS
            /*AddItem(SlotType.Passive, new ItemState());
            AddItem(SlotType.Passive, new ItemState());
            AddItem(SlotType.Passive, new ItemState());
            AddItem(SlotType.Passive, new ItemState());

            EquipItem(PassiveSlots[1]);
            UnequipItem(PassiveSlots[1]);*/
#endif
            
            CursorSlot = PassiveSlots[0];
            SelectedSlot = ActiveSlots[0];

        }

#if NOESIS

        /// <summary>
        /// Moves SelectedSlot across active and passive items.
        /// </summary>
        public void MoveCursor(int x, int y)
        {
            if(!Active)
                return;

            if (CursorSlot.SlotType == SlotType.Passive)
            {
                int newId = CursorSlot.SlotId + x + y * ColumnsCount;
                if(newId >= 0)
                {
                    newId = Mathf.Clamp(newId, 0, PassiveSlots.Count - 1);
                    CursorSlot = PassiveSlots[newId];
                }
                else
                {
                    newId = Mathf.Clamp(newId + ColumnsCount, 0, ActiveSlots.Count - 1);
                    CursorSlot = ActiveSlots[newId];
                }
            }
            else
            {

                int newId = CursorSlot.SlotId + x + y * ColumnsCount;
                if (newId < ColumnsCount)
                {
                    newId = Mathf.Clamp(newId, 0, ActiveSlots.Count - 1);
                    CursorSlot = ActiveSlots[newId];
                }
                else
                {
                    newId = Mathf.Clamp(newId - ColumnsCount, 0, PassiveSlots.Count - 1);
                    CursorSlot = PassiveSlots[newId];
                }
            }
        }

        public void HandleClick()
        {
            if(!Active)
                return;

            if(CursorSlot.SlotType == SlotType.Passive)
            {
                EquipItem(CursorSlot);
            }
            else
            {
                UnequipItem(CursorSlot);
            }
        }

        public override void UseItem()
        {
            if (Active)
                return;

            SelectedSlot.Item?.OnUseDelegate(character);
        }

        public void DropItem()
        {
            if (!Active)
                return;

            CursorSlot.Item?.OnDropDelegate(character);
            CursorSlot.Item = null;
        }

        public void ChangeSelected(bool right)
        {
            if (Active)
                return;

            int count = 0;
            int pos = SelectedSlot.SlotId;
            while(count < ActiveSlots.Count)
            {
                pos = (pos + (right ? 1 : -1) + ActiveSlots.Count) % ActiveSlots.Count;
                if(ActiveSlots[pos].Item != null)
                {
                    break;
                }
                count++;
            }

            SelectedSlot = ActiveSlots[pos];
        }
#endif
    }

    public class EnemyInventory : Inventory
    {
        InventorySlot EmptySlot => ActiveSlots.Where(slot => slot.Item == null).FirstOrDefault();

        public EnemyInventory(CharacterState character) : base(character)
        {
        }

        public override void UseItem()
        {
            // Decide on which item to use and then use it
        }
    }
}
