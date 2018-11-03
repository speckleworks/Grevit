using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using Grevit.Types;

namespace Grevit.Revit
{
    /// <summary>
    /// Interaction logic for SpeckleClientWindow.xaml
    /// </summary>
    public partial class SpeckleClientWindow : Window
    {
        public GrevitBuildModel grevit;
        public ExternalEvent ExtEvent;
        public ExtEventHandler ExtHandler;
        public UIApplication UIApplication;


        public SpeckleClientWindow( UIApplication app, ExternalEvent e, ExtEventHandler h)
        {
            InitializeComponent();

            UIApplication = app;
            ExtEvent = e;
            ExtHandler = h;

            //grevit = _grevit;
            this.receiver.OnUpdateGlobal += Receiver_OnUpdateGlobal;
        }

        private void Receiver_OnUpdateGlobal( SpeckleClientUI.Receiver receiver )
        { 
            ExtHandler.Receiver = receiver;
            ExtEvent.Raise();

            //grevit.BuildModel( myCollection );
        }

        private void OnClosing( object sender, CancelEventArgs e )
        {
            this.Hide();
            e.Cancel = true;
        }
    }
}
