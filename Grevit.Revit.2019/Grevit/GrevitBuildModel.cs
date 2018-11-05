using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Grevit.Types;
using SpeckleCore;

namespace Grevit.Revit
{

  /// <summary>
  /// The actual Revit Grevit Command
  /// </summary>
  public class GrevitBuildModel
  {
    public GrevitBuildModel( Autodesk.Revit.DB.Document doc )
    {
      document = doc;
    }

    /// <summary>
    /// Current Revit document
    /// </summary>
    public static Document document;

    /// <summary>
    /// Elements newly created by Grevit
    /// </summary>
    public static Dictionary<string, ElementId> CreatedElements;

    /// <summary>
    /// Existing Grevit Elements 
    /// </summary>
    public static Dictionary<string, ElementId> ExistingElements;

    /// <summary>
    /// List for roof shape points to apply
    /// </summary>
    public static List<Tuple<ElementId, CurveArray>> RoofShapePoints;

    /// <summary>
    /// Default scale meters to feet
    /// </summary>
    public static double Scale = 3.2808399;

    /// <summary>
    /// Version of the API being used
    /// </summary>
#if ( Revit2016 )
        public static string Version = "2016";
#endif
#if ( Revit2015 )
        public static string Version = "2015";
#endif
#if ( Revit2017 )
        public static string Version = "2017";
#endif
#if ( Revit2018 )
        public static string Version = "2018";
#endif
#if ( Revit2019 )
    public static string Version = "2019";
#endif
    /// <summary>
    /// Revit Template Folder for creating template based family instances
    /// </summary>
    public static string RevitTemplateFolder = String.Format( @"C:\ProgramData\Autodesk\RAC {0}\Family Templates\English", Version );

    public Result BuildModel( ComponentCollection components )
    {
      bool delete = false;

      if ( components == null )
      {
        // Create new Grevit Client sending existing Families 
        Grevit.Client.ClientWindow grevitClientDialog = new Grevit.Client.ClientWindow( document.GetFamilies() );
        //Grevit.Serialization.Client grevitClientDialog = new Grevit.Serialization.Client(document.GetFamilies());

        // Show Client Dialog
        grevitClientDialog.ShowWindow();
        //if (grevitClientDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return Result.Cancelled;

        // Set the received component collection
        components = grevitClientDialog.componentCollection;

        delete = grevitClientDialog.componentCollection.delete;

        Scale = grevitClientDialog.componentCollection.scale;
      }

      RoofShapePoints = new List<Tuple<ElementId, CurveArray>>();


      // Set up a List for stalled components (with References)
      List<Component> componentsWithReferences = new List<Component>();

      // Get all existing Grevit Elements from the Document
      // If Update is false this will just be an empty List
      ExistingElements = document.GetExistingGrevitElements( components.update );

      // Set up an empty List for created Elements
      CreatedElements = new Dictionary<string, ElementId>();


      #region createComponents

      Transaction trans = new Transaction( GrevitBuildModel.document, "GrevitCreate" );
      trans.Start();

      // Walk thru all received components
      foreach ( Component component in components.Items )
      {
        // If they are not reference dependent, create them directly
        // Otherwise add the component to a List of stalled elements
        if ( !component.stalledForReference )
        {
          try
          {
            component.Build( false );
          }
          catch ( Exception e )
          {
            //Grevit.Reporting.MessageBox.Show( component.GetType().Name + " Error", e.InnerException.Message );
          }
        }
        else
          componentsWithReferences.Add( component );
      }

      // Walk thru all elements which are stalled because they are depending on
      // an Element which needed to be created first


      foreach ( Component component in componentsWithReferences )
      {
        try
        {
          component.Build( true );
        }
        catch ( Exception e )
        {
          //Grevit.Reporting.MessageBox.Show( component.GetType().Name + " Error", e.InnerException.Message );
        }
      }

      trans.Commit();
      trans.Dispose();



      foreach ( Tuple<ElementId, CurveArray> rsp in RoofShapePoints )
      {
        if ( rsp.Item1 != ElementId.InvalidElementId )
        {
          Autodesk.Revit.DB.RoofBase roof = ( Autodesk.Revit.DB.RoofBase ) document.GetElement( rsp.Item1 );
          if ( roof != null )
          {
            if ( roof.SlabShapeEditor != null )
            {
              if ( roof.SlabShapeEditor.IsEnabled )
              {
                Transaction pp = new Transaction( GrevitBuildModel.document, "GrevitPostProcessing" );
                pp.Start();
                roof.SlabShapeEditor.Enable();
                pp.Commit();
                pp.Dispose();
              }

              List<XYZ> points = new List<XYZ>();
              foreach ( Curve c in rsp.Item2 )
                points.Add( c.GetEndPoint( 0 ) );

              Transaction ppx = new Transaction( GrevitBuildModel.document, "GrevitPostProcessing" );
              ppx.Start();

              foreach ( SlabShapeVertex v in roof.SlabShapeEditor.SlabShapeVertices )
              {
                double Zdiff = 0;

                foreach ( XYZ pt in points )
                {
                  if ( Math.Abs( v.Position.X - pt.X ) < double.Epsilon
                      && Math.Abs( v.Position.Y - pt.Y ) < double.Epsilon
                      && Math.Abs( v.Position.Z - pt.Z ) > double.Epsilon )
                    Zdiff = pt.Z;
                }

                if ( Zdiff != 0 )
                  roof.SlabShapeEditor.ModifySubElement( v, Zdiff );
              }

              ppx.Commit();
              ppx.Dispose();

            }

          }

        }
      }



      #endregion


      // If Delete Setting is activated
      if ( delete )
      {
        // Create a new transaction
        Transaction transaction = new Transaction( document, "GrevitDelete" );
        transaction.Start();

        // get the Difference between existing and new elements to erase them
        IEnumerable<KeyValuePair<string, ElementId>> unused =
            ExistingElements.Except( CreatedElements ).Concat( CreatedElements.Except( ExistingElements ) );

        // Delete those elements from the document
        foreach ( KeyValuePair<string, ElementId> element in unused ) document.Delete( element.Value );

        // commit and dispose the transaction
        transaction.Commit();
        transaction.Dispose();
      }



      return Result.Succeeded;
    }

    /// <summary>
    /// A copy paste of the BuildModel above with removed forms, etc.
    /// </summary>
    /// <param name="components"></param>
    /// <returns></returns>
    public Result SpeckleBake( IEnumerable<SpeckleObject> added, IEnumerable<SpeckleObject> deleted, IEnumerable<SpeckleObject> unchanged, double scale )
    {
      GrevitBuildModel.Scale = scale;

      // nasty side-dependencies in code, do not touch!
      ExistingElements = document.GetExistingGrevitElements( true );
      CreatedElements = new Dictionary<string, ElementId>();

      var deleteTransaction = new Transaction( document, "SpeckleGrevitDelete" );
      deleteTransaction.Start();

      var ToDeleteComponents = SpeckleCore.Converter.Deserialise( deleted );

      //  TODO
      foreach ( var obj in deleted )
      {

      }


      deleteTransaction.Commit();
      deleteTransaction.Dispose();

      var createTransaction = new Transaction( GrevitBuildModel.document, "SpeckleGrevitAdd" );
      createTransaction.Start();

      var ToAddComponents = SpeckleCore.Converter.Deserialise( added ).Select( obj => obj as Component );
      var componentsWithReferences = new List<Component>();

      foreach ( Component component in ToAddComponents )
      {
        // If they are not reference dependent, create them directly
        // Otherwise add the component to a List of stalled elements
        if ( !component.stalledForReference )
        {
          try
          {
            component.Build( false );
          }
          catch ( Exception e )
          {
            Debug.WriteLine( "Failed to build object" );
          }
        }
        else
          componentsWithReferences.Add( component );
      }

      foreach ( Component component in componentsWithReferences )
      {
        try
        {
          component.Build( true );
        }
        catch ( Exception e )
        {
          Debug.WriteLine( "Failed to build referebce object" );
        }
      }

      createTransaction.Commit();
      createTransaction.Dispose();

      // If Delete Setting is activated
      if ( false )
      {
        // Create a new transaction
        Transaction transaction = new Transaction( document, "GrevitDelete" );
        transaction.Start();

        // get the Difference between existing and new elements to erase them
        IEnumerable<KeyValuePair<string, ElementId>> unused =
            ExistingElements.Except( CreatedElements ).Concat( CreatedElements.Except( ExistingElements ) );

        // Delete those elements from the document
        foreach ( KeyValuePair<string, ElementId> element in unused ) document.Delete( element.Value );

        // commit and dispose the transaction
        transaction.Commit();
        transaction.Dispose();
      }



      return Result.Succeeded;
    }
  }

}
