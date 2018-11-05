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
  /// <summary>
  /// Custom class that builds a speckle stream containing grevit elements.
  /// </summary>
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

    /// <summary>
    /// Bakes a stream with grevit elements in revit.
    /// </summary>
    /// <param name="receiver"></param>
    public void Build( Receiver receiver )
    {
      Scale = receiver.Scale;
      GrevitBuildModel.Scale = Scale;

      CreatedElements = new Dictionary<string, ElementId>();
      ExistingElements = new Dictionary<string, ElementId>();

      var speckleObjects = receiver.Stream.Objects;

      var ExistingGrevitElems = RevitDoc.GetExistingGrevitElements( true );

      // diff to get removed elements only
      var deleted = receiver.PreviousStream.Objects.Where( old => !receiver.Stream.Objects.Any( o => o._id == old._id ) ).ToList();

      var toDelete = ExistingGrevitElems.Where( elem => deleted.Any( a => a.Hash == elem.Key ) ).ToList();

      // diff to get new elements only
      var added = receiver.Stream.Objects.Where( obj => !receiver.PreviousStream.Objects.Any( o => o._id == obj._id ) ).ToList();
      
      // need to check if the user deleted any manually, and add them again if so.
      var userDeleted = receiver.Stream.Objects.Where( obj => !ExistingGrevitElems.Any( kvp => kvp.Key == obj.Hash ) ).ToList();

      foreach ( var obj in userDeleted )
        if ( !added.Any( o => o._id == obj._id ) )
          added.Add( obj );
      

      // Let's delete stuff!
      var DeleteTransaction = new Transaction( RevitDoc, "SpeckleGrevitDelete" );
      DeleteTransaction.Start();

      RevitDoc.Delete( toDelete.Select( x => x.Value ).ToList() );

      DeleteTransaction.Commit();
      DeleteTransaction.Dispose();

      // Deserialise using speckle abstract magic to native grevit components
      var grevitCompsToAdd = SpeckleCore.Converter.Deserialise( added ).Select( g => g as Component ).ToList();
      // Set their ids to the speckle abstract hashes
      for ( int i = 0; i < added.Count(); i++ )
        grevitCompsToAdd[ i ].GID = added[ i ].Hash;


      // Will keep track of any grevit build errors
      string errors = "";

      // Let's add stuff!
      var AddTransaction = new Transaction( RevitDoc, "SpeckleGrevitAdd" );
      AddTransaction.Start();

      // first things first
      var readyToBuild = grevitCompsToAdd.Where( g => !g.stalledForReference ).ToList();

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

      // seconds later
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

      // finally, make sure the reciever updates its previous state and current state
      receiver.CommitStage();
    }
  }
}
