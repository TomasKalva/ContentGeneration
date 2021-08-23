#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
using UnityEditor;
#endif
using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;

namespace ContentGeneration.Assets.UI.Model
{
#if NOESIS
    [Serializable]
#endif
    public class CharacterProperties : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

#if NOESIS
        [SerializeField]
#endif
        private CharacterState _character;
        public CharacterState Character
        {
            get { return _character; }
            set 
            { 
                _character = value;

                // Update character with the properties
                Health = Health;
                Will = Will;
                Poise = Poise;
                Strength = Strength;
                Dexterity = Dexterity;

                PropertyChanged.OnPropertyChanged(this); 
            }
        }

#if NOESIS
        [SerializeField]
#endif
        private float _health;
        public float Health
        {
            get { return _health; }
            set 
            { 
                _health = value;
                if(Character != null)
                {
                    Character.Health.Maximum = _health;
                }
                PropertyChanged.OnPropertyChanged(this); 
            }
        }

#if NOESIS
        [SerializeField]
#endif
        private float _will;
        public float Will
        {
            get { return _will; }
            set
            {
                _will = value;
                if (Character != null)
                {
                    Character.Will.Maximum = _will;
                }
                PropertyChanged.OnPropertyChanged(this);
            }
        }

#if NOESIS
        [SerializeField]
#endif
        private float _poise;
        public float Poise
        {
            get { return _poise; }
            set 
            { 
                _poise = value;
                if (Character != null)
                {
                    Character.Stability.Maximum = _poise;
                }
                PropertyChanged.OnPropertyChanged(this); 
            }
        }

#if NOESIS
        [SerializeField]
#endif
        private float _strength;
        public float Strength
        {
            get { return _strength; }
            set 
            { 
                _strength = value;
                if (Character != null)
                {

                }
                PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private float _dexterity;
        public float Dexterity
        {
            get { return _dexterity; }
            set
            { 
                _dexterity = value;
                if (Character != null)
                {

                }
                PropertyChanged.OnPropertyChanged(this); 
            }
        }
    }
}
