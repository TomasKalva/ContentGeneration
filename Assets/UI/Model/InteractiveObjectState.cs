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

        protected void OnPropertyChanged(INotifyPropertyChanged thisInstance, [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(thisInstance, new PropertyChangedEventArgs(name));
        }

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


#if NOESIS
        public virtual void Interact(Agent agent)
        {
            Debug.Log("Interacted");
        }

        public virtual void OptionalInteract(Agent agent, int optionIndex)
        {
            //InteractOptions.DoOption(agent, this, optionIndex);
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
            Name = "Name";
            MessageOnInteract = "";
            InteractionDescription = "";
        }
    }

    public delegate void InteractionDelegate<InteractiveObjectT>(InteractiveObjectState<InteractiveObjectT> interactiveObject, PlayerCharacterState playerCharacter)
        where InteractiveObjectT : InteractiveObject;

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

        InteractionDelegate<InteractiveObjectT> ActionOnInteract { get; set; }

        private InteractOptions<InteractiveObjectT> _interactOptions;

        public InteractOptions<InteractiveObjectT> InteractOptions
        {
            get { return _interactOptions; }
            set { _interactOptions = value; OnPropertyChanged(this); }
        }

        Interaction<InteractiveObjectT> _interaction;
        public Interaction<InteractiveObjectT> Interaction 
        { 
            get => _interaction; 
            set
            {
                _interaction = value;
                _interaction.Enter(this);
            } 
        }

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
        public override void OptionalInteract(Agent agent, int optionIndex)
        {
            InteractOptions?.DoOption(agent, this, optionIndex);
        }

        #region Api

        public InteractiveObjectState<InteractiveObjectT> SetInteraction(InteractionSequence<InteractiveObjectT> interaction)
        {
            Interaction = interaction;
            return this;
        }

        public InteractiveObjectState<InteractiveObjectT> Interact(InteractionDelegate<InteractiveObjectT> onInteract)
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

    public class InteractOptions<InteractiveObjectT> : INotifyPropertyChanged where InteractiveObjectT : InteractiveObject
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<InteractOption<InteractiveObjectT>> _options;

        public ObservableCollection<InteractOption<InteractiveObjectT>> Options
        {
            get { return _options; }
            set { _options = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public InteractOptions(params InteractOption<InteractiveObjectT>[] options)
        {
            Options = new ObservableCollection<InteractOption<InteractiveObjectT>>();
            options.ForEach(option =>
            {
                Options.Add(option);
                option.Index = Options.Count;
            });
        }

#if NOESIS
        public void DoOption(Agent agent, InteractiveObjectState<InteractiveObjectT> interactiveObjectState, int optionIndex)
        {
            if(optionIndex >= 0 && optionIndex < Options.Count)
            {
                Options[optionIndex].Action(interactiveObjectState, (PlayerCharacterState)agent.CharacterState);
            }
        }

        public InteractOptions<InteractiveObjectT> AddOption(string description, InteractionDelegate<InteractiveObjectT> action)
        {
            int index = Options.Count + 1;
            Options.Add(new InteractOption<InteractiveObjectT>(description, action, index));
            return this;
        }
#endif
    }

    public class InteractOption<InteractiveObjectT> where InteractiveObjectT : InteractiveObject
    {
        public string Description { get; }
#if NOESIS
        public InteractionDelegate<InteractiveObjectT> Action { get; }
#endif
        public int Index { get; set; }

#if NOESIS
        public InteractOption(string description, InteractionDelegate<InteractiveObjectT> action, int index = 0)
        {
            Description = description;
            Action = action;
            Index = index;
        }
#endif
    }

    /// <summary>
    /// Expression that defines how InteractiveObjects act
    /// </summary>
    public abstract class Interaction<InteractiveObjectT> where InteractiveObjectT : InteractiveObject
    {
        public abstract void Enter(InteractiveObjectState<InteractiveObjectT> ios);
    }

    public class InteractionWithOptions<InteractiveObjectT> : Interaction<InteractiveObjectT> where InteractiveObjectT : InteractiveObject
    {
        public string Message { get; }
        public InteractOptions<InteractiveObjectT> InteractOptions { get; }

        public InteractionWithOptions(string message, InteractOptions<InteractiveObjectT> interactOptions)
        {
            Message = message;
            InteractOptions = interactOptions;
        }

        public override void Enter(InteractiveObjectState<InteractiveObjectT> ios)
        {
            ios.InteractOptions = InteractOptions;
            ios.InteractionDescription = Message;
        }
    }

    public class InteractionSequence<InteractiveObjectT> : Interaction<InteractiveObjectT> where InteractiveObjectT : InteractiveObject
    {
        List<InteractionWithOptions<InteractiveObjectT>> States { get; }
        int CurrentState { get; set; }

        public InteractionSequence()
        {
            States = new List<InteractionWithOptions<InteractiveObjectT>>();
            CurrentState = 0;
        }

        public InteractionSequence<InteractiveObjectT> Say(string message)
        {
            States.Add(
                new InteractionWithOptions<InteractiveObjectT>(
                    message,
                    new InteractOptions<InteractiveObjectT>()
                        .AddOption("<nod>", (ios, _1) =>
                        {
                            CurrentState = Math.Min(CurrentState + 1, States.Count - 1);
                            var nextState = States[CurrentState];
                            nextState.Enter(ios);
                        }))
                );
            return this;
        }

        public InteractionSequence<InteractiveObjectT> Decision(string message, params InteractOption<InteractiveObjectT>[] options)
        {
            States.Add(
                new InteractionWithOptions<InteractiveObjectT>(
                    message,
                    new InteractOptions<InteractiveObjectT>(options))
                );
            return this;
        }

        public override void Enter(InteractiveObjectState<InteractiveObjectT> ios)
        {
            States[CurrentState].Enter(ios);
        }
    }
}
