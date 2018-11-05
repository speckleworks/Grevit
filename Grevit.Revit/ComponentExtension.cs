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
using Grevit.Revit;

namespace Grevit.Revit
{
  public static class ComponentExtension
  {
    /// <summary>
    /// Invoke the Components Create Method
    /// </summary>
    /// <param name="component"></param>
    public static void Build( this Grevit.Types.Component component, bool useReferenceElement, SpeckleGrevitBuilder builder = null )
    {

      var MyCreatedElements = builder == null ? GrevitBuildModel.CreatedElements : builder.CreatedElements;

      var MyRevitDoc = builder == null ? GrevitBuildModel.document : builder.RevitDoc;

      // Get the components type
      Type type = component.GetType();

      // Get the Create extension Method using reflection
      IEnumerable<System.Reflection.MethodInfo> methods = Grevit.Reflection.Utilities.GetExtensionMethods( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Assembly, type );

      // Check all extensions methods (should only be Create() anyway)
      foreach ( System.Reflection.MethodInfo method in methods )
      {
        // get the methods parameters
        var parameters = new List<Object>();

        // As it is an extension method, the first parameter is the component itself
        parameters.Add( component );

        // if we should use a reference element to invoke the Create method
        // and parameter length equals 2
        // get the components referenceGID, see if it has been created already
        // use this element as a parameter to invoke Create(Element element)
        if ( useReferenceElement && method.GetParameters().Length == 2 )
        {
          // Get the components reference GID
          System.Reflection.PropertyInfo propertyReferenceGID = type.GetProperty( "referenceGID" );

          // Return if there is no reference GID property
          if ( propertyReferenceGID == null ) return;

          // Get the referene GID string value                    
          string referenceGID = ( string ) propertyReferenceGID.GetValue( component );

          // If the reference has been created already, get 
          // the Element from the document and apply it as parameter two
          if ( MyCreatedElements.ContainsKey( referenceGID ) )
          {
            Element referenceElement = MyRevitDoc.GetElement( MyCreatedElements[ referenceGID ] );
            parameters.Add( referenceElement );
          }
        }

        // encapsulate the extension methods within a logical context
        // (their builder class)
        parameters.Add( builder );

        Element createdElement = null;

        // If the create method exists
        if ( method != null && method.Name.EndsWith( "Create" ) )
        {
          // Invoke the Create Method without parameters
          if ( component is Grevit.Types.Wall )
          {
            createdElement = ( Element ) method.Invoke( component, new object[ ] { component, null, null, builder });
          }
          else
             createdElement = ( Element ) method.Invoke( component, parameters.ToArray() );

          // If the return value is valud set the parameters
          if ( createdElement != null )
          {
            component.SetParameters( createdElement, builder );
            component.StoreGID( createdElement.Id, builder );
          }
        }
      }

      // commit and dispose the transaction
      //transaction.Commit();
      //transaction.Dispose();

    }

  }
}
