using Assets.Characters.SpellClasses;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : AnimatedAct
{
    /// <summary>
    /// Creates the shot.
    /// </summary>
    public Effect ShotEffect { set; private get; }

    /// <summary>
    /// Time when the shot happens. Normalized to [0,1].
    /// </summary>
    [SerializeField]
    float shootT;

    MovementConstraint lockOnTarget;

    Direction3F directionToTargetF;

    public override void OnStart(Agent agent)
    {
        PlayAnimation(agent);

        directionToTargetF = () => TargetPosition.DirectionTo(agent.transform.position).XZ().X0Z().normalized;

        lockOnTarget = new TurnToDirection(() => directionToTargetF().XZ().normalized);

        SetupMovementConstraints(agent, lockOnTarget);

        agent.State = AgentState.PREPARE;
    }

    public override void OnUpdate(Agent agent)
    {
        var normalizedElapsed = timeElapsed / duration;
        if (agent.State == AgentState.PREPARE && normalizedElapsed >= shootT)
        {
            lockOnTarget.Finished = true;
            DoShot(agent);
            agent.State = AgentState.RESTORE;
        }
    }

    public override void EndAct(Agent agent)
    {
        agent.State = AgentState.NORMAL;
        MovementContraints.ForEach(con => con.Finished = true);
    }

    void DoShot(Agent agent)
    {
        if (ShotEffect != null)
        {
            ShotEffect(agent.CharacterState);
        }
    }
}