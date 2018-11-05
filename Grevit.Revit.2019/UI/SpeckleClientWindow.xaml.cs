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
using SpeckleClientUI;

namespace Grevit.Revit
{
  /// <summary>
  /// Interaction logic for SpeckleClientWindow.xaml
  /// </summary>
  public partial class SpeckleClientWindow : Window
  {
    public ExternalEvent ExtEvent;
    public SpeckleExternalEventHandler ExtHandler;
    public UIApplication UIApplication;
    public ISpeckleHostBuilderGenerator Generator;


    public SpeckleClientWindow( UIApplication app, ExternalEvent e, SpeckleExternalEventHandler h, ISpeckleHostBuilderGenerator gen )
    {
      InitializeComponent();

      UIApplication = app;
      ExtEvent = e;
      ExtHandler = h;
      Generator = gen;

      this.receiver.OnUpdateGlobal += Receiver_OnUpdateGlobal;
      this.receiver.BuildGenerator = gen;
    }

    private void Receiver_OnUpdateGlobal( SpeckleClientUI.Receiver receiver )
    {
      ExtHandler.Receiver = receiver;
      ExtEvent.Raise();
    }

    private void OnClosing( object sender, CancelEventArgs e )
    {
      this.Hide();
      e.Cancel = true;
    }
  }
}
