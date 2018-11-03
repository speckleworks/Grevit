//
//  Grevit - Create Autodesk Revit (R) Models in McNeel's Rhino Grassopper 3D (R)
//  For more Information visit grevit.net or food4rhino.com/project/grevit
//  Copyright (C) 2015
//  Authors: Maximilian Thumfart,
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Diagnostics;
using System.Xml;
using System.Net.Sockets;
using System.Net;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.IO;
using System.Threading;
using Grevit.Types;
using System.Windows.Media.Imaging;
using SpeckleCore;
using MaterialDesignColors;
using MaterialDesignThemes;
using System.Windows.Media;

namespace Grevit.Revit
{
    /// <summary>
    /// Create Grevit UI
    /// </summary>    
    class GrevitUI : IExternalApplication
    {
        /// <summary>
        /// Grevit Assembly path
        /// </summary>
        static string path = typeof( GrevitUI ).Assembly.Location;

        /// <summary>
        /// Create UI on StartUp
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public Result OnStartup( UIControlledApplication application )
        {
            RibbonPanel grevitPanel = null;

            foreach ( RibbonPanel rpanel in application.GetRibbonPanels() )
                if ( rpanel.Name == "Grevit" ) grevitPanel = rpanel;

            if ( grevitPanel == null ) grevitPanel = application.CreateRibbonPanel( "Grevit" );

            PushButton commandButton = grevitPanel.AddItem( new PushButtonData( "GrevitCommand", "Grevit", path, "Grevit.Revit.GrevitCommand" ) ) as PushButton;
            commandButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.paper_airplane.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight( 32, 32 ) );

            commandButton.SetContextualHelp( new ContextualHelp( ContextualHelpType.Url, "http://grevit.net/" ) );

            PushButton parameterButton = grevitPanel.AddItem( new PushButtonData( "ParameterNames", "Parameter names", path, "Grevit.Revit.ParameterNames" ) ) as PushButton;
            parameterButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.tag_hash.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight( 32, 32 ) );

            parameterButton.SetContextualHelp( new ContextualHelp( ContextualHelpType.Url, "http://grevit.net/" ) );

            PushButton getFaceRefButton = grevitPanel.AddItem( new PushButtonData( "GetFaceReference", "Face Reference", path, "Grevit.Revit.GrevitFaceReference" ) ) as PushButton;
            getFaceRefButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.radio_button.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight( 32, 32 ) );

            getFaceRefButton.SetContextualHelp( new ContextualHelp( ContextualHelpType.Url, "http://grevit.net/" ) );

            PushButton speckleButton = grevitPanel.AddItem( new PushButtonData( "Speckle Client", "Speckle Client", path, "Grevit.Revit.SpeckleClient" ) ) as PushButton;

            speckleButton.LargeImage = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                Properties.Resources.speckle.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight( 32, 32 ) );

            speckleButton.SetContextualHelp( new ContextualHelp( ContextualHelpType.Url, "https://github.com/speckleworks/Grevit" ) );

            return Result.Succeeded;
        }

        public Result OnShutdown( UIControlledApplication a )
        {
            return Result.Succeeded;
        }

    }

    /// <summary>
    /// The actual Revit Grevit Command
    /// </summary>
    [Transaction( TransactionMode.Manual )]
    public class GrevitCommand : IExternalCommand
    {

        public Result Execute( ExternalCommandData commandData, ref string message, ElementSet elements )
        {
            // Get Environment Variables
            UIApplication uiApp = commandData.Application;
            GrevitBuildModel model = new GrevitBuildModel( uiApp.ActiveUIDocument.Document );
            return model.BuildModel( null );
        }

    }


    [Transaction( TransactionMode.Manual )]
    public class GrevitFaceReference : IExternalCommand
    {

        public Result Execute( ExternalCommandData commandData, ref string message, ElementSet elements )
        {
            // Get Environment Variables
            UIApplication uiApp = commandData.Application;
            Reference reference = uiApp.ActiveUIDocument.Selection.PickObject( ObjectType.Face, "Select Face" );
            if ( reference != null )
            {
                string stable = reference.ConvertToStableRepresentation( uiApp.ActiveUIDocument.Document );
                Grevit.Reporting.MessageBox.ShowInTextBox( "Stable Reference:", stable );
            }

            return Result.Succeeded;
        }

    }

    [Transaction( TransactionMode.Manual )]
    public class SpeckleClient : IExternalCommand
    {
        public static bool Launched = false;
        public static SpeckleClientWindow speckleClient;
        public static GrevitBuildModel grevit;

        public Result Execute( ExternalCommandData commandData, ref string message, ElementSet elements )
        {
            if ( !Launched )
            {
                var uiApp = commandData.Application;
                grevit = new GrevitBuildModel( commandData.Application.ActiveUIDocument.Document );

                var hand = new SpeckleExternalEventHandler();
                var eve = ExternalEvent.Create( hand );

                speckleClient = new SpeckleClientWindow( uiApp, eve, hand );
                speckleClient.Show();

                Launched = true;
                return Result.Succeeded;
            }

            speckleClient.Show();
            speckleClient.Focus();

            return Result.Succeeded;
        }
    }

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
        public static Dictionary<string, ElementId> created_Elements;

        /// <summary>
        /// Existing Grevit Elements 
        /// </summary>
        public static Dictionary<string, ElementId> existing_Elements;

        /// <summary>
        /// List for roof shape points to apply
        /// </summary>
        public static List<Tuple<ElementId, CurveArray>> RoofShapePoints;

        public static double Scale = 1;

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
            existing_Elements = document.GetExistingGrevitElements( components.update );

            // Set up an empty List for created Elements
            created_Elements = new Dictionary<string, ElementId>();


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
                    existing_Elements.Except( created_Elements ).Concat( created_Elements.Except( existing_Elements ) );

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
            existing_Elements = document.GetExistingGrevitElements( true );
            created_Elements = new Dictionary<string, ElementId>();

            var deleteTransaction = new Transaction( document, "SpeckleGrevitDelete" );
            deleteTransaction.Start();

            var ToDeleteComponents = SpeckleCore.Converter.Deserialise( deleted );

            //  TODO
            foreach( var obj in deleted)
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
                    existing_Elements.Except( created_Elements ).Concat( created_Elements.Except( existing_Elements ) );

                // Delete those elements from the document
                foreach ( KeyValuePair<string, ElementId> element in unused ) document.Delete( element.Value );

                // commit and dispose the transaction
                transaction.Commit();
                transaction.Dispose();
            }



            return Result.Succeeded;
        }
    }



    /// <summary>
    /// Provides a parameter overview on selected elements
    /// </summary>
    [Transaction( TransactionMode.Manual )]
    public class ParameterNames : IExternalCommand
    {
        private UIApplication uiApp;

        private System.Reflection.Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            // if the request is coming from us
            if ( ( args.RequestingAssembly != null ) && ( args.RequestingAssembly == this.GetType().Assembly ) )
            {
                if ( ( args.Name != null ) && ( args.Name.Contains( "," ) ) ) // ignore resources and such
                {
                    string asmName = args.Name.Split( ',' )[ 0 ];
                    string targetFilename = Path.Combine( System.Reflection.Assembly.GetExecutingAssembly().Location, asmName + ".dll" );
                    uiApp.Application.WriteJournalComment( "Assembly Resolve issue. Looking for: " + args.Name, false );
                    uiApp.Application.WriteJournalComment( "Looking for " + targetFilename, false );
                    if ( File.Exists( targetFilename ) )
                    {
                        uiApp.Application.WriteJournalComment( "Found, and loading...", false );
                        return System.Reflection.Assembly.LoadFrom( targetFilename );
                    }
                }
            }
            return null;
        }

        public Result Execute( ExternalCommandData commandData, ref string message, ElementSet elements )
        {
            // Get Revit Environment
            uiApp = commandData.Application;
            Document doc = uiApp.ActiveUIDocument.Document;
            UIDocument uidoc = uiApp.ActiveUIDocument;

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            // Initialize a new Dictionary containing 4 string fields for parameter values
            Dictionary<ElementId, Tuple<string, string, string, string>> Parameters =
                new Dictionary<ElementId, Tuple<string, string, string, string>>();

            // Walk thru selected elements
            foreach ( ElementId elementid in uidoc.Selection.GetElementIds() )
            {
                // Get the element
                Element element = doc.GetElement( elementid );

                // Walk thru all parameters
                foreach ( Autodesk.Revit.DB.Parameter param in element.Parameters )
                {
                    // If the parameter hasn't been added yet, 
                    // Add a new entry to the dictionary
                    if ( !Parameters.ContainsKey( param.Id ) )
                        Parameters.Add( param.Id, new Tuple<string, string, string, string>
                        (
                            param.Definition.Name,
                            param.StorageType.ToString(),
                            param.AsValueString(),
                            param.IsReadOnly.ToString() )
                        );
                }
            }

            // Create a new instance of the parameter List Dialog
            ParameterList parameterListDialog = new ParameterList();

            // Walk thru the sorted (by name) dictionary
            foreach ( KeyValuePair<ElementId, Tuple<string, string, string, string>> kvp in Parameters.OrderBy( val => val.Value.Item1 ) )
            {
                // Create a ListViewItem containing all four values as subitems
                System.Windows.Forms.ListViewItem lvi = new System.Windows.Forms.ListViewItem();
                lvi.Text = kvp.Key.IntegerValue.ToString();
                lvi.SubItems.Add( kvp.Value.Item1 );
                lvi.SubItems.Add( kvp.Value.Item2 );
                lvi.SubItems.Add( kvp.Value.Item3 );
                lvi.SubItems.Add( kvp.Value.Item4 );

                // Add the ListViewItem to the List View
                parameterListDialog.parameters.Items.Add( lvi );
            }

            // Show the Dialog
            parameterListDialog.ShowDialog();

            // Return Success
            return Result.Succeeded;
        }
    }
}


