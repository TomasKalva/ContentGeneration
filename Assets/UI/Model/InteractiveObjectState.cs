#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using OurFramework.UI.Util;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.ObjectModel;
using OurFramework.LevelDesignLanguage;
using OurFramework.Gameplay.RealWorld;
using OurFramework.UI;
using OurFramework.Util;
using OurFramework.Game;

namespace OurFramework.Gameplay.State
{
    /// <summary>
    /// State of interactive object.
    /// </summary>
    public abstract class InteractiveObjectState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(INotifyPropertyChanged thisInstance, [CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(thisInstance, new PropertyChangedEventArgs(name));
        }

        public World World { get; set; }

        /// <summary>
        /// Does the object block path.
        /// </summary>
        public bool IsBlocking { get; set; }

        public Vector3 Position { get; set; }

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

        protected ICreatingStrategy CreatingStrategy { get; set; }
        public bool CanBeCreated() => CreatingStrategy.TryCreate();

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
        private string _interactionDescription;

        public string InteractionDescription
        {
            get { return _interactionDescription; }
            protected set { _interactionDescription = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        public virtual void Interact(Agent agent)
        {
            Debug.Log("Interacted");
        }

        public virtual void OptionalInteract(Agent agent, int optionIndex)
        {

        }

        public virtual void PlayerLeft()
        {
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
            InteractionDescription = "";
            CreatingStrategy = new CreateAlways();
            IsBlocking = false;
        }
    }

    public delegate void InteractionDelegate<InteractiveObjectT>(InteractiveObjectState<InteractiveObjectT> interactiveObject, PlayerCharacterState playerCharacter)
#if NOESIS
        where InteractiveObjectT : InteractiveObject
#else
    where InteractiveObjectT : class
#endif
        ;

    public class InteractiveObjectState<InteractiveObjectT> : InteractiveObjectState
#if NOESIS
    where InteractiveObjectT : InteractiveObject
#else
    where InteractiveObjectT : class
#endif
    {
        InteractiveObjectT _intObjT;
        public InteractiveObjectT IntObj 
        { 
            get => _intObjT; 
            protected set
            {
                _intObjT = value;
#if NOESIS
                InteractiveObject = value;
#endif
            }
        }

        public InteractiveObjectState<InteractiveObjectT> SetCreatingStrategy(ICreatingStrategy creatingStrategy)
        {
            CreatingStrategy = creatingStrategy;
            return this;
        }

        public InteractiveObjectState<InteractiveObjectT> SetBlocking(bool isBlocking)
        {
            IsBlocking = isBlocking;
            return this;
        }

        protected InteractionDelegate<InteractiveObjectT> ActionOnInteract { get; set; }

        private InteractOptions<InteractiveObjectT> _interactOptions;
        public InteractOptions<InteractiveObjectT> InteractOptions
        {
            get { return _interactOptions; }
            protected set { _interactOptions = value; OnPropertyChanged(this); }
        }

#if NOESIS
        InteractionSequence<InteractiveObjectT> _interaction;
        public InteractionSequence<InteractiveObjectT> Interaction 
        { 
            get => _interaction; 
            set
            {
                _interaction = value;
                _interaction.Enter(this);
            } 
        }

        public GeometryMaker<InteractiveObjectT> GeometryMaker { get; set; }

        public void Configure(Interaction<InteractiveObjectT>.InteractionArguments arguments)
        {
            InteractOptions = arguments.InteractOptions;
            InteractionDescription = arguments.InteractionDescription;
            ActionOnInteract = arguments.ActionOnInteract;
        }

        public override InteractiveObject MakeGeometry()
        {
            IntObj = GeometryMaker();
            InteractiveObject = IntObj;
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

        public override void PlayerLeft()
        {
            Interaction?.Reset(this);
        }
#endif

#region Api

#if NOESIS
        public InteractiveObjectState<InteractiveObjectT> SetInteraction(Func<InteractionSequence<InteractiveObjectT>, InteractionSequence<InteractiveObjectT>> interactionF)
        {
            Interaction = interactionF(new InteractionSequence<InteractiveObjectT>());
            return this;
        }
#endif

        public InteractiveObjectState<InteractiveObjectT> Interact(InteractionDelegate<InteractiveObjectT> onInteract)
        {
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

    /// <summary>
    /// Options from which the player can choose on interaction.
    /// </summary>
    public class InteractOptions<InteractiveObjectT> : INotifyPropertyChanged
#if NOESIS
    where InteractiveObjectT : InteractiveObject
#else
    where InteractiveObjectT : class
#endif
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ObservableCollection<InteractOption<InteractiveObjectT>> _options;

        public ObservableCollection<InteractOption<InteractiveObjectT>> Options
        {
            get { return _options; }
            set { _options = value; PropertyChanged.OnPropertyChanged(this); }
        }

#if NOESIS
        public InteractOptions(params InteractOption<InteractiveObjectT>[] options)
        {
            Options = new ObservableCollection<InteractOption<InteractiveObjectT>>();
            options.ForEach(option =>
            {
                Options.Add(option);
                option.Index = Options.Count;
            });
        }

        public void DoOption(Agent agent, InteractiveObjectState<InteractiveObjectT> interactiveObjectState, int optionIndex)
        {
            if(optionIndex >= 0 && optionIndex < Options.Count)
            {
                Options[optionIndex].Action(interactiveObjectState, (PlayerCharacterState)agent.CharacterState);
            }
        }
#endif

        public InteractOptions<InteractiveObjectT> AddOption(string description, InteractionDelegate<InteractiveObjectT> action)
        {
            int index = Options.Count + 1;
            Options.Add(new InteractOption<InteractiveObjectT>(description, action, index));
            return this;
        }
    }

    /// <summary>
    /// Option the player can choose on interaction.
    /// </summary>
    public class InteractOption<InteractiveObjectT>
#if NOESIS
    where InteractiveObjectT : InteractiveObject
#else
    where InteractiveObjectT : class
#endif
    {
        public string Description { get; private set; }
        public InteractionDelegate<InteractiveObjectT> Action { get; private set; }
        public int Index { get; set; }

        public InteractOption(string description = "", InteractionDelegate<InteractiveObjectT> action = null, int index = 0)
        {
            Description = description;
            Action = action ?? ((_1, _2) => { });
            Index = index;
        }

        public InteractOption<InteractiveObjectT> SetDescription(string description) 
        {
            Description = description;
            return this;
        }

        public InteractOption<InteractiveObjectT> SetAction(InteractionDelegate<InteractiveObjectT> action)
        {
            Action = action;
            return this;
        }
    }
}
