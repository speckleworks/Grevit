using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SpeckleClientUI;
using SpeckleCore;

namespace SpeckleClientUI
{
    /// <summary>
    /// Interaction logic for Receiver.xaml
    /// </summary>
    public partial class ReceiversUi : UserControl
    {
        ObservableCollection<Receiver> _receivers = new ObservableCollection<Receiver>();
        public delegate void ReceivedData(List<SpeckleObject> objs);
        public event ReceivedData OnUpdateGlobal;

        public ReceiversUi()
        {
            InitializeComponent();
            ReceiverItemsControl.ItemsSource = _receivers;
        }

        private void NewReceiverClick(object sender, RoutedEventArgs e)
        {
            var myForm = new SpecklePopup.MainWindow();
            //TODO: fix this it's crashing revit
            //myForm.Owner = Application.Current.MainWindow;

            //if default account exists form is closed automatically
            if (!myForm.HasDefaultAccount)
                myForm.ShowDialog();
            if (myForm.restApi != null && myForm.apitoken != null)
            {
                _receivers.Add(new Receiver(StreamId.Text, myForm.selectedServer, myForm.restApi, myForm.apitoken, myForm.selectedEmail));
            }
            else
            {
                MessageBox.Show("Account selection failed.");
            }
        }

        #region commands

        private void OnClickReceiveStream(object sender, ExecutedRoutedEventArgs e)
        {
            var r = e.Parameter as Receiver;

           var objs = r.UpdateGlobal();
            OnUpdateGlobal(objs);
        }

      

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            if (_receivers.Any(x=>x.Transmitting))
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        #endregion

        private void PasteClick(object sender, RoutedEventArgs e)
        {
            var c = Clipboard.GetText();
            if (c.Length>10)
                c = c.Substring(0, 10);
            StreamId.Text = c;
        }

        private void AccountsClick(object sender, RoutedEventArgs e)
        {
            var myForm = new SpecklePopup.MainWindow(false);
            myForm.ShowDialog();
        }
    }


}
