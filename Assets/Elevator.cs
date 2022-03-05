using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Elevator : MonoBehaviour
{
    [SerializeField]
    AnimancerComponent animancer;

    [SerializeField]
    AnimationClip up;

    [SerializeField]
    AnimationClip down;

    [SerializeField]
    ClipTransition moveUp;

    [SerializeField]
    ClipTransition moveDown;

    [SerializeField]
    Transform shaft;

    [SerializeField]
    Transform movingPlatform;

    float _maxHeight;
    public float MaxHeight 
    { 
        get => _maxHeight;
        set
        {
            _maxHeight = value;
            var relPlatformSpeed = platformSpeed / transform.localScale.y;
            var scale = 1f / MaxHeight;
            moveUp.Speed = scale * relPlatformSpeed;
            moveDown.Speed = scale * relPlatformSpeed;
            movingPlatform.localScale = new Vector3(1f, scale, 1f);
            shaft.localScale = new Vector3(1f, MaxHeight, 1f);
        } 
    }

    float platformSpeed = 1f;


    bool IsUp { get; set; }
    bool IsDown => !IsUp && !IsMoving;
    bool IsMoving { get; set; }

    public void Move()
    {
        Debug.Log("Elevator moving");
        if (IsUp)
        {
            IsMoving = true;
            animancer.Play(moveDown).Events.OnEnd += () =>
            {
                IsUp = false;
                IsMoving = false;
                animancer.Play(down);
            };
        }

        if (IsDown)
        {
            IsMoving = true;
            animancer.Play(moveUp).Events.OnEnd += () =>
            {
                IsUp = true;
                IsMoving = false;
                animancer.Play(up);
            };
        }
    }

    public void SetIsUp(bool isUp)
    {
        animancer.Play(isUp ? up : down);
        IsUp = isUp;
        IsMoving = false;
    }
}
