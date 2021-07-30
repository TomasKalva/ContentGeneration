#if UNITY_5_3_OR_NEWER
#define NOESIS
using UnityEngine;
#endif
using ContentGeneration.Assets.UI.Util;
using System.ComponentModel;

namespace ContentGeneration.Assets.UI.Model
{
    public class CharacterState :
#if NOESIS
        MonoBehaviour,
#endif
        INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private FloatRange _health;
        public FloatRange Health
        {
            get { return _health; }
            set { _health = value; PropertyChanged.OnPropertyChanged(this); }
        }

        private FloatRange _stamina;
        public FloatRange Stamina
        {
            get { return _stamina; }
            set { _stamina = value; PropertyChanged.OnPropertyChanged(this); }
        }

        public CharacterState()
        {
            Health = new FloatRange(100, 42);
            Stamina = new FloatRange(100, 42);
        }

 #region Screen position of health bars

#if NOESIS
        private float _screenPosX;
        public float ScreenPosX
        {
            get { return _screenPosX; }
            set { _screenPosX = value; PropertyChanged.OnPropertyChanged(this); }
        }

        private float _screenPosY;
        public float ScreenPosY
        {
            get { return _screenPosY; }
            set { _screenPosY = viewCamera.scaledPixelHeight - value; PropertyChanged.OnPropertyChanged(this); }
        }

        private Camera viewCamera;

        void Start(){
            viewCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        }

        void Update()
        {
            var agentUiPos = viewCamera.WorldToScreenPoint(transform.position + Vector3.up);
            ScreenPosX = agentUiPos.x;
            ScreenPosY = agentUiPos.y;
        }
#else
        public float ScreenPosX => 0f;

        public float ScreenPosY => 0f;
#endif

        #endregion
    }
}
