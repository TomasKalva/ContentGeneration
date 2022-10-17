using System;
using System.Collections.Generic;

namespace ContentGeneration.Assets.UI.Model
{
    /// <summary>
    /// Expression that defines how InteractiveObject acts
    /// </summary>
    public abstract class Interaction<InteractiveObjectT> where InteractiveObjectT : InteractiveObject
    {
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
            /*
            ios.InteractOptions = InteractOptions;
            ios.InteractionDescription = Message;
            */
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
            InteractionDescription = interactionDescription;
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
            /*ios.InteractOptions = null;
            ios.InteractionDescription = InteractionDescription;
            ios.ActionOnInteract = ActionOnInteract;*/
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

        public void TryMoveNext(InteractiveObjectState<InteractiveObjectT> ios)
        {
            CurrentState = Math.Min(CurrentState + 1, States.Count - 1);
            States[CurrentState].Enter(ios);
        }

        public void Reset(InteractiveObjectState<InteractiveObjectT> ios)
        {
            if (States.Count == 0)
                return;

            CurrentState = 0;
            States[CurrentState].Enter(ios);
        }

        public InteractionSequence<InteractiveObjectT> Say(string message)
        {
            States.Add(
                new InteractionWithOptions<InteractiveObjectT>(
                    message,
                    new InteractOptions<InteractiveObjectT>()
                        .AddOption("<nod>", (ios, _1) =>
                        {
                            TryMoveNext(ios);
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

        public InteractionSequence<InteractiveObjectT> Act(string interactionDescription, InteractionDelegate<InteractiveObjectT> actionOnInteract)
        {
            States.Add(
                new FastInteraction<InteractiveObjectT>(
                    interactionDescription,
                    actionOnInteract
                    )
                );
            return this;
        }

        public override void Enter(InteractiveObjectState<InteractiveObjectT> ios)
        {
            if (States.Count == 0)
                return;

            States[CurrentState].Enter(ios);
        }
    }
}
