#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEditor;
#endif
using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using OurFramework.Gameplay.RealWorld;

namespace ContentGeneration.Assets.UI.Model
{
    public class CharacterStats : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        const int MAX_STAT_VALUE = 99;
        int ValidStatValue(int val) => Math.Max(0, Math.Min(MAX_STAT_VALUE, val));

        private CharacterState _character;
        public CharacterState Character
        {
            get { return _character; }
            set 
            { 
                _character = value;

                if(_character.Stats != this)
                {
                    _character.Stats = this;
                }

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
                _will = ValidStatValue(value);
                if(Character != null)
                {
                    Character.Health.Maximum = 40 + 10 * _will;
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
                _strength = ValidStatValue(value);
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
                _endurance = ValidStatValue(value);
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
                _agility = ValidStatValue(value);
                if (Character != null)
                {
                    var agent = Character.Agent;
                    if(agent != null)
                    {
                        agent.acting.SetActingSpeedMultiplier(1f + 0.0025f * _agility);
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
                _posture = ValidStatValue(value);
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
                _resistances = ValidStatValue(value);
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
                _versatility = ValidStatValue(value);
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        public CharacterStats()
        {
            Will = 0;
            Strength = 0;
            Endurance = 0;
            Agility = 0;
            Posture = 0;
            Resistances = 0;
            Versatility = 0;
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

        static CharacterStats()
        {
            StatChanges = new Dictionary<Stat, StatManipulation<Action<CharacterStats, int>>>() {
                {Stat.Will, new StatManipulation<Action<CharacterStats, int>>(Stat.Will, (ch, val) => ch.Will += val) },
                {Stat.Strength, new StatManipulation<Action<CharacterStats, int>>(Stat.Strength, (ch, val) => ch.Strength += val) },
                {Stat.Endurance, new StatManipulation<Action<CharacterStats, int>>(Stat.Endurance, (ch, val) => ch.Endurance += val) },
                {Stat.Agility, new StatManipulation<Action<CharacterStats, int>>(Stat.Agility, (ch, val) => ch.Agility += val) },
                {Stat.Posture, new StatManipulation<Action<CharacterStats, int>>(Stat.Posture, (ch, val) => ch.Posture += val) },
                {Stat.Resistances, new StatManipulation<Action<CharacterStats, int>>(Stat.Resistances, (ch, val) => ch.Resistances += val) },
                {Stat.Versatility, new StatManipulation<Action<CharacterStats, int>>(Stat.Versatility, (ch, val) => ch.Versatility += val) },
            };
            StatIncreases = StatChanges.Values.Select(statChange => 
                new StatManipulation<Action<CharacterStats>>(statChange.Stat, stats => statChange.Manipulate(stats, 1)))
                .ToArray();
        }

        public static StatManipulation<Action<CharacterStats>>[] StatIncreases { get; }

        public static Dictionary<Stat, StatManipulation<Action<CharacterStats, int>>> StatChanges { get; }
    }

    public enum Stat
    {
        Will,
        Strength,
        Endurance,
        Agility,
        Posture,
        Resistances,
        Versatility,
    }

    public struct StatManipulation<ManipulationT>
    {
        public Stat Stat { get; }
        public ManipulationT Manipulate { get; }

        public StatManipulation(Stat stat, ManipulationT manipulation)
        {
            Stat = stat;
            Manipulate = manipulation;
        }
    }
}
