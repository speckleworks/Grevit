using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpeckleClientUI.Data
{
    public static class Commands
    {
        public static readonly RoutedCommand ReceiveStream = new RoutedCommand("ReceiveStream", typeof(Button));

    }
}
