using UnityEngine;


public class SculptureAgent : Agent
{
    [SerializeField]
    public ColliderDetector leftWideDetector;

    [SerializeField]
    public ColliderDetector rightWideDownDetector;

    [SerializeField]
    public ColliderDetector doubleSwipeLeftDetector;

    [SerializeField]
    public ColliderDetector doubleSwipeRightDetector;

    [SerializeField]
    public ColliderDetector overheadDetector;

    [SerializeField]
    public ColliderDetector groundSlamDetector;

    public Act OverheadAttack()
    {
        var attack = acting.SelectAct("Overhead") as Attack;
        return attack;
    }

    public Act WideAttack()
    {
        var currentAct = acting.ActiveAct;

        // do a combo if slash is currently active
        Attack attack;
        if (currentAct && currentAct.actName == "LeftWide")
        {
            attack = acting.SelectAct("RightWideDown") as Attack;
        }
        else
        {
            attack = acting.SelectAct("LeftWide") as Attack;
        }
        attack.Direction = movement.AgentForward;
        return attack;
    }

    public Act DoubleSwipe()
    {
        var attack = acting.SelectAct("DoubleSwipe") as Attack;
        return attack;
    }

    public Act GroundSlam()
    {
        var attack = acting.SelectAct("GroundSlam") as Attack;
        return attack;
    }
}