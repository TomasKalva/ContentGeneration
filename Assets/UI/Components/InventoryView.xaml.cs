#if UNITY_5_3_OR_NEWER
#define NOESIS
using Noesis;
#else
using ContentGeneration.Assets.UI.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#endif

namespace ContentGeneration.Assets.UI.Components
{
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
        }

#if NOESIS
        private void InitializeComponent()
        {
            Noesis.GUI.LoadComponent(this, "Assets/UI/Components/InventoryView.xaml");
        }
#endif
    }
}
