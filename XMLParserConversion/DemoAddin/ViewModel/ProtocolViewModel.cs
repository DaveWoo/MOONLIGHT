namespace DemoAddin.LoadOnDemand
{
    public class ProtocolViewModel : TreeViewItemViewModel
    {
        string name = null;
        public ProtocolViewModel(string level1)
            : base(null, true)
        {
            name = level1;
        }

        public string ProtocolName
        {
            get { return name; }
        }

        protected override void LoadChildren()
        {
            foreach (string state in Common.GetChangedTypes())
                base.Children.Add(new ChangedTypeViewModel(state, this));
        }
    }
}