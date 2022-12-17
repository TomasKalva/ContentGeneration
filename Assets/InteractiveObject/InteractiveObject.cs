using ContentGeneration.Assets.UI.Model;
using UnityEngine;

namespace OurFramework.Gameplay.RealWorld
{
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
