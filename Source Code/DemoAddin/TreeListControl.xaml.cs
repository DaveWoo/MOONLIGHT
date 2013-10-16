using System.Windows.Controls;
using System.Windows;

namespace DemoAddin.LoadOnDemand
{
    public partial class TreeList : UserControl
    {
        public TreeList()
        {
            InitializeComponent();
        }
        public TreeView TreeviewControl { get; private set; }

        private void TreeView_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            TreeviewControl = this.TreeViewControl;
            RaiseEvent(new RoutedEventArgs(ItemClickEvent));
        }

        public event RoutedEventHandler TreeView_SelectedChanged
        {
            add { AddHandler(ItemClickEvent, value); }
            remove { RemoveHandler(ItemClickEvent, value); }
        }

        public static readonly RoutedEvent ItemClickEvent = EventManager.RegisterRoutedEvent(
            "TreeView_SelectedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler),
            typeof(TreeList));
    }
}