#if UNITY_5_3_OR_NEWER
#define NOESIS
using Noesis;
using OurFramework.Game;
using UnityEngine;
#else
using System;
using System.Windows;
using System.Windows.Controls;

#endif

namespace OurFramework.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        public MainWindow()
        {
            InitializeComponent();
#if NOESIS
            DataContext = GameObject.Find("Main Camera").GetComponent<ViewModel>();
#else
            DataContext = new ViewModel();
#endif
        }

#if NOESIS
        private void InitializeComponent()
        {
            Noesis.GUI.LoadComponent(this, "Assets/MainWindow.xaml");
        }
#endif
    }
}
