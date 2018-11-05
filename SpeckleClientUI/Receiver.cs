using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SpeckleClientUI.Data;
using SpeckleCore;

namespace SpeckleClientUI
{
  public class Receiver : INotifyPropertyChanged, IContext
  {
    private string _authToken;
    private string _restApi;
    private string _email;
    private string _server;
    private string _streamId;
    private string _streamName;
    private string _message;
    private bool _transmitting;
    private bool _expired = true;
    private SpeckleApiClient _client;

    internal string AuthToken { get => _authToken; set { _authToken = value; NotifyPropertyChanged("AuthToken"); } }

    public string RestApi { get => _restApi; set { _restApi = value; NotifyPropertyChanged("RestApi"); } }
    public string Email { get => _email; set { _email = value; NotifyPropertyChanged("Email"); } }
    public string Server { get => _server; set { _server = value; NotifyPropertyChanged("Server"); } }
    public string StreamId { get => _streamId; set { _streamId = value; NotifyPropertyChanged("StreamId"); } }
    public string StreamName { get => _streamName; set { _streamName = value; NotifyPropertyChanged("StreamName"); } }
    public string Message { get => _message; set { _message = value; NotifyPropertyChanged("Message"); } }
    public bool Transmitting
    {
      get => _transmitting;
      set 
      {
        _transmitting = value;
        NotifyPropertyChanged("Transmitting");

        _dispatcher.Invoke(new Action(() =>
            CommandManager.InvalidateRequerySuggested()
          ));

      }
    }
    public bool Expired { get => _expired; set { _expired = value; NotifyPropertyChanged("Expired"); } }

    public SpeckleStream Stream;

    private readonly Dispatcher _dispatcher;

    public bool IsSynchronized
    {
      get
      {
        return this._dispatcher.Thread == Thread.CurrentThread;
      }
    }

    public Receiver(Dispatcher dispatcher, string streamid, string server, string restapi, string authtoken, string email) 
    {
      this._dispatcher = dispatcher;

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

    //  TODO
    public virtual void UpdateGlobal()
    {
      Transmitting = true;
      Stream = _client.StreamGetAsync(_client.StreamId, null).Result.Resource;
      StreamName = Stream.Name;

      Message = "Getting objects";

      LocalContext.GetObjects(Stream.Objects, _client.BaseUrl);

      // filter out the objects that were not in the cache and still need to be retrieved
      var payload = Stream.Objects.Where(o => o.Type == SpeckleObjectType.Placeholder).Select(obj => obj._id).ToArray();

      // how many objects to request from the api at a time
      int maxObjRequestCount = 20;

      // list to hold them into
      var newObjects = new List<SpeckleObject>();

      // jump in `maxObjRequestCount` increments through the payload array
      for (int i = 0; i < payload.Length; i += maxObjRequestCount)
      {
        // create a subset
        var subPayload = payload.Skip(i).Take(maxObjRequestCount).ToArray();

        // get it sync as this is always execed out of the main thread
        var res = _client.ObjectGetBulkAsync(subPayload, "omit=displayValue").Result;

        // put them in our bucket
        newObjects.AddRange(res.Resources);

        // TODO: Bind this message to somewhere!
        Message = String.Format("Got {0} out of {1} objects.", i, payload.Length);
        System.Diagnostics.Debug.WriteLine(String.Format("Got {0} out of {1} objects.", i, payload.Length));
      }

      // populate the retrieved objects in the original stream's object list
      foreach (var obj in newObjects)
      {
        var locationInStream = Stream.Objects.FindIndex(o => o._id == obj._id);
        try { Stream.Objects[locationInStream] = obj; } catch { }

        // add objects to cache
        LocalContext.AddObject(obj, _client.BaseUrl);
      }

      var copy = Stream.Objects;

      Transmitting = false;
      Expired = false;
    }

    public virtual void UpdateMeta()
    {
      var result = _client.StreamGetAsync(_client.StreamId, "fields=name").Result;

      Stream = result.Resource;

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

    public void Invoke(Action action)
    {
      Debug.Assert(action != null);

      this._dispatcher.Invoke(action);
    }

    public void BeginInvoke(Action action)
    {
      Debug.Assert(action != null);

      this._dispatcher.BeginInvoke(action);
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
