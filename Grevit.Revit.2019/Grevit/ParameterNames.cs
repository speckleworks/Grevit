using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Grevit.Revit
{
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
