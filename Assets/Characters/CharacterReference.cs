using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterReference : CharacterReference<CharacterState>
{
    void Start()
    {
        characterState.viewCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        characterState.agent = GetComponent<Agent>();
    }

    void Update()
    {
        var agentUiPos = characterState.viewCamera.WorldToScreenPoint(transform.position + Vector3.up);
        characterState.UIScreenPosX = agentUiPos.x;
        characterState.UIScreenPosY = agentUiPos.y;

        var agentCenterPos = characterState.viewCamera.WorldToScreenPoint(transform.position);
        characterState.ScreenPosX = agentCenterPos.x;
        characterState.ScreenPosY = agentCenterPos.y;

    }
}