using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;

namespace SpeckleClientUI
{
  /// <summary>
  /// TODO: Extract to separate ui project
  /// Defines a build method that needs to be implemented by any potential speckle baker.
  /// </summary>
  public interface ISpeckleHostBuilder
  {
    void Build( Receiver receiver );
  }

  /// <summary>
  /// TODO: Extract to separate ui project.
  /// </summary>
  public interface ISpeckleHostBuilderGenerator
  {
    /// <summary>
    /// Factory for getting the builder set up with app accesible document, etc.
    /// </summary>
    /// <returns></returns>
    ISpeckleHostBuilder GetHostBuilder( );
  }
}
