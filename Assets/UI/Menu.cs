#if UNITY_5_3_OR_NEWER
#define NOESIS
#endif
using System.ComponentModel;
using ContentGeneration.Assets.UI.Util;
#if NOESIS
using UnityEngine;
using UnityEngine.SceneManagement;
using static ShapeGrammar.Game;
#endif

namespace ContentGeneration.Assets.UI
{
    public class Menu : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        bool _visible;
        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                PropertyChanged.OnPropertyChanged(this);
            }
        }

        public Menu()
        {
            Visible = false;
        }

#if NOESIS
        public void Update(GameControl gameControl)
        {
            if (Visible)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Visible = false;
                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    gameControl.EndRun();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Visible = true;
                }
            }
        }
#endif
    }
}
