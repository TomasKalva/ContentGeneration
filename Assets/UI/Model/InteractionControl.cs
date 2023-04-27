#if UNITY_5_3_OR_NEWER
#define NOESIS
#endif
using OurFramework.Gameplay.RealWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OurFramework.Gameplay.State
{
    /// <summary>
    /// Expression that defines how InteractiveObject acts
    /// </summary>
    public abstract class Interaction<InteractiveObjectT> where InteractiveObjectT : InteractiveObject
    {
        /// <summary>
        /// Sets the current interaction state to InteractiveObjectState.
        /// </summary>
        public struct InteractionArguments
        {
            public InteractOptions<InteractiveObjectT> InteractOptions { get; }
            public string InteractionDescription { get; }
            public InteractionDelegate<InteractiveObjectT> ActionOnInteract { get; }

            public InteractionArguments(InteractOptions<InteractiveObjectT> interactOptions, string interactionDescription, InteractionDelegate<InteractiveObjectT> actionOnInteract)
            {
                InteractOptions = interactOptions;
                InteractionDescription = interactionDescription;
                ActionOnInteract = actionOnInteract;
            }
        }

        public abstract void Enter(InteractiveObjectState<InteractiveObjectT> ios);
    }

    /// <summary>
    /// Decide between multiple options
    /// </summary>
    public class InteractionWithOptions<InteractiveObjectT> : Interaction<InteractiveObjectT> where InteractiveObjectT : InteractiveObject
    {
        string Message { get; }
        InteractOptions<InteractiveObjectT> InteractOptions { get; }

        public InteractionWithOptions(string message, InteractOptions<InteractiveObjectT> interactOptions)
        {
            Message = message;
            InteractOptions = interactOptions;
        }

        public override void Enter(InteractiveObjectState<InteractiveObjectT> ios)
        {
            ios.Configure(
                new InteractionArguments(
                        InteractOptions,
                        Message,
                        (_1, _2) => { }
                    )
                );
        }
    }

    /// <summary>
    /// Press E to interact
    /// </summary>
    public class FastInteraction<InteractiveObjectT> : Interaction<InteractiveObjectT> where InteractiveObjectT : InteractiveObject
    {
        string InteractionDescription { get; }
        InteractionDelegate<InteractiveObjectT> ActionOnInteract { get; set; }

        public FastInteraction(string interactionDescription, InteractionDelegate<InteractiveObjectT> actionOnInteract)
        {
            InteractionDescription = $"[E]: {interactionDescription}";
            ActionOnInteract = actionOnInteract;
        }

        public override void Enter(InteractiveObjectState<InteractiveObjectT> ios)
        {
            ios.Configure(
                new InteractionArguments(
                        null,
                        InteractionDescription,
                        ActionOnInteract
                    )
                );
        }
    }

    /// <summary>
    /// Follow a sequence of interactions
    /// </summary>
    public class InteractionSequence<InteractiveObjectT> : Interaction<InteractiveObjectT> where InteractiveObjectT : InteractiveObject
    {
        List<Interaction<InteractiveObjectT>> States { get; }
        int CurrentState { get; set; }

        public InteractionSequence()
        {
            States = new List<Interaction<InteractiveObjectT>>();
            CurrentState = 0;
        }

        /// <summary>
        /// Move to the next interaction if it exists.
        /// </summary>
        public void TryMoveNext(InteractiveObjectState<InteractiveObjectT> ios)
        {
            CurrentState = Math.Min(CurrentState + 1, States.Count - 1);
            States[CurrentState].Enter(ios);
        }

        /// <summary>
        /// Go back to first interaction state.
        /// </summary>
        public void Reset(InteractiveObjectState<InteractiveObjectT> ios)
        {
            if (States.Count == 0)
                return;

            CurrentState = 0;
            States[CurrentState].Enter(ios);
        }

        /// <summary>
        /// The interactive object says something.
        /// </summary>
        public InteractionSequence<InteractiveObjectT> Say(string message)
        {
            States.Add(
                new InteractionWithOptions<InteractiveObjectT>(
                    message,
                    new InteractOptions<InteractiveObjectT>()
                        .AddOption("ok", (ios, _1) =>
                        {
                            TryMoveNext(ios);
                        })
                        )
                );
            return this;
        }

        /// <summary>
        /// Decide between multiple options.
        /// </summary>
        public InteractionSequence<InteractiveObjectT> Decide(string message, params Func<InteractOption<InteractiveObjectT>, InteractOption<InteractiveObjectT>>[] optionCreators)
        {
            Decide(message, optionCreators.Select(oc => oc(new InteractOption<InteractiveObjectT>())).ToArray());
            return this;
        }

        /// <summary>
        /// Decide between multiple options.
        /// </summary>
        public InteractionSequence<InteractiveObjectT> Decide(string message, params InteractOption<InteractiveObjectT>[] options)
        {
            States.Add(
                new InteractionWithOptions<InteractiveObjectT>(
                    message,
                    new InteractOptions<InteractiveObjectT>(options))
                );
            return this;
        }

        /// <summary>
        /// Interactive object does something on interaction.
        /// </summary>
        public InteractionSequence<InteractiveObjectT> Interact(string interactionDescription, InteractionDelegate<InteractiveObjectT> actionOnInteract)
        {
            States.Add(
                new FastInteraction<InteractiveObjectT>(
                    interactionDescription,
                    actionOnInteract
                    )
                );
            return this;
        }

        /// <summary>
        /// Enter the state.
        /// </summary>
        public override void Enter(InteractiveObjectState<InteractiveObjectT> ios)
        {
            if (States.Count == 0)
                return;

            States[CurrentState].Enter(ios);
        }
    }
}
