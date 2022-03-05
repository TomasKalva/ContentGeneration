#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.ObjectModel;

namespace ContentGeneration.Assets.UI.Model
{
    public class ObjectState
    {
        Transform _object;
        public Transform Object 
        { 
            get => _object;
            set
            {
                _object = value;
                AfterObjectSet();
            }
        }

        protected virtual void AfterObjectSet() { }
    }

    public class InteractiveObjectState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

#if NOESIS
        InteractiveObject _interactiveObject;
        public InteractiveObject InteractiveObject 
        { 
            get => _interactiveObject;
            set
            {
                _interactiveObject = value;
                if(_interactiveObject.State != this)
                {
                    _interactiveObject.State = this;
                }
            } 
        }
#endif

#if NOESIS
        [SerializeField]
#endif
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private string _messageOnInteract;
        public string MessageOnInteract
        {
            get { return _messageOnInteract; }
            set { _messageOnInteract = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        [SerializeField]
#endif
        private string _interactionDescription;

        public string InteractionDescription
        {
            get { return _interactionDescription; }
            set { _interactionDescription = value; PropertyChanged.OnPropertyChanged(this); }
        }

        private InteractOptions _interactOptions;

        public InteractOptions InteractOptions
        {
            get { return _interactOptions; }
            set { _interactOptions = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        public virtual void Interact(Agent agent)
        {
            Debug.Log("Interacted");
        }

        public void OptionalInteract(Agent agent, int optionIndex)
        {
            InteractOptions.DoOption(agent, optionIndex);
        }
#endif

        public InteractiveObjectState()
        {
            InteractOptions = new InteractOptions();
        }
    }

    public class InteractOptions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<InteractOption> _options;

        public ObservableCollection<InteractOption> Options
        {
            get { return _options; }
            set { _options = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public InteractOptions()
        {
            Options = new ObservableCollection<InteractOption>();
        }

#if NOESIS
        public void DoOption(Agent agent, int optionIndex)
        {
            if(optionIndex >= 0 && optionIndex < Options.Count)
            {
                Options[optionIndex].Action(agent);
            }
        }

        public InteractOptions AddOption(string description, Action<Agent> action)
        {
            int index = Options.Count + 1;
            Options.Add(new InteractOption(description, action, index));
            return this;
        }
#endif
    }

    public class InteractOption
    {
        public string Description { get; }
#if NOESIS
        public Action<Agent> Action { get; }
#endif
        public int Index { get; }

#if NOESIS
        public InteractOption(string description, Action<Agent> action, int index)
        {
            Description = description;
            Action = action;
            Index = index;
        }
#endif
    }
}
