namespace XinYu.XSD2Code
{
    using StyleCop.CSharp;
    using XinYu.XSD2Code.Code;
    using XinYu.SOMDiff;

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

        public int Key
        {
            get
            {
                int key = -1;
                if (Location == null)
                {
                    key = string.Format("{0}:{1}", FileFullPath, -1).GetHashCode();
                }
                else
                {
                    key = string.Format("{0}:{1}", FileFullPath, Location.LineNumber).GetHashCode();
                }
                return key;

            }
        }
    }

    public class CodeStructure
    {
        public CodeStructure()
        { }

        public CodeStructure(IParserObject itemObject, int lineNumber)
        {
            CsElement element = null;
            if (itemObject != null)
            {
                element = itemObject.Element as CsElement;
                this.FullPath = itemObject.FileFullPath;
                this.ProjectName = itemObject.ProjectName;
            }

            this.LineNumber = lineNumber;
            if (element == null)
            {
            }
            else if (element.Declaration != null)
            {
                this.MethodName = element.Declaration.Name;
            }
        }

        public string FullPath { get; internal set; }

        public string ProjectName { get; internal set; }

        public int LineNumber { get; internal set; }

        // public bool IsCommented { get; private set; }

        public string MethodName { get; internal set; }

        //public string LineCotext { get; private set; }
    }

    public class AppliedRule
    {
        public AppliedRule(ChangeTypes type)
        {
            this.ChangedType = type;
        }

        public ChangeTypes ChangedType { get; private set; }

        public int CheckId { get; set; }

        public CodeStructure CodeDocument
        {
            get
            {
                CodeStructure codeStructure = null;
                if (ParserObject != null)
                {
                    codeStructure = ParserObject.ToCodeStructure();
                }
                return codeStructure;
            }
        }

        public RuleStatus RuleStatus { get; set; }

        public object Tag { get; set; }

        public IParserObject ParserObject { get; set; }

        public int Key
        {
            get
            {
                int key = -1;
                if (ParserObject == null)
                {
                    key = -1;
                }
                else
                {
                    if (ParserObject.Location == null)
                    {
                        key = string.Format("{0}:{1}", ParserObject.FileFullPath, -1).GetHashCode();
                    }
                    else
                    {
                        key = string.Format("{0}:{1}", ParserObject.FileFullPath, ParserObject.Location.LineNumber).GetHashCode();
                    }
                }
                return key;

            }
        }


    }

    public enum RuleStatus
    {
        Passed,
        Failed,
        Info,
    }
}