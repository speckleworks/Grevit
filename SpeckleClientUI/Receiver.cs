using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        internal string AuthToken { get => _authToken; set { _authToken = value; NotifyPropertyChanged( "AuthToken" ); } }

        public string RestApi { get => _restApi; set { _restApi = value; NotifyPropertyChanged( "RestApi" ); } }
        public string Email { get => _email; set { _email = value; NotifyPropertyChanged( "Email" ); } }
        public string Server { get => _server; set { _server = value; NotifyPropertyChanged( "Server" ); } }
        public string StreamId { get => _streamId; set { _streamId = value; NotifyPropertyChanged( "StreamId" ); } }
        public string StreamName { get => _streamName; set { _streamName = value; NotifyPropertyChanged( "StreamName" ); } }
        public string Message { get => _message; set { _message = value; NotifyPropertyChanged( "Message" ); } }
        public bool Transmitting
        {
            get => _transmitting;
            set
            {
                _transmitting = value; NotifyPropertyChanged( "Transmitting" );
                //little hack to get ui to refresh the canexecute binding
                //Application.Current.Dispatcher.Invoke(
                //    DispatcherPriority.Normal, ( DispatcherOperationCallback ) delegate ( object arg )
                //    {

                //        return null;
                //    },
                //    null );
            }
        }
        public bool Expired { get => _expired; set { _expired = value; NotifyPropertyChanged( "Expired" ); } }

        /// <summary>
        /// Keeps track of the current stream
        /// </summary>
        public SpeckleStream Stream;

        /// <summary>
        /// Keeps track of the previous version of the stream.
        /// </summary>
        public SpeckleStream PreviousStream;

        public Receiver( string streamid, string server, string restapi, string authtoken, string email )
        {
            Transmitting = true;

            StreamId = streamid;
            Email = email;
            Server = server;
            RestApi = restapi;
            AuthToken = authtoken;

            _client = new SpeckleApiClient( RestApi, true );

            _client.OnReady += ( sender, e ) =>
            {
                UpdateMeta();
                Stream = _client.Stream;
                PreviousStream = SpeckleStream.FromJson( Stream.ToJson() );
            };

            _client.OnWsMessage += OnWsMessage;

            _client.OnError += ( sender, e ) =>
            {
                Console.Write( e );
            };


            _client.IntializeReceiver( StreamId, "", "Revit", "", AuthToken );
        }

        public virtual void UpdateGlobal( )
        {
            Transmitting = true;
            // clone the previous state
            PreviousStream = SpeckleStream.FromJson(Stream.ToJson());
            // get the new state
            Stream = _client.StreamGetAsync( _client.StreamId, null ).Result.Resource;
            // update matteo's local binding vars, etc.
            StreamName = Stream.Name;

            Message = "Getting objects";

            LocalContext.GetObjects( Stream.Objects, _client.BaseUrl );

            // filter out the objects that were not in the cache and still need to be retrieved
            var payload = Stream.Objects.Where( o => o.Type == SpeckleObjectType.Placeholder ).Select( obj => obj._id ).ToArray();

            // how many objects to request from the api at a time
            int maxObjRequestCount = 20;

            // list to hold them into
            var newObjects = new List<SpeckleObject>();

            // jump in `maxObjRequestCount` increments through the payload array
            for ( int i = 0; i < payload.Length; i += maxObjRequestCount )
            {
                // create a subset
                var subPayload = payload.Skip( i ).Take( maxObjRequestCount ).ToArray();

                // get it sync as this is always execed out of the main thread
                var res = _client.ObjectGetBulkAsync( subPayload, "omit=displayValue" ).Result;

                // put them in our bucket
                newObjects.AddRange( res.Resources );

                // TODO: Bind this message to somewhere!
                Message = String.Format( "Got {0} out of {1} objects.", i, payload.Length );
                System.Diagnostics.Debug.WriteLine( String.Format( "Got {0} out of {1} objects.", i, payload.Length ) );
            }

            // populate the retrieved objects in the original stream's object list
            foreach ( var obj in newObjects )
            {
                var locationInStream = Stream.Objects.FindIndex( o => o._id == obj._id );
                try { Stream.Objects[ locationInStream ] = obj; } catch { }

                // add objects to cache
                LocalContext.AddObject( obj, _client.BaseUrl );
            }

            var copy = Stream.Objects;

            Transmitting = false;
        }

        public virtual void UpdateMeta( )
        {
            var result = _client.StreamGetAsync( _client.StreamId, "fields=name" ).Result;

            Stream = result.Resource;

            StreamName = result.Resource.Name;
            Transmitting = false;
            CommandManager.InvalidateRequerySuggested();
        }

        public virtual void OnWsMessage( object source, SpeckleEventArgs e )
        {
            switch ( ( string ) e.EventObject.args.eventType )
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
        private void NotifyPropertyChanged( string info )
        {
            if ( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( info ) );
            }
        }

    }

    //only used for the DesignData xaml
    public class Receivers : List<Object>
    {
        public Receivers( ) { }
    }
}
