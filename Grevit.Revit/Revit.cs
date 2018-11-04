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
}


