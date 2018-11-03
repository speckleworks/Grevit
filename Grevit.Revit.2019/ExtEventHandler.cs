using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using SpeckleClientUI;

namespace Grevit.Revit
{
    public class ExtEventHandler : IExternalEventHandler
    {
        public Receiver Receiver;

        /// <summary>
        /// Where the speckle build and bake happens
        /// </summary>
        /// <param name="app"></param>
        public void Execute( UIApplication app )
        {
            var pizza = SpeckleCore.Converter.Deserialise(Receiver.Stream.Objects);

            var grevit = new GrevitBuildModel( app.ActiveUIDocument.Document );

            var units = (( string ) Receiver.Stream.BaseProperties.units).ToLower();

            // TODO: Check

            switch (units) {
                case "kilometers":
                    GrevitBuildModel.Scale = 3.2808399 * 1000;
                    break;
                case "meters":
                    GrevitBuildModel.Scale = 3.2808399;
                    break;
                case "centimeters":
                    GrevitBuildModel.Scale = 0.032808399;
                    break;
                case "millimiters":
                    GrevitBuildModel.Scale = 0.0032808399;
                    break;
                case "miles":
                    GrevitBuildModel.Scale = 5280;
                    break;
                case "feet":
                    GrevitBuildModel.Scale = 1;
                    break;
                case "inches":
                    GrevitBuildModel.Scale = 0.0833333;
                    break;

            };

            grevit.BuildModel( new Grevit.Types.ComponentCollection()
            {
                Items = pizza.Where( xx => xx is Grevit.Types.Component ).Select( xxx => xxx as Grevit.Types.Component ).ToList()
            });



            var x = "Hello World";
        }

        public string GetName( )
        {
            return "Speckle Bake";
        }
    }
}
