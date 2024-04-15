using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameDemo
{
    /// <summary>
    /// Interaction logic for StartupControl.xaml
    /// </summary>
    public partial class StartupControl : UserControl
    {
        public delegate void CloseApp();
        public event CloseApp OnClose;
        public StartupControl()
        {
            InitializeComponent();
        }

        private void btnJoin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (OnClose != null)
            {
                OnClose();
            }
        }

        private void btnStartup_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = (MainWindow)Application.Current.MainWindow;
            main.LoadStartupPage();
        }

        private void PlayOnline_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = (MainWindow)Application.Current.MainWindow;
            main.LoadLoginPage();
        }

        private void PlayOffline_Click(object sender, RoutedEventArgs e)
        {
            MainWindow main = (MainWindow)Application.Current.MainWindow;

        }
    }
}
