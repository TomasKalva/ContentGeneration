using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : AnimatedAct
{
    [SerializeField]
    Projectile projectile;

    [SerializeField]
    Transform gun;

    [SerializeField]
    float speed;

    /// <summary>
    /// Normalized to [0,1].
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
        movementContraints = new List<MovementConstraint>()
        {
            lockOnTarget,
        };

        movementContraints.ForEach(con => agent.movement.Constraints.Add(con));

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
        movementContraints.ForEach(con => con.Finished = true);
    }

    void DoShot(Agent agent)
    {
        var direction = directionToTargetF();
        var bullet = Instantiate(projectile);
        bullet.transform.position = gun.position + gun.forward * 0.5f;
        bullet.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        bullet.GetComponent<Rigidbody>().velocity = direction * speed;
        Debug.Log(bullet.GetComponent<Rigidbody>().velocity);
        bullet.Active = true;
        bullet.Owner = agent;
    }
}