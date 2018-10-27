using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using SpeckleCore;

namespace SpeckleForm
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    public ViewModel model;
    public SpeckleApiClient mySpeckleClient;

    public MainWindow( Autodesk.Revit.UI.UIApplication application )
    {
      InitializeComponent();
      this.DataContext = model = new ViewModel();
      model.PropertyChanged += Model_PropertyChanged;

      mySpeckleClient = new SpeckleApiClient( "https://hestia.speckle.works/api/v1" ); // LOL 
    }

    private void Model_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      Debug.WriteLine( e.PropertyName + " changed" );
    }

    private void Button_Click( object sender, RoutedEventArgs e )
    {
      Debug.WriteLine( this.model.myStreamId );
      Debug.WriteLine(this.StreamIdInput.Text);
    }
  }

  public class ViewModel : INotifyPropertyChanged
  {
    private string _myStreamId;

    public string myStreamId { get => _myStreamId; set { _myStreamId = value; NotifyPropertyChanged( "myStreamId" ); } }

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged( String info )
    {
      PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( info ) );
    }
  }


}
