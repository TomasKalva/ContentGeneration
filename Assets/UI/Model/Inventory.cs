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
namespace ContentGeneration.Assets.UI.Model
{
    public enum SlotType
    {
        ACTIVE,
        PASSIVE
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
        InventorySlot AddItem(ItemState item);

        void Update();
    }

    public class PlayerInventory : INotifyPropertyChanged, IInventory
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        InventorySlot _selectedSlot;
        public InventorySlot SelectedSlot
        {
            get => _selectedSlot;
            private set
            {

                if (_selectedSlot != null)
                    _selectedSlot.Selected = false;

                _selectedSlot = null;
                PropertyChanged.OnPropertyChanged(this);

                _selectedSlot = value;
                if (_selectedSlot != null)
                    _selectedSlot.Selected = true;
                PropertyChanged.OnPropertyChanged(this);
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
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        CharacterState character;

        public PlayerInventory(CharacterState character)
        {
            this.character = character;

            PassiveSlots = new ObservableCollection<InventorySlot>();
            for(int i=0; i<15; i++)
            {
                PassiveSlots.Add(new InventorySlot(SlotType.PASSIVE, i));
            }

            ActiveSlots = new ObservableCollection<InventorySlot>();
            for (int i = 0; i < 5; i++)
            {
                ActiveSlots.Add(new InventorySlot(SlotType.ACTIVE, i));
            }

#if !NOESIS
            AddItem(new ItemState());
            AddItem(new ItemState());
            AddItem(new ItemState());
            AddItem(new ItemState());

            EquipItem(PassiveSlots[1]);
            UnequipItem(PassiveSlots[1]);
#endif

            CursorSlot = PassiveSlots[0];
            SelectedSlot = ActiveSlots[0];

        }

        InventorySlot AvailableSlot(IEnumerable<InventorySlot> slots)
        {
            return slots.Where(slot => slot.Item == null).FirstOrDefault();
        }

        /// <summary>
        /// Returns slot the item is put to.
        /// </summary>
        public InventorySlot AddItem(ItemState item)
        {
            var slot = AvailableSlot(PassiveSlots);
            if(slot != null)
            {
                slot.Item = item;
                return slot;
            }
            else
            {
                return null;
            }
        }

        public void EquipItem(InventorySlot itemSlot)
        {
            var item = itemSlot.Item;
            if (MoveFromSlotToSlots(itemSlot, ActiveSlots))
            {
                item?.OnEquip(character);
            }
        }

        public void UnequipItem(InventorySlot itemSlot)
        {
            var item = itemSlot.Item;
            if (MoveFromSlotToSlots(itemSlot, PassiveSlots))
            {
                item?.OnUnequip(character);
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

#if NOESIS

        /// <summary>
        /// Moves SelectedSlot across active and passive items.
        /// </summary>
        public void MoveCursor(int x, int y)
        {
            if(!Active)
                return;

            if (CursorSlot.SlotType == SlotType.PASSIVE)
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

            if(CursorSlot.SlotType == SlotType.PASSIVE)
            {
                EquipItem(CursorSlot);
            }
            else
            {
                UnequipItem(CursorSlot);
            }
        }

        public void UseItem()
        {
            if (Active)
                return;

            SelectedSlot.Item?.OnUse(character);
        }

        public void DropItem()
        {
            if (!Active)
                return;

            CursorSlot.Item?.OnDrop(character);
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

        public void Update()
        {
            var activeItems = from slot in ActiveSlots where slot.Item != null select slot.Item;
            foreach (var item in activeItems)
            {
                item.OnUpdate(character);
            }
        }
    }

    public class EnemyInventory : INotifyPropertyChanged, IInventory
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        InventorySlot EmptySlot => ActiveSlots.Where(slot => slot.Item == null).FirstOrDefault();

        CharacterState character;

        public EnemyInventory(CharacterState character)
        {
            this.character = character;
            ActiveSlots = new ObservableCollection<InventorySlot>();
            for(int i=0; i<5; i++)
            {
                ActiveSlots.Add(new InventorySlot(SlotType.ACTIVE, i));
            }
        }

        public InventorySlot AddItem(ItemState item)
        {
            var slot = EmptySlot;
            if(slot != null)
            {
                slot.Item = item;
                item.OnEquip(character);
                return slot;
            }
            else
            {
                return null;
            }
        }

        public void Update()
        {
            var activeItems = from slot in ActiveSlots where slot.Item != null select slot.Item;
            foreach (var item in activeItems)
            {
                item.OnUpdate(character);
            }
        }
    }
}
