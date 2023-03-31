using OurFramework.Gameplay.State;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
    /// <summary>
    /// Object that the player can interact with.
    /// </summary>
    public class InteractiveObject : MonoBehaviour
    {

        public InteractiveObjectState _state;
        public InteractiveObjectState State
        {
            get => _state;
            set
            {
                _state = value;
                if (_state.InteractiveObject != this)
                {
                    _state.InteractiveObject = this;
                }
            }
        }
    }
}
