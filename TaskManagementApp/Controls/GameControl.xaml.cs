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
    /// Interaction logic for GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        public delegate void CloseApp();
        public event CloseApp OnClose;

        public GameControl()
        {
            InitializeComponent();
        }

        private void btnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            if (OnClose != null)
            {
                OnClose();
            }
        }

        private void Square_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnLeaveGame_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnStartup_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
