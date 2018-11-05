using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;

namespace SpeckleClientUI
{
  public interface ISpeckleHostBuilder
  {
    void Add( IEnumerable<SpeckleObject> objects, double scale );

    void Delete( IEnumerable<SpeckleObject> objects );
  }

  public  interface ISpeckleHostBuilderGenerator
  {
    ISpeckleHostBuilder GetHostBuilder( );
  }
}
