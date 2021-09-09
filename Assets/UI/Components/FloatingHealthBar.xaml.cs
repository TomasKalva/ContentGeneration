#if UNITY_5_3_OR_NEWER
#define NOESIS
using Noesis;
#else
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endif

namespace ContentGeneration.Assets.UI.Components
{
    /// <summary>
    /// Interaction logic for ProgressBarNum.xaml.
    /// 
    /// Currently doesn't register changes inside of FloatRange provided as data context.
    /// </summary>
    public partial class FloatingHealthBar : UserControl
    {
        public FloatingHealthBar()
        {
            InitializeComponent();
        }

#if NOESIS
        private void InitializeComponent()
        {
            Noesis.GUI.LoadComponent(this, "Assets/UI/Components/FloatingHealthBar.xaml");
        }
#endif
    }
}
