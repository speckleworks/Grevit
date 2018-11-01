using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using SpeckleCore;

namespace SpeckleClientUI
{
    /// <summary>
    /// Interaction logic for Receiver.xaml
    /// </summary>
    public partial class ReceiversUi : UserControl
    {
        private ObservableCollection<Receiver> _receivers = new ObservableCollection<Receiver>();
        private ObservableCollection<Account> _accounts = new ObservableCollection<Account>();
        public delegate void ReceivedData(List<SpeckleObject> objs);
        public event ReceivedData OnUpdateGlobal;

        public ReceiversUi()
        {
            InitializeComponent();
            LocalContext.Init();
            _accounts = new ObservableCollection<Account>(LocalContext.GetAllAccounts());
            AccountsComboBox.ItemsSource = _accounts;
            if (_accounts.Any(x => x.IsDefault))
            {
                AccountsComboBox.SelectedIndex = _accounts.IndexOf(_accounts.First(x => x.IsDefault));
            }

            ReceiverItemsControl.ItemsSource = _receivers;
        }

        private void NewReceiverClick(object sender, RoutedEventArgs e)
        {

            if (StreamsComboBox.SelectedIndex == -1 || AccountsComboBox.SelectedIndex==-1)
                return;

            var stream = StreamsComboBox.SelectedItem as SpeckleStream;
            var account = _accounts[AccountsComboBox.SelectedIndex];

            _receivers.Add(new Receiver(stream.StreamId, account.ServerName, account.RestApi, account.Token, account.Email));
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

            if (_receivers.Any(x => x.Transmitting))
            {
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = true;
            }
        }

        #endregion

        private void PasteClick(object sender, RoutedEventArgs e)
        {
            //var c = Clipboard.GetText();
            //if (c.Length>10)
            //    c = c.Substring(0, 10);
            //StreamId.Text = c;
        }

        private void AccountsClick(object sender, RoutedEventArgs e)
        {
            var myForm = new SpecklePopup.MainWindow(false);
            myForm.ShowDialog();
        }

        private void AccountsComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountsComboBox.SelectedIndex == -1)
            {
                return;
            }

            var client = new SpeckleApiClient();
            var account = _accounts[AccountsComboBox.SelectedIndex];

            client.BaseUrl = account.RestApi;
            client.AuthToken = account.Token;
            client.StreamsGetAllAsync().ContinueWith(tsk =>
            {
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() =>
                  {
                      StreamsComboBox.ItemsSource = tsk.Result.Resources.ToList();
                  }));

            });

        }
    }


}
