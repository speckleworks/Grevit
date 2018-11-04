using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Grevit.Types;
using SpeckleCore;

namespace Grevit.Revit
{

  public interface ISpeckleRevitBuilder
  {
    void Build( IEnumerable<SpeckleObject> objects, double scale );
  }

  public class SpeckleGrevitBuilder : ISpeckleRevitBuilder
  {
    public Document RevitDoc;
    public double Scale;


    public SpeckleGrevitBuilder( Document _doc )
    {
      RevitDoc = _doc;
      Scale = 1;
    }

    public void Build( IEnumerable<SpeckleObject> objects, double scale )
    {
      Scale = scale;

      var grevitObjects = SpeckleCore.Converter.Deserialise( objects ).Select( o => o as Component ).ToList();
      var speckleObjetcs = objects.ToList();

      // replace grevit id with speckle hash
      int k = 0;
      foreach ( var gObj in grevitObjects )
        gObj.GID = speckleObjetcs[ k++ ].Hash;

      var readyToBuild = grevitObjects.Where( g => !g.stalledForReference ).ToList();

      foreach( var component  in readyToBuild)
      {
        component.Build( false, this );
      }

      var withReferences = grevitObjects.Where( g => g.stalledForReference ).ToList();

      foreach( var component in withReferences)
      {
        component.Build( true, this );
      }

    }
  }
}
