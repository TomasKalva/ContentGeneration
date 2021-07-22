#if UNITY_5_3_OR_NEWER
#define NOESIS
using Noesis;
#else
using System;
using System.Windows;
using System.Windows.Controls;

#endif

namespace ContentGeneration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NewComponent : UserControl
    {
        public NewComponent()
        {
            InitializeComponent();

            DataContext = new ViewModel();
        }

#if NOESIS
        private void InitializeComponent()
        {
            Noesis.GUI.LoadComponent(this, "Assets/NewComponent.xaml");
        }
#endif
    }
}
