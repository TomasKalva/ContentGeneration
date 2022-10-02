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
        Weapon,
        Head,
        Wrist,
        Skin
    }
    
    public class InventorySlot : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        CharacterState Owner { get; }

        ItemState _item;
        public ItemState Item
        {
            get => _item;
            set
            {
                if (SlotType != SlotType.Passive)
                {
                    if (_item != null)
                    {
                        _item.OnUnequipDelegate(Owner);
                    }
                    
                    if (value != null)
                    {
                        value.OnEquipDelegate(Owner);
                    }
                }

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

        public InventorySlot(CharacterState owner, SlotType slotType, int slotId)
        {
            SlotType = slotType;
            SlotId = slotId;
            Selected = false;
            this.Owner = owner;
        }
    }

    public interface IInventory
    {
        InventorySlot AddItem(ItemState item);
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

        ObservableCollection<InventorySlot> _equipmentSlots;
        public ObservableCollection<InventorySlot> EquipmentSlots
        {
            get => _equipmentSlots;
            private set
            {
                _equipmentSlots = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }


        public InventorySlot LeftWeapon => GetSlots(SlotType.Weapon).First(slot => slot.SlotId == 0);
        public InventorySlot RightWeapon => GetSlots(SlotType.Weapon).First(slot => slot.SlotId == 1);
        public InventorySlot Head => GetSlots(SlotType.Head).First();
        public InventorySlot LeftWrist => GetSlots(SlotType.Wrist).First(slot => slot.SlotId == 0);
        public InventorySlot RightWrist => GetSlots(SlotType.Wrist).First(slot => slot.SlotId == 1);
        public InventorySlot Skin => GetSlots(SlotType.Skin).First();

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
                PassiveSlots.Add(new InventorySlot(character, SlotType.Passive, i));
            }

            ActiveSlots = new ObservableCollection<InventorySlot>();
            for (int i = 0; i < 5; i++)
            {
                ActiveSlots.Add(new InventorySlot(character, SlotType.Active, i));
            }

            EquipmentSlots = new ObservableCollection<InventorySlot>() 
            { 
                new InventorySlot(character, SlotType.Weapon, 0),
                new InventorySlot(character, SlotType.Weapon, 1),
                new InventorySlot(character, SlotType.Head, 0),
                new InventorySlot(character, SlotType.Wrist, 0),
                new InventorySlot(character, SlotType.Wrist, 1),
                new InventorySlot(character, SlotType.Skin, 0),
            };

            AllSlots = EquipmentSlots.Concat(ActiveSlots).Concat(PassiveSlots).ToList();
        }

        public InventorySlot AvailableSlot(IEnumerable<InventorySlot> slots)
        {
            return slots.Where(slot => slot.Item == null).FirstOrDefault();
        }

        public InventorySlot SlotWithSameStackableItem(IEnumerable<InventorySlot> slots, string itemName)
        {
            return slots.Where(slot => slot.Item?.Name == itemName).FirstOrDefault();
        }

        public List<InventorySlot> AllSlots { get; }

        public IEnumerable<InventorySlot> GetSlots(SlotType slotType)
        {
            return AllSlots.Where(slot => slot.SlotType == slotType);
        }

        /// <summary>
        /// Adds item and returns slot the item is put to.
        /// </summary>
        public InventorySlot AddItem(ItemState item)
        {
            return item.AddToInventory(this);
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

        public bool TryPay(string itemName, int amount, Action success = null, Action fail = null)
        {

            if (!HasItems(itemName, amount, out var dew))
            {
                if(fail != null)
                {
                    fail();
                }
                return false;
            }
            
            RemoveStacksOfItems(dew, amount);
            if (success != null)
            {
                success();
            }
            return true;
        }
#endif

        public void RemoveItem(ItemState item)
        {
            var itemSlot = AllSlots.Where(slot => slot.Item == item);
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

        public InventorySlot MoveFromSlotToSlots(InventorySlot from, IEnumerable<InventorySlot> to)
        {
            var newSlot = AvailableSlot(to);
            if (from.Item == null || newSlot == null)
                return null;

            return ExchangeItems(from, newSlot);
        }

        public InventorySlot ExchangeItems(InventorySlot from, InventorySlot to)
        {
            var item = from.Item;
            from.Item = to.Item;
            to.Item = item;
            return to;
        }

        public void Update()
        {
            var activeItems = from slot in ActiveSlots.Concat(EquipmentSlots) where slot.Item != null select slot.Item;
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
            foundItems = AllSlots.SelectNN(slot => slot.Item).FirstOrDefault(item => item.Name == name);
            return foundItems != null && foundItems.StacksCount >= count;
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
        public void MoveCursor(int x, int _)
        {
            // todo: remove the _ argument
            if(!Active)
                return;

            var allSlots = AllSlots;
            var cursorPosition = allSlots.TakeWhile(x => x != CursorSlot).Count();
            var max = allSlots.Count();
            var newCursorPos = Mathf.Clamp(cursorPosition + x, 0, max - 1);
            CursorSlot = allSlots.ElementAt(newCursorPos);
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
        public EnemyInventory(CharacterState character) : base(character)
        {
        }

        public override void UseItem()
        {
            // Decide on which item to use and then use it
        }
    }
}
