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
    public partial class ProgressBarNum : UserControl
    {
        public ProgressBarNum()
        {
            InitializeComponent();
        }

#if NOESIS
        private void InitializeComponent()
        {
            Noesis.GUI.LoadComponent(this, "Assets/UI/Components/ProgressBarNum.xaml");
        }
#endif

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public bool NoNumbers
        {
            get { return (bool)GetValue(NoNumbersProperty); }
            set { SetValue(NoNumbersProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(ProgressBarNum), new UIPropertyMetadata());

        public static readonly DependencyProperty NoNumbersProperty =
            DependencyProperty.Register("NoNumbers", typeof(bool), typeof(ProgressBarNum), new UIPropertyMetadata(true));
    }
}
