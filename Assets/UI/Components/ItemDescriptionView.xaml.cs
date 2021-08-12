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
    public partial class ItemDescriptionView : UserControl
    {
        public ItemDescriptionView()
        {
            InitializeComponent();
        }

#if NOESIS
        private void InitializeComponent()
        {
            Noesis.GUI.LoadComponent(this, "Assets/UI/Components/ItemDescriptionView.xaml");
        }
#endif
    }
}
