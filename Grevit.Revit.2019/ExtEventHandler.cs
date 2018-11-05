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
    /// <summary>
    /// Where the speckle build and bake gets triggered.
    /// </summary>
    /// <param name="app"></param>
    public void Execute( UIApplication app )
    {
      // There's something wrong with the line below.
      // TODO: Cleanup flow and referncing, etc. I obviously got confused in the process.
      Receiver.Builder.Build( Receiver );
    }

    public string GetName( )
    {
      return "Speckle Bake";
    }
  }
}
