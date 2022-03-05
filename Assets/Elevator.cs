using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Elevator : MonoBehaviour
{
    [SerializeField]
    Rigidbody elevatorPlatform;

    [SerializeField]
    ColliderDetector onPlatformDetector;

    public float Max_height { get; set; }
    float Height 
    {
        get => elevatorPlatform.transform.localPosition.y; 
        /*set
        {
            _height = value;
            elevatorPlatform.localPosition = new Vector3(0f, _height, 0f);
        } */
    }
    //bool movingUp;
    float movementSpeed = 1f;

    bool IsUp => Height >= Max_height;
    bool IsDown => Height <= 0f;
    int timeSpentMoving = 0;

    public void Move()
    {
        Debug.Log("Elevator moving");
        if (IsUp)
        {
            //movingUp = false;
            elevatorPlatform.velocity = movementSpeed * Vector3.down;
            timeSpentMoving = 1;
        }

        if (IsDown)
        {
            //movingUp = true;
            elevatorPlatform.velocity = movementSpeed * Vector3.up;
            timeSpentMoving = 1;
        }
    }

    private void FixedUpdate()
    {
        if (timeSpentMoving > 2)
        {

            /*
            var speed = movingUp ? movementSpeed : -movementSpeed;
            var positionChange = speed * Time.fixedDeltaTime;
            Height = Mathf.Clamp(Height + positionChange, 0f, Max_height);

            if (onPlatformDetector.Triggered)
            {
                Debug.Log($"Moving {onPlatformDetector.other.name} on elevator");
                onPlatformDetector.other.transform.position += 0.5f * positionChange * Vector3.up;
            }*/

            if(IsUp || IsDown)
            {
                timeSpentMoving = 0;
                //elevatorPlatform.velocity = Vector3.zero;
            }
        }

        timeSpentMoving += timeSpentMoving > 0 ? 1 : 0;
    }

    public void SetIsUp(bool isUp)
    {
        elevatorPlatform.transform.localPosition = isUp ? Max_height * Vector3.up : Vector3.zero;
    }
}
