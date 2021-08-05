using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterRef : MonoBehaviour
{
    public abstract CharacterState CharacterState { get; }
}

public class CharacterReference<T> : CharacterRef where T : CharacterState
{
    [SerializeField]
    protected T characterState;

    public override CharacterState CharacterState => characterState;
}
