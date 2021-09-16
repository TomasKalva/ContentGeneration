using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterReference : CharacterReference<CharacterState>
{
    void Awake()
    {
        characterState = new CharacterState();
        characterState.viewCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        characterState.Agent = GetComponent<Agent>();
    }

    void Update()
    {
        var camera = characterState.viewCamera;

        var agentUiPos = camera.WorldToScreenPoint(transform.position + characterState.Agent.UIOffset * Vector3.up);
        characterState.UIScreenPosX = agentUiPos.x;
        characterState.UIScreenPosY = agentUiPos.y;

        var agentCenterPos = camera.WorldToScreenPoint(transform.position + characterState.Agent.CenterOffset * Vector3.up);
        characterState.ScreenPosX = agentCenterPos.x;
        characterState.ScreenPosY = agentCenterPos.y;

        characterState.VisibleOnCamera = ExtensionMethods.IsPointInDirection(camera.transform.position, camera.transform.forward, characterState.Agent.transform.position) &&
                                        (camera.transform.position - characterState.Agent.transform.position).magnitude < 25f;
    }
}
