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

        public SlotType SlotType { get; }
        public int SlotId { get; }

        public InventorySlot(SlotType slotType, int slotId)
        {
            SlotType = slotType;
            SlotId = slotId;
        }
    }

    public class Inventory : INotifyPropertyChanged
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

        public Inventory()
        {
            PassiveSlots = new ObservableCollection<InventorySlot>();
            ActiveSlots = new ObservableCollection<InventorySlot>();

            //var slot = new InventorySlot();

            //slot.Item = new ItemState();
            
            for(int i=0; i<15; i++)
            {
                PassiveSlots.Add(new InventorySlot(SlotType.PASSIVE, i));
            }
            /*PassiveSlots.Add(slot);
            PassiveSlots.Add(new InventorySlot());
            PassiveSlots.Add(new InventorySlot());
            PassiveSlots.Add(new InventorySlot());
            PassiveSlots.Add(new InventorySlot());
            PassiveSlots.Add(new InventorySlot());
            PassiveSlots.Add(new InventorySlot());
            PassiveSlots.Add(new InventorySlot());
            PassiveSlots.Add(new InventorySlot());*/

            for (int i = 0; i < 5; i++)
            {
                ActiveSlots.Add(new InventorySlot(SlotType.ACTIVE, i));
            }

            var myItem = new ItemState();

            AddItem(new ItemState());
            AddItem(myItem);
            AddItem(new ItemState());
            AddItem(new ItemState());

            EquipItem(PassiveSlots[1]);
            UnequipItem(PassiveSlots[1]);
        }

        InventorySlot AvailableSlot(IEnumerable<InventorySlot> slots)
        {
            return slots.Where(slot => slot.Item == null).FirstOrDefault();
        }

        bool AddItem(ItemState item)
        {
            var slot = AvailableSlot(PassiveSlots);
            if(slot != null)
            {
                slot.Item = item;
                return true;
            }
            else
            {
                return false;
            }
        }

        void EquipItem(InventorySlot itemSlot)
        {
            MoveFromSlotToSlots(itemSlot, PassiveSlots);
        }

        void UnequipItem(InventorySlot itemSlot)
        {
            MoveFromSlotToSlots(itemSlot, ActiveSlots);
        }

        void MoveFromSlotToSlots(InventorySlot from, IEnumerable<InventorySlot> to)
        {
            var newSlot = AvailableSlot(to);
            if (from.Item == null || newSlot == null)
                return;

            newSlot.Item = from.Item;
            from.Item = null;
        }
    }
}
