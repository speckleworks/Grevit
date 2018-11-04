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
    public ISpeckleRevitBuilder Builder;

    /// <summary>
    /// Where the speckle build and bake happens
    /// </summary>
    /// <param name="app"></param>
    public void Execute( UIApplication app )
    {
      if ( Receiver.PreviousStream == null )
      {
        var x = "Should not happen";
      }

      var deleted = Receiver.PreviousStream.Objects.Except( Receiver.Stream.Objects, new SpeckleObjectComparer() ).ToList();
      var added = Receiver.Stream.Objects.Except( Receiver.PreviousStream.Objects, new SpeckleObjectComparer() ).ToList();
      var unchanged = Receiver.PreviousStream.Objects.Intersect( Receiver.Stream.Objects, new SpeckleObjectComparer() ).ToList();


      //var pizza = SpeckleCore.Converter.Deserialise(Receiver.Stream.Objects);

      var units = ( ( string ) Receiver.Stream.BaseProperties.units ).ToLower();

      //return;

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

      var myGrevit = new GrevitBuildModel( app.ActiveUIDocument.Document );

      myGrevit.SpeckleBake( added, deleted, unchanged, scale );

      Builder.Build( null, scale );

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
