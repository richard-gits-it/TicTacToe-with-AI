using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDemo
{
    public static class MyDataHelper
    {
        public static Client _client;
        public static ObservableCollection<string> Players { get; set; }
        public static string currentPlayer;
        public static bool isConnected = false;

    }
}
