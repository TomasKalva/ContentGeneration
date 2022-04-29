#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
using ShapeGrammar;
#endif
using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ContentGeneration.Assets.UI.Model
{
    public class ObjectState
    {
#if NOESIS
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
#endif

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
        /// <summary>
        /// D E P R E C A T E D, handle messaging manually
        /// </summary>
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

        private InteractOptions<InteractiveObjectState> _interactOptions;

        public InteractOptions<InteractiveObjectState> InteractOptions
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

#if NOESIS
        public virtual InteractiveObject MakeGeometry()
        {
            throw new InvalidOperationException("Geometry can be made only for generic variant of this class");
        }
#endif

        public InteractiveObjectState()
        {
            InteractOptions = new InteractOptions<InteractiveObjectState>();
            Name = "Name";
            MessageOnInteract = "";
            InteractionDescription = "";
        }
    }

#if NOESIS
    public class InteractiveObjectState<InteractiveObjectT> : InteractiveObjectState where InteractiveObjectT : InteractiveObject
    {
        InteractiveObjectT _intObjT;
        public InteractiveObjectT IntObj 
        { 
            get => _intObjT; 
            private set
            {
                _intObjT = value;
                InteractiveObject = value;
            } 
        }

        public delegate void InteractionDelegate(InteractiveObjectState<InteractiveObjectT> interactiveObject, PlayerCharacterState playerCharacter);

        InteractionDelegate ActionOnInteract { get; set; }

        public GeometryMaker<InteractiveObjectT> GeometryMaker { get; set; }

        public override InteractiveObject MakeGeometry()
        {
            IntObj = GeometryMaker.CreateGeometry();
            return IntObj;
        }

        public override void Interact(Agent agent)
        {
            if (ActionOnInteract != null)
            {
                // We assume that only the player can interact with this object
                ActionOnInteract(this, (PlayerCharacterState)agent.CharacterState);
            }
        }

        #region Api

        public InteractiveObjectState<InteractiveObjectT> Interact(InteractionDelegate onInteract)
        {
            MessageOnInteract = "";
            ActionOnInteract = onInteract;
            return this;
        }

        public InteractiveObjectState<InteractiveObjectT> Description(string description)
        {
            InteractionDescription = description;
            return this;
        }

        #endregion


    }
#endif

    public class InteractOptions<InteractiveObjectStateT> : INotifyPropertyChanged where InteractiveObjectStateT : InteractiveObjectState
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<InteractOption<InteractiveObjectStateT>> _options;

        public ObservableCollection<InteractOption<InteractiveObjectStateT>> Options
        {
            get { return _options; }
            set { _options = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public InteractOptions(params InteractOption<InteractiveObjectStateT>[] options)
        {
            Options = new ObservableCollection<InteractOption<InteractiveObjectStateT>>();
            options.ForEach(option => Options.Add(option));
        }

#if NOESIS
        public void DoOption(Agent agent, int optionIndex)
        {
            if(optionIndex >= 0 && optionIndex < Options.Count)
            {
                Options[optionIndex].Action(agent);
            }
        }

        public InteractOptions<InteractiveObjectStateT> AddOption(string description, Action<Agent> action)
        {
            int index = Options.Count + 1;
            Options.Add(new InteractOption<InteractiveObjectStateT>(description, action, index));
            return this;
        }
#endif
    }

    public class InteractOption<InteractiveObjectStateT> where InteractiveObjectStateT : InteractiveObjectState
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

    public class Interaction<InteractiveObjectStateT> where InteractiveObjectStateT : InteractiveObjectState
    {
        public string Message { get; }
        public InteractOptions<InteractiveObjectStateT> InteractOptions { get; }

        public Interaction(string message, InteractOptions<InteractiveObjectStateT> interactOptions)
        {
            Message = message;
            InteractOptions = interactOptions;
        }

        //public void Enter(InteractiveObject)
    }

    public class ComplexInteraction<InteractiveObjectStateT> where InteractiveObjectStateT : InteractiveObjectState
    {
        List<Interaction<InteractiveObjectStateT>> States { get; } 
        int CurrentState { get; set; }

        public ComplexInteraction<InteractiveObjectStateT> Say(string message)
        {
            States.Add(
                new Interaction<InteractiveObjectStateT>(
                    message,
                    new InteractOptions<InteractiveObjectStateT>()
                        .AddOption("Ok", _ => CurrentState++))
                );
            return this;
        }

        public ComplexInteraction<InteractiveObjectStateT> Decision(string message, params InteractOption<InteractiveObjectStateT>[] options)
        {
            States.Add(
                new Interaction<InteractiveObjectStateT>(
                    message,
                    new InteractOptions<InteractiveObjectStateT>(options))
                );
            return this;
        }
    }
}
