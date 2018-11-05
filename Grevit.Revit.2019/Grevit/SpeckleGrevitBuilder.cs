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

    public void Build( Receiver receiver )
    {
      Scale = receiver.Scale;
      GrevitBuildModel.Scale = Scale;

      CreatedElements = new Dictionary<string, ElementId>();
      ExistingElements = new Dictionary<string, ElementId>();

      var speckleObjects = receiver.Stream.Objects;

      var ExistingGrevitElems = RevitDoc.GetExistingGrevitElements( true );

      var deleted = receiver.PreviousStream.Objects.Where( old => !receiver.Stream.Objects.Any( o => o._id == old._id ) ).ToList();

      var toDelete = ExistingGrevitElems.Where( elem => deleted.Any( a => a.Hash == elem.Key ) ).ToList();

      var added = receiver.Stream.Objects.Where( obj => !receiver.PreviousStream.Objects.Any( o => o._id == obj._id ) ).ToList();

      var userDeleted = receiver.Stream.Objects.Where( obj => !ExistingGrevitElems.Any( kvp => kvp.Key == obj.Hash ) ).ToList();

      foreach ( var obj in userDeleted )
      {
        if ( !added.Any( o => o._id == obj._id ) )
          added.Add( obj );
      }

      var DeleteTransaction = new Transaction( RevitDoc, "SpeckleGrevitDelete" );
      DeleteTransaction.Start();

      RevitDoc.Delete( toDelete.Select( x => x.Value ).ToList() );

      DeleteTransaction.Commit();
      DeleteTransaction.Dispose();

      var grevitCompsToAdd = SpeckleCore.Converter.Deserialise( added ).Select( g => g as Component ).ToList();
      for ( int i = 0; i < added.Count(); i++ )
        grevitCompsToAdd[ i ].GID = added[ i ].Hash;

      var readyToBuild = grevitCompsToAdd.Where( g => !g.stalledForReference ).ToList();

      string errors = "";

      var AddTransaction = new Transaction( RevitDoc, "SpeckleGrevitAdd" );
      AddTransaction.Start();

      foreach ( var component in readyToBuild )
      {
        try
        {
          component.Build( false, this );
        }
        catch ( Exception e )
        {
          errors += e.InnerException?.Message + "\n";
        }
      }

      var withReferences = grevitCompsToAdd.Where( g => g.stalledForReference ).ToList();
      foreach ( var component in withReferences )
      {
        try
        {
          component.Build( true, this );
        }
        catch ( Exception e )
        {
          errors += e.InnerException?.Message + "\n";
        }
      }

      AddTransaction.Commit();
      AddTransaction.Dispose();

      if ( errors != "" )
        System.Windows.MessageBox.Show( errors, @"¯\_(ツ)_/¯" );

      receiver.CommitStage();
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

      RevitDoc.Delete( toDelete.Select( x => x.Value ).ToList() );

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
        grevitComps[ i ].GID = spkObjs[ i ].Hash;

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
