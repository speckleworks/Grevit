using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Grevit.Types;
using SpeckleClientUI;
using SpeckleCore;

namespace Grevit.Revit
{
  public class SpeckleGrevitBuilder : ISpeckleHostBuilder
  {
    public Document RevitDoc;
    public double Scale;
    public Dictionary<string, ElementId> CreatedElements;
    public Dictionary<string, ElementId> ExistingElements;

    public SpeckleGrevitBuilder( Document _doc )
    {
      RevitDoc = _doc;
      Scale = 3.2808399;

      CreatedElements = new Dictionary<string, ElementId>();
      ExistingElements = new Dictionary<string, ElementId>();
    }

    public void Build( IEnumerable<SpeckleObject> objects, double scale )
    {
      if ( objects == null )
      {
        throw new ArgumentNullException( nameof( objects ) );
      }

      Scale = scale;

      var grevitObjects = SpeckleCore.Converter.Deserialise( objects ).Select( o => o as Component ).ToList();
      var speckleObjetcs = objects.ToList();

      // replace grevit id with speckle hash
      int k = 0;
      foreach ( var gObj in grevitObjects )
        gObj.GID = speckleObjetcs[ k++ ].Hash;

      var readyToBuild = grevitObjects.Where( g => !g.stalledForReference ).ToList();

      foreach ( var component in readyToBuild )
      {
        component.Build( false, this );
      }

      var withReferences = grevitObjects.Where( g => g.stalledForReference ).ToList();

      foreach ( var component in withReferences )
      {
        component.Build( true, this );
      }
    }

    public void Delete( IEnumerable<SpeckleObject> objects )
    {
      var ExistingGrevitElems = RevitDoc.GetExistingGrevitElements( true );

      var toDelete = ExistingGrevitElems.Where( elem =>
      objects.Any( a => a.Hash == elem.Key )
      );


      var pause = "here";

      var ls = toDelete.ToList();
      var cp = ls;

      var DeleteTransaction = new Transaction( RevitDoc, "SpeckleGrevitDelete" );
      DeleteTransaction.Start();

      RevitDoc.Delete(toDelete.Select(x =>x.Value).ToList());

      DeleteTransaction.Commit();
      DeleteTransaction.Dispose();

    }

    public void Add( IEnumerable<SpeckleObject> objects, double scale )
    {
      CreatedElements = new Dictionary<string, ElementId>();
      ExistingElements = new Dictionary<string, ElementId>();

      var grevitComps = SpeckleCore.Converter.Deserialise( objects ).Select( x => x as Component ).ToList();
      var spkObjs = objects.ToList();

      for ( int i = 0; i < spkObjs.Count(); i++ )
      {
        grevitComps[ i ].GID = spkObjs[ i ].Hash;
      }

      var AddTransaction = new Transaction( RevitDoc, "SpeckleGrevitAdd" );
      AddTransaction.Start();

      var readyToBuild = grevitComps.Where( g => !g.stalledForReference ).ToList();

      string errors = "";

      foreach ( var component in readyToBuild )
      {
        try
        {
          component.Build( false, this );
        }
        catch ( Exception e )
        {
          errors += e.Message + "\n";
        }
      }

      var withReferences = grevitComps.Where( g => g.stalledForReference ).ToList();
      foreach ( var component in withReferences )
      {
        try
        {
          component.Build( true, this );
        }
        catch ( Exception e )
        {
          errors += e.Message + "\n";
        }
      }

      AddTransaction.Commit();
      AddTransaction.Dispose();

      if ( errors != "" )
        System.Windows.MessageBox.Show( errors, @"¯\_(ツ)_/¯" );
    }
  }
}
