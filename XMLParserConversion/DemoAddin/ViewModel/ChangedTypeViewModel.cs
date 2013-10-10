namespace DemoAddin.LoadOnDemand
{
    using XMLParserConversion;

    public class ChangedTypeViewModel : TreeViewItemViewModel
    {
        string changeType = null;

        public ChangedTypeViewModel(string p, ProtocolViewModel parent)
            : base(parent, true)
        {
            changeType = p;
        }

        public string ChangedType
        {
            get { return changeType; }
        }

        protected override void LoadChildren()
        {
            foreach (AppliedRule state in DemoAddin.Common.GetRuleDetails(changeType))
                base.Children.Add(new RuleViewModel(state, this));
        }
    }
}