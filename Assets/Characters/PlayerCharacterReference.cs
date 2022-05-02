using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterReference : CharacterReference<PlayerCharacterState>
{
    void Start()
    {
        //characterState.viewCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        //characterState.Agent = GetComponent<Agent>();
    }

    void Update()
    {
        //var agentUiPos = characterState.viewCamera.WorldToScreenPoint(transform.position + Vector3.up);
        //characterState.UIScreenPosX = agentUiPos.x;
        //characterState.UIScreenPosY = agentUiPos.y;
    }
}
