namespace DemoAddin.LoadOnDemand
{
    public class ProtocolViewModel : TreeViewItemViewModel
    {
        private string name = null;
        private string spanTime = null;

        public ProtocolViewModel(string level1, string spanTime)
            : base(null, true)
        {
            this.name = level1;
            this.spanTime = spanTime;
        }

        public string ProtocolName
        {
            get { return this.name; }
        }
        public string SpanTime
        {
            get { return "Elapsed time: " + this.spanTime; }
        }

        protected override void LoadChildren()
        {
            foreach (string state in DataModel.GetChangedTypes())
                base.Children.Add(new ChangedTypeViewModel(state, this));
        }
    }
}