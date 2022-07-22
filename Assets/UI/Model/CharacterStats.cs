#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
using UnityEditor;
#endif
using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;

namespace ContentGeneration.Assets.UI.Model
{
    public class CharacterStats : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private CharacterState _character;
        public CharacterState Character
        {
            get { return _character; }
            set 
            { 
                _character = value;

                // Update character with the properties
                Update();

                PropertyChanged.OnPropertyChanged(this); 
            }
        }

        private int _will;
        public int Will
        {
            get { return _will; }
            set 
            { 
                _will = value;
                if(Character != null)
                {
                    Character.Health.Maximum = 50 + 10 * _will;
                    Debug.Log($"Max health:{Character.Health.Maximum}");
                }
                PropertyChanged.OnPropertyChanged(this); 
            }
        }

        private int _strength;
        public int Strength
        {
            get { return _strength; }
            set
            {
                _strength = value;
                if (Character != null)
                {
                }
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        private int _endurance;
        public int Endurance
        {
            get { return _endurance; }
            set
            {
                _endurance = value;
                if (Character != null)
                {
                    Character.Stamina.Maximum = 10 + _endurance;
                }
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        private int _agility;
        public int Agility
        {
            get { return _agility; }
            set
            {
                _agility = value;
                if (Character != null)
                {
                    var agent = Character.Agent;
                    if(agent != null)
                    {
                        agent.acting.SetActingSpeedMultiplier(1f + 0.0025f * _agility);
                        //var walk = agent.acting.GetAct("Walk") as Move;
                        //walk.SetSpeedMultiplier(1f + 0.01f * _agility);
                        var run = agent.acting.GetAct("Run") as Move;
                        run.SetSpeedMultiplier(1f + 0.01f * _agility);
                    }
                }
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        private int _posture;
        public int Posture
        {
            get { return _posture; }
            set 
            { 
                _posture = value;
                if (Character != null)
                {
                    Character.Poise.Maximum = 20f + 6f * _posture;
                    Character.PhysicalDefense.ReductionPercentage = 0.5f * _posture;
                }
                PropertyChanged.OnPropertyChanged(this); 
            }
        }

        private int _resistances;
        public int Resistances
        {
            get { return _resistances; }
            set
            {
                _resistances = value;
                if (Character != null)
                {
                    Character.FireDefense.ReductionPercentage = 0.5f * _resistances;
                    Character.DarkDefense.ReductionPercentage = 0.5f * _resistances;
                    Character.DivineDefense.ReductionPercentage = 0.5f * _resistances;
                }
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        private int _versatility;
        public int Versatility
        {
            get { return _versatility; }
            set
            {
                _versatility = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        public CharacterStats(CharacterState character)
        {
            Character = character;
        }

        public void Update()
        {
            Will = Will;
            Strength = Strength;
            Endurance = Endurance;
            Agility = Agility;
            Posture = Posture;
            Resistances = Resistances;
            Versatility = Versatility;
        }

        public static IEnumerable<StatIncrease> StatIncreases()
        {
            return new[] {
                new StatIncrease("Will", ch => ch.Stats.Will++),
                new StatIncrease("Strength", ch => ch.Stats.Strength++),
                new StatIncrease("Endurance", ch => ch.Stats.Endurance++),
                new StatIncrease("Agility", ch => ch.Stats.Agility++),
                new StatIncrease("Posture", ch => ch.Stats.Posture++),
                new StatIncrease("Resistances", ch => ch.Stats.Resistances++),
                new StatIncrease("Versatility", ch => ch.Stats.Versatility++),
            };
        }
    }

    public struct StatIncrease
    {
        public string Stat { get; }
        public Action<CharacterState> Increase { get; }

        public StatIncrease(string stat, Action<CharacterState> increase)
        {
            Stat = stat;
            Increase = increase;
        }
    }
}
