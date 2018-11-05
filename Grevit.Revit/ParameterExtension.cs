﻿//
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
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Grevit.Revit
{
  /// <summary>
  /// Extends Grevit Components by Parameter Methods
  /// </summary>
  public static class ParameterExtension
  {
    /// <summary>
    /// Sets Parameter Values
    /// </summary>
    /// <param name="component"></param>
    /// <param name="element">The Element to apply parameters to</param>
    public static void SetParameters( this Grevit.Types.Component component, Element element, SpeckleGrevitBuilder host = null )
    {
      var revitDoc = host == null ? GrevitBuildModel.document : host.RevitDoc;

      if ( element != null )
      {
        if ( component.parameters != null )
        {
          foreach ( Grevit.Types.Parameter componentParameter in component.parameters )
          {
            int iid;
            Autodesk.Revit.DB.Parameter elementParameter = null;

            if ( int.TryParse( componentParameter.name, out iid ) )
            {
              foreach ( Autodesk.Revit.DB.Parameter p in element.Parameters ) if ( p.Id.IntegerValue == iid ) elementParameter = p;
            }
            else elementParameter = element.LookupParameter( componentParameter.name );

            if ( elementParameter != null )
            {
              switch ( elementParameter.StorageType )
              {
                case StorageType.Double:
                  if ( componentParameter.value.GetType() == typeof( double ) ) elementParameter.Set( ( double ) componentParameter.value );
                  break;

                case StorageType.Integer:
                  if ( componentParameter.value.GetType() == typeof( int ) ) elementParameter.Set( ( int ) componentParameter.value );
                  break;

                case StorageType.String:
                  if ( componentParameter.value.GetType() == typeof( string ) ) elementParameter.Set( ( string ) componentParameter.value );
                  break;
                case StorageType.ElementId:
                  if ( componentParameter.value.GetType() == typeof( Grevit.Types.ElementID ) )
                  {
                    Grevit.Types.ElementID grvid = ( Grevit.Types.ElementID ) componentParameter.value;
                    ElementId id = new ElementId( grvid.ID );
                    if ( revitDoc.GetElement( id ) != null ) elementParameter.Set( id );
                  }
                  else if ( componentParameter.value.GetType() == typeof( Grevit.Types.SearchElementID ) )
                  {
                    Grevit.Types.SearchElementID grvid = ( Grevit.Types.SearchElementID ) componentParameter.value;
                    Element e = revitDoc.GetElementByName( grvid.Name );
                    if ( e != null ) elementParameter.Set( e.Id );
                  }

                  break;
              }

            }
          }
        }

        Autodesk.Revit.DB.Parameter grevitIdParameter = element.LookupParameter( "GID" );

        if ( grevitIdParameter == null )
        {
          revitDoc.GrevitAddSharedParameter();
          grevitIdParameter = element.LookupParameter( "GID" );
        }

        if ( grevitIdParameter != null && !grevitIdParameter.IsReadOnly ) grevitIdParameter.Set( component.GID );
      }
    }

    /// <summary>
    /// Store GID in create elements List
    /// </summary>
    /// <param name="component"></param>
    /// <param name="elementId">ElementId of created element</param>
    public static void StoreGID( this Grevit.Types.Component component, Autodesk.Revit.DB.ElementId elementId, SpeckleGrevitBuilder host = null )
    {
      var myCreatedElements = host == null ? GrevitBuildModel.CreatedElements : host.CreatedElements;
      if ( elementId != null
          && component.GID != null
          && !myCreatedElements.ContainsKey( component.GID ) )
        myCreatedElements.Add( component.GID, elementId );
    }
  }
}
