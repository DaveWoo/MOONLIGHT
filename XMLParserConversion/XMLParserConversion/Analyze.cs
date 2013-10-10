namespace XMLParserConversion
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using StyleCop;
    using StyleCop.CSharp;
    using Xin.SOMDiff;

    public partial class Analyze
    {
        public void Run(List<string> originalProxy, List<string> changedProxy)
        {
            try
            {
                #region Step1: SOMDiff invocation
                // Get the dependency graph
                string sourcepath = @"D:\GitHub\MOONLIGHT\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Request";
                string changepath = @"D:\GitHub\MOONLIGHT\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\ChangedXSD\Request";

                // Diff a specific pair of XSD files
                string sourefile = @"D:\GitHub\MOONLIGHT\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\OriginalXSD\Request\Calendar.xsd";
                string changefile = @"D:\GitHub\MOONLIGHT\TSEA\ConsoleApplication1\ConsoleApplication1\Resources\ChangedXSD\Request\cal.xsd";

                SOMDiffFiles(sourcepath, changepath, sourefile, changefile);
                List<MismatchedPair> result = SOMDiff.Result;



                #endregion


                #region Step 1 Anaylse XSD Source code

                #endregion Step 1 Anaylse XSD Source code

                #region Step 2 Anaylse destination Code

                StyleCopCore core = CreateStyleCopCore();

                // Get all *.csproj file from specific *.sln file.
                List<string> csprojFileList = this.GetCsprojFiles(this.slnFilePath);
                // Get all *.cs file from specific *.csproj file
                if (Common.CSFileList == null)
                {
                    Common.CSFileList = this.FindFAllCSFiles(this.slnFilePath, csprojFileList);
                }

                // Initial log info
                Common.OutputContext = new StringBuilder();

                // Generate final date structure
                this.GenerateParserObjectListAndAddThemIntoContextXMLParserList(core);

                #endregion Step 2 Anaylse destination Code

                #region Load Proxy
                List<IParserObject> originalObject = GenerateParserObjectList(core, originalProxy);
                List<IParserObject> changedObject = GenerateParserObjectList(core, changedProxy);
                #endregion

                #region Step 3 Calculate/Validate/Compatibility impacted destination code

                if (Common.ContextXMLParserList == null)
                {
                    throw new ArgumentNullException("contextXMLParserList is null");
                }

                #region Signature/Compatibility validation

                string classXPath = "ExceptionsException";
                string expressionXpath = "Reminder";
                List<IParserObject> IParserObjectListExpressionsNeedToUpdate = this.SignatureValidation(classXPath: classXPath, expressionXpath: expressionXpath);

                // Sort array
                List<IParserObject> sortedCodeStructure =
                    IParserObjectListExpressionsNeedToUpdate.OrderBy(c => c.FileFullPath).ThenBy(c => c.Location.LineNumber).ToList();

                Common.OutputContext.Append("Changed: 1");
                Common.OutputContext.Append(Environment.NewLine);
                Common.OutputContext.Append("Changed: 2");
                Common.OutputContext.Append(Environment.NewLine);
                Common.OutputContext.Append("Changed: 3");
                Common.OutputContext.Append(Environment.NewLine);
                Common.OutputContext.Append("Changed: 4");
                Common.OutputContext.Append(Environment.NewLine);
                #endregion Signature validation

                #endregion Step 3 Calculate/Validate impacted destination code

                #region Step 4 Update destination code with XSD change

                ChangeTypes typeChange = ChangeTypes.TypeChange_Update;

                #region Replace "reminder"

                ReplaceCodeWithChangeType(sortedCodeStructure, typeChange);

                #endregion Replace "reminder"

                #region Add new filed "Specified"

                //  AddNewCodeSegmentWithTypeChange(sortedCodeStructure, typeChange);

                #endregion Add new filed "Specified"

                #endregion Step 4 Update destination code with XSD change

            }
            catch
            {
                throw new Exception();
            }
        }

        private void SOMDiffFiles(string sourcepath, string changepath, string sourefile, string changefile)
        {
            if (string.IsNullOrWhiteSpace(sourcepath))
            {
                throw new ArgumentNullException(sourcepath);
            }

            if (string.IsNullOrWhiteSpace(changepath))
            {
                throw new ArgumentNullException(changepath);
            }
            if (string.IsNullOrWhiteSpace(sourefile))
            {
                throw new ArgumentNullException(sourefile);
            }
            if (string.IsNullOrWhiteSpace(changefile))
            {
                throw new ArgumentNullException(changefile);
            }

            SOMDiff sdiff = new SOMDiff();
            sdiff.ParseSchemaDependency(sourcepath);
            sdiff.ParseSchemaDependency(changepath);

            sdiff.DiffSchemas(sourefile, changefile);
        }

        private List<IParserObject> SignatureValidation(string classXPath, string expressionXpath)
        {
            if (string.IsNullOrWhiteSpace(classXPath))
            {
                throw new ArgumentNullException(classXPath);
            }

            List<IParserObject> validateListClass = Common.ContextXMLParserList.FindAll(p => p.OriginalT is Class);
            List<IParserObject> validateListVariable = Common.ContextXMLParserList.FindAll(p => p.OriginalT is Variable);
            List<IParserObject> validateListExpression = Common.ContextXMLParserList.FindAll(p => p.OriginalT is StyleCop.CSharp.Expression);
            List<IParserObject> validateListUsingDirective = Common.ContextXMLParserList.FindAll(p => p.OriginalT is UsingDirective);

            List<IParserObject> validateListExpressionsNeedToUpdate = new List<IParserObject>();

            #region Target Type

            // We will use ABC.ExceptionsException instead of ExceptionsException
            List<IParserObject> TargetTypeList = validateListVariable.FindAll(p => p.Tag.ToString().Contains(classXPath));

            #endregion Target Type

            foreach (var item in TargetTypeList)
            {
                // Start to match variable and expression; Find all related expression
                // 1. Start with same variable
                // 2. Should exist in same method/namespace
                // 3. In same file
                List<IParserObject> validateListExpressionsNames = validateListExpression.FindAll(
                    p => p.Name == item.Name
                        // && p.FileFullPath == item.FileFullPath
                        && p.FullNamespaceName == item.FullNamespaceName);
                if (validateListExpressionsNames.Count == 0)
                {
                    continue;
                }

                // If no related expression exist, just return related variable
                if (string.IsNullOrWhiteSpace(expressionXpath))
                {
                    validateListExpressionsNames.ForEach(p => validateListExpressionsNeedToUpdate.Add(p));
                    continue;
                }

                #region Target Name

                // We will use ExceptionsException.Reminder instead of Reminder
                List<IParserObject> validateListExpressionsFindList = validateListExpressionsNames.FindAll(p => (p.Tag as StyleCop.CSharp.Expression).Text.IndexOf(expressionXpath) > 0);
                if (validateListExpressionsFindList.Count == 0)
                {
                    continue;
                }

                foreach (var singleFindCode in validateListExpressionsFindList)
                {
                    if (!validateListExpressionsNeedToUpdate.Contains(singleFindCode))
                    {
                        validateListExpressionsNeedToUpdate.Add(singleFindCode);
                    }
                }

                #endregion Target Name
            }

            #region Xpath

            #endregion Xpath

            return validateListExpressionsNeedToUpdate;
        }

        private static int CompareByNumber(IParserObject x, IParserObject y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    // If x is null and y is null, they're
                    // equal.
                    return 0;
                }
                else
                {
                    // If x is null and y is not null, y
                    // is greater.
                    return -1;
                }
            }
            else
            {
                // If x is not null...
                //
                if (y == null)
                // ...and y is null, x is greater.
                {
                    return 1;
                }
                else
                {
                    // ...and y is not null, compare the
                    // lengths of the two strings.
                    //
                    int retval = x.Location.LineNumber.CompareTo(y.Location.LineNumber);

                    return retval;
                }
            }
        }

        /// <summary>
        /// Checks the case of element names within the given document.
        /// </summary>
        /// <param name="document">
        /// The document to check.
        /// </param>
        public void AnalyseDocument(CodeFile scCode, StyleCopCore core, List<IParserObject> contextXMLParserList)
        {
            CodeDocument document = GetCodeDocument(scCode);
            Param.RequireNotNull(document, "document");

            CsDocument csdocument = (CsDocument)document;

            if (csdocument.RootElement != null && !csdocument.RootElement.Generated)
            {
                // Get Class/Field.. definition
                this.ProcessElement(csdocument.RootElement, false, contextXMLParserList);

                ReadabilityRules rule = new ReadabilityRules(core);
                // Get the expressions.
                rule.CheckClassMemberRulesForElements(csdocument.RootElement, null, null);
            }
        }

        public void GetDocumentElements(CodeFile scCode, StyleCopCore core, List<IParserObject> contextXMLParserList)
        {
            CodeDocument document = GetCodeDocument(scCode);
            Param.RequireNotNull(document, "document");

            CsDocument csdocument = (CsDocument)document;

            if (csdocument.RootElement != null)
            {
                // Get Class/Field.. definition
                this.ProcessElement(csdocument.RootElement, false, contextXMLParserList);
            }
        }

        private CodeDocument GetCodeDocument(CodeFile scCode)
        {
            CodeDocument scDoc = null;
            scCode.Parser.PreParse();
            scCode.Parser.ParseFile(scCode, 0, ref scDoc);

            return scDoc;
        }

        #region From StyleCop code

        /// <summary>
        /// Processes one element and its children.
        /// </summary>
        /// <param name="element">
        /// The element to process.
        /// </param>
        /// <param name="validPrefixes">
        /// The list of valid prefixes for this element.
        /// </param>
        /// <param name="nativeMethods">
        /// Indicates whether the element is within a NativeMethods class.
        /// </param>
        /// <returns>
        /// Returns false if the analyzer should quit.
        /// </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Minimizing refactoring before release.")]
        private bool ProcessElement(CsElement element, bool nativeMethods, List<IParserObject> contextXMLParserList)
        {
            Param.AssertNotNull(element, "element");
            Param.Ignore(nativeMethods);

            switch (element.ElementType)
            {
                case ElementType.UsingDirective:
                    if (!nativeMethods)
                    {
                        IParserObject itemElement = Common.ConvertStyleCopTypeToLocal<UsingDirective>(element as UsingDirective, element);

                        if (itemElement != null)
                        {
                            contextXMLParserList.Add(itemElement);
                        }
                    }
                    break;

                case ElementType.Namespace:
                    if (!nativeMethods)
                    {
                        string[] namespaceParts = element.Declaration.Name.Split('.');
                        foreach (string namespacePart in namespaceParts)
                        {
                        }
                    }

                    break;

                case ElementType.Class:
                    IParserObject itemElementClass = Common.ConvertStyleCopTypeToLocal<Class>(element as Class, element);
                    if (itemElementClass != null)
                    {
                        contextXMLParserList.Add(itemElementClass);
                    }
                    break;

                case ElementType.Enum:
                case ElementType.Struct:
                case ElementType.Delegate:
                case ElementType.Property:
                    if (!nativeMethods)
                    {
                        StyleCop.CSharp.Property item = element as StyleCop.CSharp.Property;
                        IParserObject itemElement = Common.ConvertStyleCopTypeToLocal<StyleCop.CSharp.Property>(item, element);
                        if (itemElement != null)
                        {
                            contextXMLParserList.Add(itemElement);
                        }
                    }

                    break;

                case ElementType.Event:
                    if (!nativeMethods)
                    {
                        foreach (EventDeclaratorExpression declarator in ((Event)element).Declarators)
                        {
                        }
                    }

                    break;

                case ElementType.Method:
                    if (!nativeMethods && !element.Declaration.Name.StartsWith("operator", StringComparison.Ordinal) && element.Declaration.Name != "foreach")
                    {
                        IParserObject itemElement = Common.ConvertStyleCopTypeToLocal<Method>(element as Method, element);
                        if (itemElement != null)
                        {
                            contextXMLParserList.Add(itemElement);
                        }
                    }

                    break;

                case ElementType.Interface:
                    if (element.Declaration.Name.Length < 1 || element.Declaration.Name[0] != 'I')
                    {
                    }

                    break;

                case ElementType.Field:
                    if (!nativeMethods)
                    {
                        //this.CheckFieldUnderscores(element);
                        IParserObject itemElement = Common.ConvertStyleCopTypeToLocal<StyleCop.CSharp.Field>(element as Field, element);
                        if (itemElement != null)
                        {
                            contextXMLParserList.Add(itemElement);
                        }
                    }

                    break;

                default:
                    break;
            }

            if (!nativeMethods && (element.ElementType == ElementType.Class || element.ElementType == ElementType.Struct)
                && element.Declaration.Name.EndsWith("NativeMethods", StringComparison.Ordinal))
            {
                nativeMethods = true;
            }

            bool doneAccessor = false;
            foreach (CsElement child in element.ChildElements)
            {
                if ((element.ElementType == ElementType.Indexer && !doneAccessor) || element.ElementType != ElementType.Indexer)
                {
                    if (child.ElementType == ElementType.Accessor)
                    {
                        doneAccessor = true;
                    }

                    if (!this.ProcessElement(child, nativeMethods, contextXMLParserList))
                    {
                        return false;
                    }
                }
            }

            if (!nativeMethods)
            {
                this.ProcessStatementContainer(element, contextXMLParserList);
            }

            return true;
        }

        /// <summary>
        /// Processes the given expression.
        /// </summary>
        /// <param name="expression">
        /// The expression to process.
        /// </param>
        /// <param name="element">
        /// The parent element.
        /// </param>
        /// <param name="validPrefixes">
        /// The list of acceptable Hungarian-type prefixes.
        /// </param>
        private void ProcessExpression(StyleCop.CSharp.Expression expression, CsElement element, List<IParserObject> contextXMLParserList)
        {
            Param.AssertNotNull(expression, "expression");
            Param.AssertNotNull(element, "element");

            // Check the type of the expression.
            if (expression.ExpressionType == ExpressionType.AnonymousMethod)
            {
                AnonymousMethodExpression anonymousMethod = (AnonymousMethodExpression)expression;

                // Check the anonymous method's variables.
                if (anonymousMethod.Variables != null)
                {
                    foreach (Variable variable in anonymousMethod.Variables)
                    {
                        IParserObject itemElement = Common.ConvertStyleCopTypeToLocal<StyleCop.CSharp.Variable>(variable, element);
                        if (itemElement != null)
                        {
                            contextXMLParserList.Add(itemElement);
                        }
                    }

                    // Check the statements under the anonymous method.
                    foreach (Statement statement in anonymousMethod.ChildStatements)
                    {
                        this.ProcessStatement(statement, element, contextXMLParserList);
                    }
                }
            }

            // Check the child expressions under this expression.
            foreach (StyleCop.CSharp.Expression childExpression in expression.ChildExpressions)
            {
                this.ProcessExpression(childExpression, element, contextXMLParserList);
            }
        }

        /// <summary>
        /// Processes the given statement.
        /// </summary>
        /// <param name="statement">
        /// The statement to process.
        /// </param>
        /// <param name="element">
        /// The parent element.
        /// </param>
        /// <param name="validPrefixes">
        /// The list of acceptable Hungarian-type prefixes.
        /// </param>
        private void ProcessStatement(Statement statement, CsElement element, List<IParserObject> contextXMLParserList)
        {
            Param.AssertNotNull(statement, "statement");
            Param.AssertNotNull(element, "element");

            // Check the statement's variables.
            if (statement.Variables != null)
            {
                foreach (Variable variable in statement.Variables)
                {
                    IParserObject itemElement = Common.ConvertStyleCopTypeToLocal<StyleCop.CSharp.Variable>(variable, element);
                    if (itemElement != null)
                    {
                        contextXMLParserList.Add(itemElement);
                    }
                }
            }

            // Check the expressions under this statement.
            foreach (StyleCop.CSharp.Expression expression in statement.ChildExpressions)
            {
                this.ProcessExpression(expression, element, contextXMLParserList);
            }

            // Check each of the statements under this statement.
            foreach (Statement childStatement in statement.ChildStatements)
            {
                this.ProcessStatement(childStatement, element, contextXMLParserList);
            }
        }

        /// <summary>
        /// Processes the given statement container.
        /// </summary>
        /// <param name="element">
        /// The statement container element to process.
        /// </param>
        /// <param name="validPrefixes">
        /// The list of acceptable Hungarian-type prefixes.
        /// </param>
        private void ProcessStatementContainer(CsElement element, List<IParserObject> contextXMLParserList)
        {
            Param.AssertNotNull(element, "element");

            // Check the statement container's variables.
            if (element.Variables != null)
            {
                foreach (Variable variable in element.Variables)
                {
                    if (!variable.Generated)
                    {
                        IParserObject itemElement = Common.ConvertStyleCopTypeToLocal<StyleCop.CSharp.Variable>(variable, element);
                        if (itemElement != null)
                        {
                            contextXMLParserList.Add(itemElement);
                        }
                    }
                }
            }

            // Check each of the statements under this container.
            foreach (Statement statement in element.ChildStatements)
            {
                this.ProcessStatement(statement, element, contextXMLParserList);
            }
        }

        #endregion From StyleCop code

        #region Get code by build

        private void AddNewCodeSegmentWithTypeChange(List<IParserObject> IParserObjectListExpressionsNeedToUpdate, ChangeTypes changeType)
        {
            if (IParserObjectListExpressionsNeedToUpdate.Count > 0)
            {
                foreach (var item in IParserObjectListExpressionsNeedToUpdate)
                {
                    StyleCop.CSharp.Expression expression = item.Tag as StyleCop.CSharp.Expression;
                    string oldStr = string.Empty;
                    string newStr = string.Empty;
                    if (expression != null)
                    {
                        newStr = item.Name + ".Specified = true;";
                    }

                    if (string.IsNullOrWhiteSpace(newStr))
                    {
                        throw new ArgumentNullException(newStr);
                    }

                    this.AddExpression(newStr, item, changeType);
                }
            }
        }

        private void ReplaceCodeWithChangeType(List<IParserObject> sortedCodeStructure, ChangeTypes typeChange)
        {
            if (sortedCodeStructure.Count() > 0)
            {
                int helpCount = 0;
                List<string> helperList = new List<string>();

                for (int i = 0; i < sortedCodeStructure.Count(); i++)
                {
                    // Count every code segment line number for commented code
                    IParserObject item = sortedCodeStructure[i];
                    if (!helperList.Contains(item.FileFullPath))
                    {
                        helperList.Add(item.FileFullPath);
                        helpCount = 0;
                    }
                    else
                    {
                        helpCount++;
                    }

                    StyleCop.CSharp.Expression expression = item.Tag as StyleCop.CSharp.Expression;
                    string oldStr = string.Empty;
                    string newStr = string.Empty;
                    if (expression != null)
                    {
                        oldStr = expression.Text;
                        string[] expressionList = expression.Text.Split(new char[] { '=' });
                        if (expressionList.Count() > 1)
                        {
                            string tempStr = expressionList[1].TrimX();
                            newStr = expressionList[0] + "=" + Convert.ToUInt32(tempStr).ToString();
                        }
                    }
                    if (string.IsNullOrWhiteSpace(oldStr))
                    {
                        throw new ArgumentNullException(oldStr);
                    }

                    if (string.IsNullOrWhiteSpace(newStr))
                    {
                        throw new ArgumentNullException(newStr);
                    }
                    this.ReplaceExpression(oldStr, newStr, item, typeChange, helpCount);
                }
            }
        }

        public List<string> GetCsprojFiles(string slnFilePath)
        {
            string[] fileContentList = File.ReadAllLines(slnFilePath);
            List<string> projectItemList = new List<string>();
            foreach (var item in fileContentList)
            {
                if (item.Contains(".csproj"))
                {
                    var projectItem = item.Split(new char[] { ',' }).ToList();
                    string projectItemPath = projectItem.Find(p => p.Contains(".csproj"));
                    if (!string.IsNullOrWhiteSpace(projectItemPath))
                    {
                        projectItemList.Add(projectItemPath.TrimX());
                    }
                }
            }

            return projectItemList;
        }

        private bool ReplaceExpression(string oldName, string newName, IParserObject itemObject, ChangeTypes changeType, int timeCount)
        {
            if (itemObject == null)
                throw new ArgumentNullException("itemObject is null");

            if (itemObject.Location == null)
                throw new ArgumentNullException("itemObject.Location is null");

            bool isSucceed = false;
            itemObject.Location.LineNumber = itemObject.Location.LineNumber + timeCount;
            try
            {
                List<string> files = File.ReadAllLines(itemObject.FileFullPath).ToList();

                #region Replace code

                if (files != null && files.Count > itemObject.Location.LineNumber)
                {
                    files[itemObject.Location.LineNumber - 1] =
                        ("// Original code: "
                        + files[itemObject.Location.LineNumber - 1].Trim()).PadLeft(itemObject.Location.StartPoint.IndexOnLine + 42)
                        + Environment.NewLine
                        + files[itemObject.Location.LineNumber - 1].Replace(oldName, newName);
                    File.WriteAllLines(itemObject.FileFullPath, files);
                }

                #endregion Replace code

                isSucceed = true;

                // Comments code
                CodeStructure codeSturcture1 = new CodeStructure(itemObject, itemObject.Location.LineNumber, true);
                AppliedRule appliedRule = new AppliedRule(changeType);
                appliedRule.CodeDocument = codeSturcture1;

                // Replace code
                CodeStructure codeSturcture2 = new CodeStructure(itemObject, itemObject.Location.LineNumber + 1, false);

                appliedRule.CodeDocument = codeSturcture2;
                appliedRule.RuleStatus = RuleStatus.Passed;
                Common.AppliedRuleList.Add(appliedRule);
            }
            catch (Exception)
            {
                isSucceed = false;
            }
            return isSucceed;
        }

        private bool AddExpression(string newField, IParserObject itemObject, ChangeTypes changeType)
        {
            if (itemObject == null)
                throw new ArgumentNullException("itemObject is null");

            if (itemObject.Location == null)
                throw new ArgumentNullException("itemObject.Location is null");
            bool isSucceed = false;

            try
            {
                List<string> files = File.ReadAllLines(itemObject.FileFullPath).ToList();
                if (files != null && files.Count > itemObject.Location.LineNumber)
                {
                    files.Insert(itemObject.Location.LineNumber, newField);
                    File.WriteAllLines(itemObject.FileFullPath, files);
                }
                isSucceed = true;
                // Replace code
                CodeStructure codeSturcture2 = new CodeStructure(itemObject, itemObject.Location.LineNumber, false);
                AppliedRule appliedRule = new AppliedRule(changeType);
                appliedRule.CodeDocument = codeSturcture2;
                Common.AppliedRuleList.Add(appliedRule);
            }
            catch (Exception)
            {
                isSucceed = false;
            }

            return isSucceed;
        }

        public List<string> FindFallCSFiles(List<string> csprojFileFullPath)
        {
            List<string> allCSFile = new List<string>();

            if (csprojFileFullPath == null)
            {
                throw new ArgumentException("csprojFileFullPath is null.");
            }

            foreach (var item in csprojFileFullPath)
            {
                string fileFuleName = Path.Combine(Path.GetDirectoryName(this.slnFilePath), item);
                var project = new Microsoft.Build.Evaluation.Project(fileFuleName);
                foreach (var file in project.GetItems("Compile"))
                {
                    string csFullpath = Path.Combine(Path.GetDirectoryName(fileFuleName), file.Xml.Include);
                    allCSFile.Add(csFullpath);
                }
            }
            return allCSFile;
        }

        public Dictionary<string, List<string>> FindFAllCSFiles(string fileSlnPath, List<string> csprojFileFullPath)
        {
            List<string> allCSFile = null;
            Dictionary<string, List<string>> ProjectInfoList = new Dictionary<string, List<string>>();

            if (csprojFileFullPath == null)
            {
                throw new ArgumentException("csprojFileFullPath is null.");
            }

            foreach (var item in csprojFileFullPath)
            {
                string fileFuleName = Path.Combine(Path.GetDirectoryName(fileSlnPath), item);
                try
                {
                    var project = new Microsoft.Build.Evaluation.Project(fileFuleName);
                    allCSFile = new List<string>();
                    foreach (var file in project.GetItems("Compile"))
                    {
                        string csFullpath = Path.Combine(Path.GetDirectoryName(fileFuleName), file.Xml.Include);
                        allCSFile.Add(csFullpath);
                    }
                    ProjectInfoList.Add(Path.GetFileNameWithoutExtension(item), allCSFile);
                }
                catch
                {
                    throw new Exception();
                }
            }
            return ProjectInfoList;
        }

        private void GenerateParserObjectListAndAddThemIntoContextXMLParserList(StyleCopCore core)
        {
            CodeProject codeProject = new CodeProject(
                1,
                " project.FullName",
                new StyleCop.Configuration(new String[] { "XMLParserConversion" }));

            CodeFile scCode = null;
            if (Common.CSFileList == null)
            {
                throw new Exception("No related CS found");
            }
            Common.ContextXMLParserList.Clear();
            ;
            foreach (var project in Common.CSFileList.Values)
            {
                foreach (string itemProjectItem in project)
                {
                    scCode = new CodeFile(itemProjectItem, codeProject, core.Parsers.ElementAt(0));
                    this.AnalyseDocument(scCode, core, Common.ContextXMLParserList);
                }
            }
        }

        private List<IParserObject> GenerateParserObjectList(StyleCopCore core, List<string> csFiles)
        {
            CodeProject codeProject = new CodeProject(
                1,
                " project.FullName",
                new StyleCop.Configuration(new String[] { "XMLParserConversion" }));

            CodeFile scCode = null;
            if (csFiles == null)
            {
                throw new Exception("No related CS found");
            }
            List<IParserObject> contextXMLParserList = new List<IParserObject>();

            foreach (string csFile in csFiles)
            {
                scCode = new CodeFile(csFile, codeProject, core.Parsers.ElementAt(0));
                this.GetDocumentElements(scCode, core, contextXMLParserList);
            }

            return contextXMLParserList;
        }


        #endregion Get code by build
    }
}