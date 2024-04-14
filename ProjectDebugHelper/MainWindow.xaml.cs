using ServerProject;
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

namespace ProjectDebugHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ServerForm serverForm = new ServerForm();
            serverForm.Show();

            //run window of 280Final
            _280Final.MainWindow gameWindow = new _280Final.MainWindow();
            gameWindow.Show();

            //run window of 280Final
            _280Final.MainWindow gameWindow1 = new _280Final.MainWindow();
            gameWindow1.Show();

            //run window of 280Final
            _280Final.MainWindow gameWindow2 = new _280Final.MainWindow();
            gameWindow2.Show();
        }
    }
}
