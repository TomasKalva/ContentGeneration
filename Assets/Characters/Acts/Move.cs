using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : AnimatedAct
{
    [SerializeField]
    float speed = 3f;

    Vector2 direction;

    public Vector2 Direction{
        get => direction;
        set => direction = value;
    }

    public bool SetDirection { get; set; } = true;

    public override bool UpdateAct(Agent agent)
    {
        PlayIfNotActive(agent, 0.1f);

        agent.movement.Move(direction, speed, SetDirection);
        return true;
    }
}
