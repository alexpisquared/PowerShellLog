using System;
using System.Collections.Generic;
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

namespace PowerShellLog
{
  /// <summary>
  /// Interaction logic for Window1.xaml
  /// </summary>
  public partial class Window1 : Window
  {
    public Window1()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {

      System.Windows.Data.CollectionViewSource cmdViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("cmdViewSource")));
      // Load data by setting the CollectionViewSource.Source property:
      // cmdViewSource.Source = [generic data source]
      System.Windows.Data.CollectionViewSource logViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("logViewSource")));
      // Load data by setting the CollectionViewSource.Source property:
      // logViewSource.Source = [generic data source]
      System.Windows.Data.CollectionViewSource usageVwViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("usageVwViewSource")));
      // Load data by setting the CollectionViewSource.Source property:
      // usageVwViewSource.Source = [generic data source]
    }
  }
}
