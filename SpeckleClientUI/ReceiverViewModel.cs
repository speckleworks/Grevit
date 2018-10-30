using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleClientUI
{
    public class ReceiverViewModel : INotifyPropertyChanged
    {
        private string _authToken;
        private string _restApi;
        private string _email;
        private string _server;
        private string _streamId;
        private string _streamName;
        private bool _transmitting;

        internal string AuthToken { get => _authToken; set { _authToken = value; NotifyPropertyChanged("AuthToken"); } }
        internal bool Expired = false;


        public string RestApi { get => _restApi; set { _restApi = value; NotifyPropertyChanged("RestApi"); } }
        public string Email { get => _email; set { _email = value; NotifyPropertyChanged("Email"); } }
        public string Server { get => _server; set { _server = value; NotifyPropertyChanged("Server"); } }
        public string StreamId { get => _streamId; set { _streamId = value; NotifyPropertyChanged("StreamId"); } }
        public string StreamName { get => _streamName; set { _streamName = value; NotifyPropertyChanged("StreamName"); } }
        public bool Transmitting { get => _transmitting; set { _transmitting = value; NotifyPropertyChanged("Transmitting"); } }


        
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }

    //only used for the DesignData xaml
    public class Receivers : List<Object>
    {
        public Receivers() { }
    }
}
