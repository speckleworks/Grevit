using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Grevit.Revit
{
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
}
