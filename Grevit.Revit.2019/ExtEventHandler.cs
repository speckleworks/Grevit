using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using SpeckleClientUI;
using SpeckleCore;

namespace Grevit.Revit
{
  /// <summary>
  /// This one here works the magic
  /// </summary>
  public class SpeckleExternalEventHandler : IExternalEventHandler
  {
    public Receiver Receiver;
    /// <summary>
    /// Where the speckle build and bake happens
    /// </summary>
    /// <param name="app"></param>
    public void Execute( UIApplication app )
    {
      var units = ( ( string ) Receiver.Stream.BaseProperties.units ).ToLower();

      // TODO: Check
      double scale = 1;
      switch ( units )
      {
        case "kilometers":
          scale = 3.2808399 * 1000;
          break;
        case "meters":
          scale = 3.2808399;
          break;
        case "centimeters":
          scale = 0.032808399;
          break;
        case "millimiters":
          scale = 0.0032808399;
          break;
        case "miles":
          scale = 5280;
          break;
        case "feet":
          scale = 1;
          break;
        case "inches":
          scale = 0.0833333;
          break;
      };

      try
      {
        var deleted = Receiver.PreviousStream.Objects.Where( old => !Receiver.Stream.Objects.Any( o => o._id == old._id ) ).ToList();

        var added = Receiver.Stream.Objects.Where( obj => !Receiver.PreviousStream.Objects.Any( o => o._id == obj._id ) ).ToList();

        var pause = "here";

        if ( deleted != null && deleted.Count > 0 ) Receiver.Builder.Delete( deleted );
        if ( added != null && added.Count > 0 ) Receiver.Builder.Add( added, scale );
        Receiver.CommitStage();

      }
      catch(Exception  e) {
        Receiver.Builder.Add( Receiver.Stream.Objects, scale );
      }

      //grevit.BuildModel( new Grevit.Types.ComponentCollection()
      //{
      //    Items = pizza.Where( xx => xx is Grevit.Types.Component ).Select( xxx => xxx as Grevit.Types.Component ).ToList()
      //});
    }

    public string GetName( )
    {
      return "Speckle Bake";
    }
  }
}
