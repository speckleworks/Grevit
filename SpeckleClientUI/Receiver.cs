using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SpeckleCore;

namespace SpeckleClientUI
{
    public class Receiver : INotifyPropertyChanged
    {
        private string _authToken;
        private string _restApi;
        private string _email;
        private string _server;
        private string _streamId;
        private string _streamName;
        private string _message;
        private bool _transmitting;
        private bool _expired;
        private SpeckleApiClient _client;

        internal string AuthToken { get => _authToken; set { _authToken = value; NotifyPropertyChanged("AuthToken"); } }


        public string RestApi { get => _restApi; set { _restApi = value; NotifyPropertyChanged("RestApi"); } }
        public string Email { get => _email; set { _email = value; NotifyPropertyChanged("Email"); } }
        public string Server { get => _server; set { _server = value; NotifyPropertyChanged("Server"); } }
        public string StreamId { get => _streamId; set { _streamId = value; NotifyPropertyChanged("StreamId"); } }
        public string StreamName { get => _streamName; set { _streamName = value; NotifyPropertyChanged("StreamName"); } }
        public string Message { get => _message; set { _message = value; NotifyPropertyChanged("Message"); } }
        public bool Transmitting { get => _transmitting; set { _transmitting = value; NotifyPropertyChanged("Transmitting");
                //little hack to get ui to refresh the canexecute binding 
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal, (DispatcherOperationCallback)delegate (object arg) {
                        CommandManager.InvalidateRequerySuggested();
                        return null;
                    },
                    null);
        }}
        public bool Expired { get => _expired; set { _expired = value; NotifyPropertyChanged("Expired"); } }

        public Receiver(string streamid, string server, string restapi, string authtoken, string email)
        {
            Transmitting = true;

            StreamId = streamid;
            Email = email;
            Server = server;
            RestApi = restapi;
            AuthToken = authtoken;

            _client = new SpeckleApiClient(RestApi, true);

            _client.OnReady += (sender, e) =>
            {
                UpdateMeta();
            };

            _client.OnWsMessage += OnWsMessage;

            _client.OnError += (sender, e) =>
            {
                Console.Write(e);
                //if (e.EventName == "websocket-disconnected")
                //    return;
                //Warning(e.EventName + ": " + e.EventData);
            };


            _client.IntializeReceiver(StreamId, "", "Dynamo", "", AuthToken);
        }

        public virtual void UpdateMeta()
        {
            var result = _client.StreamGetAsync(_client.StreamId, "fields=name").Result;

            StreamName = result.Resource.Name;
            Transmitting = false;
        }

        public virtual void OnWsMessage(object source, SpeckleEventArgs e)
        {
            switch ((string)e.EventObject.args.eventType)
            {
                case "update-global":
                    Message = "Update available since " + DateTime.Now;
                    Expired = true;
                    break;
                case "update-meta":
                    UpdateMeta();
                    break;
                case "update-name":
                    UpdateMeta();
                    break;
                default:
                    //CustomMessageHandler((string)e.EventObject.args.eventType, e);
                    break;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
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
