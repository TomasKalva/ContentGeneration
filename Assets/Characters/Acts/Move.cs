using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : AnimatedAct
{
    [SerializeField]
    protected float speed = 3f;

    [SerializeField]
    protected float animMetersPerSecond = 1f;

    protected Vector2 direction;

    public Vector2 Direction 
    {
        get => direction.normalized;
        set => direction = value;
    }

    public bool SetDirection { get; set; } = true;

    private void Awake()
    {
        Duration = animMetersPerSecond / speed;
    }

    public override bool UpdateAct(Agent agent, float dt)
    {
        PlayIfNotActive(agent, 0.1f);

        agent.movement.Move(Direction, speed, SetDirection);
        return true;
    }
}
