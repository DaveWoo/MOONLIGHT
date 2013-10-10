namespace XMLParserConversion
{
    using StyleCop.CSharp;
    using XMLParserConversion.Code;

    public class ParserObject<T> : IParserObject
    {
        public ParserObject(T t)
        {
            OriginalT = t;
        }

        public object OriginalT { set; get; }

        public string Name { set; get; }

        public string ProjectName { get; set; }

        public string FileFullPath { set; get; }

        public string FullNamespaceName { set; get; }

        public CodeLocationX Location { set; get; }

        public object Tag { get; set; }

        public object Parent { get; set; }

        public object Element { get; set; }
    }

    public class CodeStructure
    {
        public CodeStructure(IParserObject itemObject, int lineNumber, bool isCommented)
        {
            CsElement element = itemObject.Element as CsElement;
            this.FullPath = itemObject.FileFullPath;
            this.ProjectName = itemObject.ProjectName;
            this.LineNumber = lineNumber;
            this.IsCommented = isCommented;
            if (element == null)
            {
            }
            else if (element.Declaration != null)
            {
                this.MethodName = element.Declaration.Name;
            }
        }

        public string FullPath { get; private set; }

        public string ProjectName { get; private set; }

        public int LineNumber { get; private set; }

        public bool IsCommented { get; private set; }

        public string MethodName { get; private set; }

        //  public string LineCotext { get; private set; }
    }

    public class AppliedRule
    {
        public AppliedRule(ChangeTypes type)
        {
            this.ChangedType = type;
        }

        public ChangeTypes ChangedType { get; private set; }

        public int CheckId { get; set; }

        public CodeStructure CodeDocument { get; set; }

        public RuleStatus RuleStatus { get; set; }

    }

    public enum RuleStatus
    {
        Passed,
        Failed,
        Info,
    }
}