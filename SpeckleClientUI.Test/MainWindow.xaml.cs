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

namespace SpeckleClientUI.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            receiver.DataContext = new List<ReceiverViewModel>
            {
                new ReceiverViewModel
                {
                    StreamName="TEST",
                    StreamId="123",
                },
                                new ReceiverViewModel
                {
                    StreamName="AAAA",
                    StreamId="sdfsdf",
                },
            };
        }
    }
}
