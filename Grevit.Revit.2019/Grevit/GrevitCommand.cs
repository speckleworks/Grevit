using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Grevit.Revit
{
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
}
