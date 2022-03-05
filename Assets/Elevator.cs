using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Elevator : MonoBehaviour
{
    [SerializeField]
    Transform elevatorPlatform;

    public float Max_height { get; set; }
    float _height;
    float Height 
    {
        get => _height; 
        set
        {
            _height = value;
            elevatorPlatform.localPosition = new Vector3(0f, _height, 0f);
        } 
    }
    bool movingUp;
    float movementSpeed = 2f;

    bool IsUp => Height == Max_height;
    bool IsDown => Height == 0f;
    bool isMoving = false;

    public void Move()
    {
        Debug.Log("Elevator moving");
        if (IsUp)
        {
            movingUp = false;
            isMoving = true;
        }

        if (IsDown)
        {
            movingUp = true;
            isMoving = true;
        }
    }

    private void Update()
    {
        if (isMoving)
        {
            var speed = movingUp ? movementSpeed : -movementSpeed;
            Height = Mathf.Clamp(Height + speed * Time.deltaTime, 0f, Max_height);

            if(IsUp || IsDown)
            {
                isMoving = false;
            }
        }
    }

    public void SetIsUp(bool isUp)
    {
        Height = isUp ? Max_height : 0f;
    }
}
