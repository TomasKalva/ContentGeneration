using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AgentState
{
    PREPARE,
    DAMAGE,
    RESTORE,
    NORMAL
}

public class Attack : AnimatedAct
{
    [SerializeField, Curve(0f, 0f, 1f, 15f, true)]
    AnimationCurve speedF;

    Vector3 direction;
    public Vector3 Direction
    {
        get => direction;
        set => direction = value.normalized;
    }

    [SerializeField]
    WeaponSlot[] weaponSlots;

    /// <summary>
    /// Normalized to [0,1].
    /// </summary>
    [SerializeField]
    float damageStartT;

    /// <summary>
    /// Normalized to [0,1].
    /// </summary>
    [SerializeField]
    float damageEndT;

    public override void OnStart(Agent agent)
    {
        PlayAnimation(agent);

        agent.movement.VelocityUpdater = new CurveVelocityUpdater(speedF, duration, Direction);

        movementContraints = new List<MovementConstraint>()
        {
            new VelocityInDirection(Direction),
        };

        movementContraints.ForEach(con => agent.movement.Constraints.Add(con));

        agent.State = AgentState.PREPARE;
    }

    void SetSlotsActive(bool active)
    {
        foreach (var weaponSlot in weaponSlots)
        {
            weaponSlot.Weapon.Active = active;
        }
    }

    public override void OnUpdate(Agent agent)
    {
        var normalizedElapsed = timeElapsed / duration;
        if(agent.State == AgentState.PREPARE && normalizedElapsed >= damageStartT)
        {
            agent.State = AgentState.DAMAGE;
            SetSlotsActive(true);
        }

        if (agent.State == AgentState.DAMAGE && normalizedElapsed >= damageEndT)
        {
            agent.State = AgentState.RESTORE;
            SetSlotsActive(false);
        }
    }

    public override void EndAct(Agent agent)
    {
        SetSlotsActive(false);
        agent.State = AgentState.NORMAL;
        movementContraints.ForEach(con => con.Finished = true);
    }
}