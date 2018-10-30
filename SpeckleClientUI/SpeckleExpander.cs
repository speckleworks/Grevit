using System.Windows;
using System.Windows.Controls;

namespace SpeckleClientUI
{
    public class SpeckleExpander : Expander
    {

        public static DependencyProperty StreamIdProperty =
            DependencyProperty.Register(
                "StreamId",
                typeof(string),
                typeof(SpeckleExpander));

      

        static SpeckleExpander()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SpeckleExpander),
                new FrameworkPropertyMetadata(typeof(SpeckleExpander)));
        }


       public string StreamId
        {
            get { return (string)GetValue(StreamIdProperty); }
            set { SetValue(StreamIdProperty, value); }
        }



    }
}
