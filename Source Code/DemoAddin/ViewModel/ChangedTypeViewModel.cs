namespace DemoAddin.LoadOnDemand
{
    using XinYu.XSD2Code;

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

        public int ChangedTypeCount
        {
            get { return DemoAddin.DataModel.GetRuleDetails(changeType).Count; }
        }

        protected override void LoadChildren()
        {
            foreach (AppliedRule state in DemoAddin.DataModel.GetRuleDetails(changeType))
                base.Children.Add(new RuleViewModel(state, this));
        }
    }
}