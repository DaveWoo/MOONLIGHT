namespace DemoAddin.LoadOnDemand
{
    using XinYu.XSD2Code;

    public class RuleViewModel : TreeViewItemViewModel
    {
        private AppliedRule rule = null;

        public RuleViewModel(AppliedRule rule, ChangedTypeViewModel parent)
            : base(parent, false)
        {
            this.rule = rule;
        }

        public AppliedRule Rule
        {
            get
            {
                return this.rule;
            }
        }

        public string FullPath
        {
            get
            {
                return this.rule.CodeDocument == null ? string.Empty : this.rule.CodeDocument.FullPath;
            }
        }

        public int LineNumber
        {
            get
            {
                return this.rule.CodeDocument.LineNumber;
            }
        }

        public string RuleDetail
        {
            get
            {
                string detail = string.Empty;
                switch (this.rule.RuleStatus)
                {
                    case RuleStatus.Passed:
                        detail = string.Format("Line {0} in {1}", this.rule.CodeDocument.LineNumber, this.rule.CodeDocument.MethodName);
                        break;

                    case RuleStatus.Failed:
                        detail = string.Format("{0}", this.rule.Tag.ToString());
                        break;

                    case RuleStatus.Info:
                        detail = string.Format("{0}", this.rule.Tag.ToString());
                        break;

                    default:
                        break;
                }
                return detail;
            }
        }

        public string ImgSrc
        {
            get
            {
                string path = string.Empty;
                switch (this.rule.RuleStatus)
                {
                    case RuleStatus.Passed:
                        path = @" Images\109_AllAnnotations_Default_16x16_72.png";
                        break;

                    case RuleStatus.Failed:
                        path = @" Images\109_AllAnnotations_Error_16x16_72.png";
                        break;

                    case RuleStatus.Info:
                        path = @" Images\pending_request_16x16_72.png";
                        break;

                    default:
                        break;
                }
                return path;
            }
        }
    }
}