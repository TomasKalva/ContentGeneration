#if UNITY_5_3_OR_NEWER
#define NOESIS
using Noesis;
using UnityEngine;
#else
using System;
using System.Windows;
using System.Windows.Controls;

#endif

namespace ContentGeneration.Assets.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        public MainWindow()
        {
            InitializeComponent();
            #if UNITY_5_3_OR_NEWER
            Debug.Log("MainWindowInitialized");
            #endif
            DataContext = new ViewModel();
        }

        private void ProgressBarNum_Loaded(object sender, RoutedEventArgs e)
        {

        }

#if NOESIS
        private void InitializeComponent()
        {
            Noesis.GUI.LoadComponent(this, "Assets/MainWindow.xaml");
        }
#endif
    }
}
