using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;

namespace ContentGeneration.Assets.UI.Model
{
    public class CharacterState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FloatRange _health;
        public FloatRange Health
        {
            get { return _health; }
            set { _health = value; PropertyChanged.OnPropertyChanged(this); }
        }

        private FloatRange _stamina;
        public FloatRange Stamina
        {
            get { return _stamina; }
            set { _stamina = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public CharacterState()
        {
            Health = new FloatRange(100, 42);
            Stamina = new FloatRange(100, 42);
        }
    }
}
