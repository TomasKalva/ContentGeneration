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
    /// Interaction logic for ProgressBarNum.xaml
    /// </summary>
    public partial class ProgressBarNum : UserControl
    {
        public ProgressBarNum()
        {
            InitializeComponent();
        }

#if NOESIS
        private void InitializeComponent()
        {
            Noesis.GUI.LoadComponent(this, "Assets/BlueComponent.xaml");
        }
#endif

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(ProgressBarNum), new UIPropertyMetadata());
    }
}
