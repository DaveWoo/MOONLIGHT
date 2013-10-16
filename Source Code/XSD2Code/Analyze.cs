namespace XinYu.XSD2Code
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using StyleCop;
    using StyleCop.CSharp;
    using XinYu.SOMDiff;

    public partial class Analyze
    {
        public void Run(string originalXSDPath, string changedXSDPath, List<MismatchedPair> result)
        {
            try
            {
                #region Step 1: Generate proxy cs file

                string originalProxyNamespace = "TSEA.Original.Response";
                string changedProxyNamespace = "TSEA.Changed.Response";
                string originalXSDProxy = this.XSD2Proxy(Path.GetDirectoryName(originalXSDPath), originalProxyNamespace);
                string changedXSDProxy = this.XSD2Proxy(Path.GetDirectoryName(changedXSDPath), changedProxyNamespace);

                #endregion Step 2: Generate proxy cs file

                #region Step 2: Load Proxy and Parse Proxy code

                StyleCopCore core = CreateStyleCopCore();

                List<string> originalProxyFiles = new List<string>();
                List<string> changedProxyFiles = new List<string>();
                originalProxyFiles.Add(Path.Combine(Path.GetDirectoryName(originalXSDPath), originalXSDProxy));
                changedProxyFiles.Add(Path.Combine(Path.GetDirectoryName(changedXSDPath), changedXSDProxy));
                List<IParserObject> originalObject = GenerateParserObjectList(core, originalProxyFiles);
                List<IParserObject> changedObject = GenerateParserObjectList(core, changedProxyFiles);

                #endregion Step 3: Load Proxy and Parse Proxy code

                #region Step 3: Anaylse destination Code

                // Get all *.csproj file from specific *.sln file
                List<string> csprojFileList = this.GetCsprojFiles(this.slnFilePath);
                // Get all *.cs file from specific *.csproj file
                if (Common.CSFileList == null)
                {
                    Common.CSFileList = this.FindFAllCSFiles(this.slnFilePath, csprojFileList);
                }

                // Initial log info
                Common.OutputContext = new StringBuilder();

                // Generate final data
                //if (Common.ContextXMLParserList != null && Common.ContextXMLParserList.Count == 0)
                //{
                //    this.GenerateParserObjectListAndAddThemIntoContextXMLParserList(core);
                //}
                this.GenerateParserObjectListAndAddThemIntoContextXMLParserList(core);
                #endregion Step 4: Anaylse destination Code

                #region Step 4: Catrgory diff result
                if (result == null)
                {
                    return;
                }

                List<IParserObject> finalResult = null;
                foreach (MismatchedPair item in result)
                {
                    switch (item.ChangeType)
                    {
                        case ChangeTypes.None:
                            break;

                        case ChangeTypes.Element_Add:
                            ProcessPotentialCheckpoint(item);
                            break;

                        case ChangeTypes.Element_Remove:
                            this.ProcessTypeChangeRemove(item);
                            break;

                        case ChangeTypes.TypeChange_Update:
                            finalResult = this.ProcessTypeChangeUpdate(item, originalObject, changedObject, Common.ContextXMLParserList);
                            if (finalResult != null)
                            {
                                string typeChangedType = this.ReturnType(item);
                                this.ReplaceCodeWithChangeType(finalResult, item.ChangeType, typeChangedType);
                            }
                            break;

                        case ChangeTypes.TypeChange_Remove:
                            this.ProcessTypeChangeRemove(item);
                            break;

                        case ChangeTypes.TypeChange_Add:
                            ProcessPotentialCheckpoint(item);
                            break;

                        case ChangeTypes.TypeToSimpleType:
                            this.ProcessTypeChangeUpdate(item, originalObject, changedObject, Common.ContextXMLParserList);
                            break;

                        case ChangeTypes.SimpleTypeToType:
                            this.ProcessTypeChangeUpdate(item, originalObject, changedObject, Common.ContextXMLParserList);
                            break;

                        case ChangeTypes.TypeToComplexType:
                            this.ProcessTypeChangeUpdate(item, originalObject, changedObject, Common.ContextXMLParserList);
                            break;

                        case ChangeTypes.ComplexTypeToType:
                            this.ProcessTypeChangeUpdate(item, originalObject, changedObject, Common.ContextXMLParserList);
                            break;

                        case ChangeTypes.SimpleTypeToComplexType:
                            this.ProcessTypeChangeUpdate(item, originalObject, changedObject, Common.ContextXMLParserList);
                            break;

                        case ChangeTypes.ComplexTypeToSimpleType:
                            this.ProcessTypeChangeUpdate(item, originalObject, changedObject, Common.ContextXMLParserList);
                            break;

                        case ChangeTypes.Element_NameAttribute_Update:
                            break;

                        case ChangeTypes.IncreasedMinOccurs:
                            ProcessPotentialCheckpointOccurs(item);
                            break;

                        case ChangeTypes.DecreasedMinOccurs:
                            ProcessPotentialCheckpointOccurs(item);
                            break;

                        case ChangeTypes.AddMinOccurs:
                            ProcessPotentialCheckpointOccurs(item);
                            //changedType = this.ReturnType(item);
                            //AddNewCodeSegmentWithTypeChange(sortedCodeStructure, changedType);
                            break;

                        case ChangeTypes.RemoveMinOccurs:
                            ProcessPotentialCheckpointOccurs(item);
                            break;

                        case ChangeTypes.IncreasedMaxOccurs:
                            ProcessPotentialCheckpointOccurs(item);
                            break;

                        case ChangeTypes.DecreasedMaxOccurs:
                            ProcessPotentialCheckpointOccurs(item);
                            break;

                        case ChangeTypes.AddMaxOccurs:
                            ProcessPotentialCheckpointOccurs(item);
                            break;

                        case ChangeTypes.RemoveMaxOccurs:
                            ProcessPotentialCheckpointOccurs(item);
                            break;

                        case ChangeTypes.IncreasedMaxLength:
                            ProcessPotentialCheckpointMaxLengthChanged(item);
                            break;

                        case ChangeTypes.DecreasedMaxLength:
                            ProcessPotentialCheckpointMaxLengthChanged(item);
                            break;

                        case ChangeTypes.AddMaxLength:
                            ProcessPotentialCheckpointMaxLengthChanged(item);
                            break;

                        case ChangeTypes.RemoveMaxLength:
                            ProcessPotentialCheckpointMaxLengthChanged(item);
                            break;

                        case ChangeTypes.ChangeRestriction_MaxLength:
                            ProcessPotentialCheckpointMaxLengthChanged(item);
                            break;

                        case ChangeTypes.ChangeRestriction_MaxLength_Increased:
                            ProcessPotentialCheckpointMaxLengthChanged(item);
                            break;

                        case ChangeTypes.ChangeRestriction_MaxLength_Decreased:
                            ProcessPotentialCheckpointMaxLengthChanged(item);
                            break;

                        case ChangeTypes.ImportElementChange:
                            this.ProcessNoChange(item);
                            break;

                        case ChangeTypes.ImportElementChange_Namespace_Update:
                            this.ProcessNoChange(item);
                            break;

                        case ChangeTypes.ImportElementChange_Namespace_Add:
                            this.ProcessNoChange(item);
                            break;

                        case ChangeTypes.ImportElementChange_Namespace_Remove:
                            this.ProcessNoChange(item);
                            break;

                        case ChangeTypes.ImportElementChange_SchemaLocation_Update:
                            this.ProcessNoChange(item);
                            break;

                        case ChangeTypes.ImportElementChange_SchemaLocation_Add:
                            this.ProcessNoChange(item);
                            break;

                        case ChangeTypes.ImportElementChange_SchemaLocation_Remove:
                            this.ProcessNoChange(item);
                            break;

                        case ChangeTypes.Element_ReferenceChange_Update:
                            this.ProcessNoChange(item);
                            break;

                        case ChangeTypes.Element_Name_Update:
                            this.ProcessNoChange(item);
                            break;

                        case ChangeTypes.AllToSequence:
                            this.ProcessNoChange(item);
                            break;

                        default:
                            break;
                    }
                }

                #endregion Step 5: Catrgory diff result
            }
            catch
            {
                throw;
            }
        }

        private void SetFileAttribute(string csFile, FileAttributes attribute)
        {
            if (File.GetAttributes(csFile) != attribute)
            {
                File.SetAttributes(csFile, attribute);
            }
        }

        #region Private Method

        private string ReturnType(MismatchedPair somDiffResult)
        {
            System.Xml.Schema.XmlSchemaElement elementChangeObject = somDiffResult.ChangeObject as System.Xml.Schema.XmlSchemaElement;
            string changedType = elementChangeObject.SchemaTypeName.Name;

            return string.IsNullOrWhiteSpace(changedType) ? string.Empty : changedType.ConvertToCsharpType();
        }

        private string XSD2Proxy(string xsdPath, string proxyNamespace)
        {
            string[] xsdfiles = Directory.GetFiles(xsdPath, "*.xsd", SearchOption.TopDirectoryOnly);

            StringBuilder arguments = new StringBuilder();

            foreach (string file in xsdfiles)
            {
                arguments.Append(file);
                arguments.Append(" ");
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\xsd.exe";
            if (!File.Exists(processStartInfo.FileName))
            {
                processStartInfo.FileName = @"C:\Program File\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\xsd.exe";
            }
            processStartInfo.Arguments = arguments.ToString() + "/classes /language:cs /n:" + proxyNamespace;
            processStartInfo.WorkingDirectory = xsdPath;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;

            // Start xsd.exe
            Process xsdtool = new Process();
            xsdtool.StartInfo = processStartInfo;
            xsdtool.Start();
            string output = xsdtool.StandardOutput.ReadToEnd();
            xsdtool.WaitForExit();

            string[] lines = output.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            // todo:log:
            string[] proxy = Directory.GetFiles(xsdPath, "*.cs", SearchOption.TopDirectoryOnly);

            if (proxy == null)
            {
                return null;
            }
            else
            {
                return proxy.FirstOrDefault();
            }
        }

        private void ProcessNoChange(MismatchedPair somDiffResult)
        {
            //
        }

        /// <summary>
        /// No need to validation
        /// </summary>
        /// <param name="item"></param>
        private void ProcessPotentialCheckpointOccurs(MismatchedPair item)
        {
            try
            {
                decimal minOccursSource = 0;
                decimal maxOccursSource = 0;
                decimal minOccurs = 0;
                decimal maxOccurs = 0;
                var sequenceSourceObject = item.SourceObject as System.Xml.Schema.XmlSchemaSequence;
                if (sequenceSourceObject != null)
                {
                    minOccursSource = sequenceSourceObject.MinOccurs;
                    maxOccursSource = sequenceSourceObject.MaxOccurs;
                }
                else
                {
                    System.Xml.Schema.XmlSchemaElement elementSourceObject = item.SourceObject as System.Xml.Schema.XmlSchemaElement;
                    if (elementSourceObject != null)
                    {
                        minOccursSource = elementSourceObject.MinOccurs;
                        maxOccursSource = elementSourceObject.MaxOccurs;
                    }
                }

                var sequenceChangeObject = item.ChangeObject as System.Xml.Schema.XmlSchemaSequence;
                if (sequenceChangeObject != null)
                {
                    minOccurs = sequenceChangeObject.MinOccurs;
                    maxOccurs = sequenceChangeObject.MaxOccurs;
                }
                else
                {
                    System.Xml.Schema.XmlSchemaElement elementChangedObject = item.ChangeObject as System.Xml.Schema.XmlSchemaElement;
                    if (elementChangedObject != null)
                    {
                        minOccurs = elementChangedObject.MinOccurs;
                        maxOccurs = elementChangedObject.MaxOccurs;
                    }
                }

                AppliedRule appliedRule = new AppliedRule(item.ChangeType);
                appliedRule.RuleStatus = RuleStatus.Info;
                string strPre = "Potential Checkpoint: ";
                string body = string.Empty;
                switch (item.ChangeType)
                {
                    case ChangeTypes.IncreasedMinOccurs:
                    case ChangeTypes.AddMinOccurs:
                    case ChangeTypes.DecreasedMinOccurs:
                    case ChangeTypes.RemoveMinOccurs:

                        body += string.Format("'{2}' - Source\\Changed MinOccurs: {0}\\{1}", minOccursSource, minOccurs, item.ChangePath);
                        break;
                    case ChangeTypes.IncreasedMaxOccurs:
                    case ChangeTypes.AddMaxOccurs:
                    case ChangeTypes.DecreasedMaxOccurs:
                    case ChangeTypes.RemoveMaxOccurs:
                        body += string.Format("'{2}' - Source\\Changed MaxOccurs {0}\\{1}", maxOccursSource, maxOccurs, item.ChangePath);
                        break;

                    default:
                        break;
                }
                appliedRule.Tag = strPre + body;
                Common.AppliedRuleList.Add(appliedRule);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// No need to validation
        /// </summary>
        /// <param name="item"></param>
        private void ProcessPotentialCheckpoint(MismatchedPair item)
        {
            try
            {
                decimal minOccursSource = 0;
                decimal maxOccursSource = 0;
                decimal minOccurs = 0;
                decimal maxOccurs = 0;
                var sequenceSourceObject = item.SourceObject as System.Xml.Schema.XmlSchemaSequence;
                if (sequenceSourceObject != null)
                {
                    minOccursSource = sequenceSourceObject.MinOccurs;
                    maxOccursSource = sequenceSourceObject.MaxOccurs;
                }
                else
                {
                    System.Xml.Schema.XmlSchemaElement elementSourceObject = item.SourceObject as System.Xml.Schema.XmlSchemaElement;
                    if (elementSourceObject != null)
                    {
                        minOccursSource = elementSourceObject.MinOccurs;
                        maxOccursSource = elementSourceObject.MaxOccurs;
                    }
                }

                var sequenceChangeObject = item.ChangeObject as System.Xml.Schema.XmlSchemaSequence;
                if (sequenceChangeObject != null)
                {
                    minOccurs = sequenceChangeObject.MinOccurs;
                    maxOccurs = sequenceChangeObject.MaxOccurs;
                }
                else
                {
                    System.Xml.Schema.XmlSchemaElement elementChangedObject = item.ChangeObject as System.Xml.Schema.XmlSchemaElement;
                    if (elementChangedObject != null)
                    {
                        minOccurs = elementChangedObject.MinOccurs;
                        maxOccurs = elementChangedObject.MaxOccurs;
                    }
                }

                AppliedRule appliedRule = new AppliedRule(item.ChangeType);
                appliedRule.RuleStatus = RuleStatus.Info;
                string strPre = "Potential Checkpoint: ";
                string body = string.Empty;
                switch (item.ChangeType)
                {
                    case ChangeTypes.IncreasedMinOccurs:
                    case ChangeTypes.AddMinOccurs:
                    case ChangeTypes.DecreasedMinOccurs:
                    case ChangeTypes.RemoveMinOccurs:

                        body += string.Format("'{4}' - Source\\Changed MinOccurs: {0}\\{1} ; Source\\Changed MaxOccurs {2}\\{3}", minOccursSource, minOccurs, maxOccursSource, maxOccurs, item.ChangePath);
                        break;
                    case ChangeTypes.IncreasedMaxOccurs:
                    case ChangeTypes.AddMaxOccurs:
                    case ChangeTypes.DecreasedMaxOccurs:
                    case ChangeTypes.RemoveMaxOccurs:
                        body += string.Format("'{4}' - Source\\Changed MinOccurs: {0}\\{1} ; Source\\Changed MaxOccurs {2}\\{3}", minOccursSource, minOccurs, maxOccursSource, maxOccurs, item.ChangePath);
                        break;

                    default:
                        break;
                }
                appliedRule.Tag = strPre + body;
                Common.AppliedRuleList.Add(appliedRule);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// No need to validation
        /// </summary>
        /// <param name="item"></param>
        private void ProcessPotentialCheckpointMaxLengthChanged(MismatchedPair item)
        {
            System.Xml.Schema.XmlSchemaMaxLengthFacet elementSourceObject = item.SourceObject as System.Xml.Schema.XmlSchemaMaxLengthFacet;
            System.Xml.Schema.XmlSchemaMaxLengthFacet elementChangeObject = item.ChangeObject as System.Xml.Schema.XmlSchemaMaxLengthFacet;
            string sourceValue = string.Empty;
            string changedValue = string.Empty;
            if (elementSourceObject != null)
            {
                sourceValue = elementSourceObject.Value;
            }
            if (elementChangeObject != null)
            {
                changedValue = elementChangeObject.Value;
            }
            AppliedRule appliedRule = new AppliedRule(item.ChangeType);
            appliedRule.RuleStatus = RuleStatus.Info;
            string strPre = "Potential Checkpoint: ";
            string body = string.Empty;
            switch (item.ChangeType)
            {
                case ChangeTypes.IncreasedMaxLength:
                case ChangeTypes.AddMaxLength:
                case ChangeTypes.ChangeRestriction_MaxLength:
                case ChangeTypes.ChangeRestriction_MaxLength_Increased:
                case ChangeTypes.ChangeRestriction_MaxLength_Decreased:
                case ChangeTypes.DecreasedMinOccurs:
                case ChangeTypes.RemoveMaxOccurs:
                case ChangeTypes.RemoveMaxLength:
                case ChangeTypes.DecreasedMaxLength:
                    body += string.Format("'{2}' - Source\\Changed : {0}\\{1}", sourceValue, changedValue, item.ChangePath);
                    break;

                default:
                    break;
            }

            appliedRule.Tag = strPre + body;
            Common.AppliedRuleList.Add(appliedRule);
        }

        private void ProcessTypeChangeRemove(MismatchedPair somDiffResult)
        {
            System.Xml.Schema.XmlSchemaElement elementSourceObject = somDiffResult.SourceObject as System.Xml.Schema.XmlSchemaElement;
            System.Xml.Schema.XmlSchemaElement elementChangeObject = somDiffResult.ChangeObject as System.Xml.Schema.XmlSchemaElement;
            string originalType = elementSourceObject.SchemaTypeName.Name;
            string changedType = elementChangeObject.SchemaTypeName.Name;
            string sourcePath = elementSourceObject.QualifiedName.Name;
            string changedPath = elementChangeObject.QualifiedName.Name;

            AppliedRule appliedRule = new AppliedRule(somDiffResult.ChangeType);
            appliedRule.RuleStatus = RuleStatus.Info;
            appliedRule.Tag = "Potential Checkpoint: " + somDiffResult.ChangeObject;
            Common.AppliedRuleList.Add(appliedRule);
        }

        private List<IParserObject> ProcessTypeChangeUpdate(MismatchedPair somDiffResult, List<IParserObject> originalObject, List<IParserObject> changedObject, List<IParserObject> destinationObject)
        {
            #region Exception

            if (somDiffResult == null)
            {
                throw new ArgumentNullException(somDiffResult.ToString());
            }
            if (originalObject == null)
            {
                throw new ArgumentNullException(originalObject.ToString());
            }
            if (changedObject == null)
            {
                throw new ArgumentNullException(changedObject.ToString());
            }
            if (destinationObject == null)
            {
                throw new ArgumentNullException(destinationObject.ToString());
            }
            #endregion

            List<IParserObject> finalResult = null;
            try
            {
                string[] entryListChanged = somDiffResult.ChangePath.Split('/');
                string[] entryListSource = somDiffResult.SourcePath.Split('/');
                if (entryListChanged == null || entryListSource == null)
                {
                    return null;
                }
                if (entryListSource != null && entryListSource.Length == 1)
                {
                    return null;
                }
                if (entryListChanged != null && entryListChanged.Length == 1)
                {
                    return null;
                }
                #region Prepare parameter

                Queue<string> xPathSource = this.ConvertToQueue(entryListSource);
                string classPath = xPathSource.ToList()[xPathSource.ToList().Count - 2];
                string fieldPath = xPathSource.ToList().Last();

                #endregion

                #region Verify diff result in source code or not

                IParserObject result = null;

                bool isExistClassInCode = IsValidateType(originalObject, xPathSource.Dequeue(), ref result);
                if (!isExistClassInCode)
                {
                    return null;
                }

                bool isExist = IsExistElement(xPathSource, result.Element as CsElement, originalObject, ref result);

                if (!isExist)
                {
                    AppliedRule appliedRule = new AppliedRule(somDiffResult.ChangeType);
                    appliedRule.RuleStatus = RuleStatus.Failed;
                    Common.AppliedRuleList.Add(appliedRule);
                    appliedRule.Tag = string.Format("Diff result: '{0}' just Head exist in Source code.", somDiffResult.ChangePath);
                    return null;
                }

                #endregion Verify that exist in code

                #region Verify diff result in changed code or not
                Queue<string> xPathChanged = this.ConvertToQueue(entryListChanged);
                classPath = xPathChanged.ToList()[xPathChanged.ToList().Count - 2];
                fieldPath = xPathChanged.ToList().Last();

                result = null;
                isExistClassInCode = IsValidateType(changedObject, xPathChanged.Dequeue(), ref result);
                if (!isExistClassInCode)
                {
                    return null;
                }

                bool isExistInChangedCode = IsExistElement(xPathChanged, result.Element as CsElement, changedObject, ref result);

                if (!isExistInChangedCode)
                {
                    AppliedRule appliedRule = new AppliedRule(somDiffResult.ChangeType);
                    appliedRule.RuleStatus = RuleStatus.Failed;
                    Common.AppliedRuleList.Add(appliedRule);
                    appliedRule.Tag = string.Format("Diff result: '{0}' just Head exist in Changed code.", somDiffResult.ChangePath);
                    return null;
                }

                #endregion Verify that exist in code

                finalResult = this.SignatureValidation(destinationObject, classPath, fieldPath);
            }
            catch (Exception)
            {
                throw;
            }

            return finalResult;
        }

        private Queue<string> ConvertToQueue(string[] entryList)
        {
            Queue<string> xPath = new Queue<string>();
            for (int i = 0; i < entryList.Length; i++)
            {
                string path = entryList[i];
                if (path.IndexOf(':') > -1)
                    xPath.Enqueue(path.Substring(path.IndexOf(':') + 1));
                else
                {
                    xPath.Enqueue(path);
                }
            }
            return xPath;
        }

        private bool IsExistElement(Queue<string> xPath, CsElement csElement, List<IParserObject> originalObject, ref IParserObject returnCode)
        {
            bool isExist = false;
            if (xPath.Count == 0)
            {
                return false;
            }
            string path = xPath.Dequeue();
            foreach (var item in csElement.ChildElements)
            {
                if (item.Declaration.Name == path && xPath.Count > 0)
                {
                    StyleCop.CSharp.Property property = item as StyleCop.CSharp.Property;
                    if (property != null && property.ReturnType.Text.ConvertToCsharpSingleType() != "object")
                    {
                        if (this.IsValidateType(originalObject, property.ReturnType.Text.ConvertToCsharpSingleType(), ref returnCode))
                        {
                            this.IsExistElement(xPath, returnCode.Element as CsElement, originalObject, ref returnCode);
                        }
                    }
                    else if (property != null && property.ReturnType.Text.ConvertToCsharpSingleType() == "object")
                    {
                        Dictionary<string, string> attributeList = this.GetAttributesAndConvertToList(property);
                        string element = xPath.Dequeue();
                        if (attributeList.Count == 0)
                        {
                            break;
                        }
                        string elementType = string.Empty;
                        foreach (var attributeType in attributeList)
                        {
                            if (attributeType.Key == element)
                            {
                                elementType = attributeType.Value;
                                break;
                            }
                        }
                        if (string.IsNullOrWhiteSpace(elementType))
                        {
                            break;
                        }
                        if (this.IsValidateType(originalObject, elementType, ref returnCode) && xPath.Count > 0)
                        {
                            if (this.IsExistElement(xPath, returnCode.Element as CsElement, originalObject, ref returnCode))
                            {
                                break;
                            }
                        }
                    }
                }
                else if (item.Declaration.Name == path && xPath.Count == 0)
                {
                    isExist = true;
                    break;
                }

            }

            return isExist;
        }

        private Dictionary<string, string> GetAttributesAndConvertToList(StyleCop.CSharp.Property property)
        {
            Dictionary<string, string> attributeList = new Dictionary<string, string>();
            if (property.Attributes.Count > 0)
            {
                foreach (var attribute in property.Attributes)
                {
                    string name = string.Empty;
                    string type = string.Empty;
                    string nameSpace = string.Empty;

                    // [System.Xml.Serialization.XmlElementAttribute("AllDayEvent", typeof(byte), Namespace="Calendar")]
                    string[] attributeArray = attribute.Text.Split(',');
                    foreach (var item in attributeArray)
                    {
                        if (item.StartsWith("[System.Xml"))
                        {
                            int index = item.IndexOf('"');
                            name = item.Substring(index).Trim('"');
                        }
                        else if (item.StartsWith("typeof"))
                        {
                            int index = item.IndexOf('(');
                            type = item.Substring(index).Trim(new char[] { '(', ')' });
                        }
                        else if (item.StartsWith("Namespace"))
                        {
                            nameSpace = string.Empty;
                        }
                    }


                    if (!attributeList.ContainsKey(name))
                    {
                        attributeList.Add(name, type);
                    }
                }
            }

            return attributeList;
        }

        private bool IsValidateType(List<IParserObject> originalObject, string returnType, ref IParserObject returnCode)
        {
            bool isTrue = false;
            List<IParserObject> validateListClass = originalObject.FindAll(p => p.OriginalT is Class);

            // We will use ABC.ExceptionsException instead of ExceptionsException
            List<IParserObject> TargetTypeList = validateListClass.FindAll(p => p.Name.Equals(returnType));
            if (TargetTypeList != null && TargetTypeList.Count > 0)
            {
                isTrue = true;
                returnCode = TargetTypeList.FirstOrDefault();
            }
            return isTrue;
        }

        private bool IsValidateVariable(List<IParserObject> originalObject, string sourcePath)
        {
            bool isTrue = false;
            List<IParserObject> validateListVariable = originalObject.FindAll(p => p.OriginalT is Variable);

            List<IParserObject> TargetVariableList = validateListVariable.FindAll(p => p.Tag.ToString().Contains(sourcePath));
            if (TargetVariableList != null && TargetVariableList.Count > 0)
            {
                isTrue = true;
            }

            return isTrue;
        }

        private bool IsValidateFields(List<IParserObject> originalObject, string sourcePath, string returnType, ref IParserObject returnCode)
        {
            bool isTrue = false;
            List<IParserObject> validateListVariable = originalObject.FindAll(p => p.OriginalT is Property);

            List<IParserObject> originalObjectInCode = validateListVariable.FindAll(p => p.Name.ToString().Equals(sourcePath));
            if (originalObjectInCode != null && originalObjectInCode.Count > 0)
            {
                foreach (var item in originalObjectInCode)
                {
                    Property property = item.OriginalT as Property;
                    if (property != null)
                    {
                        if (property.ReturnType.ToString().Equals(returnType, StringComparison.InvariantCultureIgnoreCase))
                        {
                            isTrue = true;
                            returnCode = item;
                            break;
                        }
                    }
                }
            }

            return isTrue;
        }

        /// <summary>
        /// Validate related type exist in code or not.
        /// </summary>
        /// <param name="originalObject">from code</param>
        /// <param name="subject">field name</param>
        /// <param name="type">field type</param>
        /// <param name="subjectNamespace">field namespace</param>
        /// <returns></returns>
        private bool IsValidateAttribute(List<IParserObject> originalObject, string subject, string type, string subjectNamespace)
        {
            bool isTrue = false;
            string attribute = string.Format("\"{0}\", typeof({1}), Namespace=\"{2}\"", subject, type, subjectNamespace);

            List<IParserObject> validateListVariable = originalObject.FindAll(p => p.OriginalT is StyleCop.CSharp.Attribute);
            List<IParserObject> TargetVariableList = validateListVariable.FindAll(p => p.Name.Contains(attribute));
            if (TargetVariableList != null && TargetVariableList.Count > 0)
            {
                isTrue = true;
            }

            return isTrue;
        }

        private List<IParserObject> SignatureValidation(List<IParserObject> sourceParseObject, string classXPath, string expressionXpath)
        {
            if (string.IsNullOrWhiteSpace(classXPath))
            {
                throw new ArgumentNullException(classXPath);
            }

            List<IParserObject> validateListClass = sourceParseObject.FindAll(p => p.OriginalT is Class);
            List<IParserObject> validateListVariable = sourceParseObject.FindAll(p => p.OriginalT is Variable);
            List<IParserObject> validateListExpression = sourceParseObject.FindAll(p => p.OriginalT is StyleCop.CSharp.Expression);
            List<IParserObject> validateListUsingDirective = sourceParseObject.FindAll(p => p.OriginalT is UsingDirective);

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

            // Sort array
            List<IParserObject> sortedCodeStructure =
                validateListExpressionsNeedToUpdate.OrderBy(c => c.FileFullPath).ThenByDescending(c => c.Location.LineNumber).ToList();

            return sortedCodeStructure;
        }

        private int CompareByNumber(IParserObject x, IParserObject y)
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
        private void AnalyseDocument(CodeFile scCode, StyleCopCore core, List<IParserObject> contextXMLParserList)
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

        private void GetDocumentElements(CodeFile scCode, StyleCopCore core, List<IParserObject> contextXMLParserList)
        {
            CodeDocument document = GetCodeDocument(scCode);
            Param.RequireNotNull(document, "document");

            CsDocument csdocument = (CsDocument)document;

            if (csdocument.RootElement != null)
            {
                // Get Class/Field/Attribute.. definition
                this.ProcessElement(csdocument.RootElement, false, contextXMLParserList);
                // this.CheckCodeAnalysisAttribute(csdocument.RootElement, contextXMLParserList);
            }
        }

        private CodeDocument GetCodeDocument(CodeFile scCode)
        {
            CodeDocument scDoc = null;
            scCode.Parser.PreParse();
            scCode.Parser.ParseFile(scCode, 0, ref scDoc);

            return scDoc;
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

        private void CheckCodeAnalysisAttribute(CsElement element, List<IParserObject> contextXMLParserList)
        {
            Param.AssertNotNull(element, "element");
            if (element.Attributes == null)
            {
                foreach (var item in element.ChildElements)
                {
                    this.CheckCodeAnalysisAttribute(item, contextXMLParserList);

                    if (item is StyleCop.CSharp.Namespace)
                    {
                        foreach (var subElement in item.ChildElements)
                        {
                            this.CheckCodeAnalysisAttribute(subElement, contextXMLParserList);
                        }
                    }
                }
            }
            else if (element.Attributes != null)
            {
                foreach (StyleCop.CSharp.Attribute attribute in element.Attributes)
                {
                    IParserObject itemElement = Common.ConvertStyleCopTypeToLocal<StyleCop.CSharp.Attribute>(attribute, element);
                    contextXMLParserList.Add(itemElement);
                }

                foreach (var item in element.ChildElements)
                {
                    this.CheckCodeAnalysisAttribute(item, contextXMLParserList);
                }
            }
        }

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

        private void ReplaceCodeWithChangeType(List<IParserObject> sortedCodeStructure, ChangeTypes typeChange, string changedType)
        {
            if (sortedCodeStructure.Count() > 0)
            {
                int helpCount = 0;
                List<string> helperList = new List<string>();

                for (int i = 0; i < sortedCodeStructure.Count(); i++)
                {
                    // Count every code segment line number for commented code
                    IParserObject item = sortedCodeStructure[i];

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
                            switch (changedType)
                            {
                                case "string":
                                case "String":
                                case "System.String":
                                    newStr = expressionList[0] + string.Format(" = \"{1}\"", tempStr);
                                    break;

                                default:
                                    newStr = expressionList[0] + " = " + tempStr;
                                    break;
                            }
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
                    this.ReplaceExpression(oldStr, newStr, item, typeChange);
                }
            }
        }

        private bool ReplaceExpression(string oldName, string newName, IParserObject itemObject, ChangeTypes changeType)
        {
            if (itemObject == null)
                throw new ArgumentNullException("itemObject is null");

            if (itemObject.Location == null)
                throw new ArgumentNullException("itemObject.Location is null");

            bool isSucceed = false;
            itemObject.Location.LineNumber = itemObject.Location.LineNumber;
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
                    this.SetFileAttribute(itemObject.FileFullPath, FileAttributes.Normal);
                    File.WriteAllLines(itemObject.FileFullPath, files);
                }

                #endregion Replace code

                isSucceed = true;

                // Replace code
                AppliedRule appliedRule = new AppliedRule(changeType);
                appliedRule.ParserObject = itemObject;
                appliedRule.RuleStatus = RuleStatus.Passed;

                Common.AppliedRuleList.Add(appliedRule);
                Common.ContextXMLParserList.InsertChange(itemObject);

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
                    this.SetFileAttribute(itemObject.FileFullPath, FileAttributes.Normal);
                    File.WriteAllLines(itemObject.FileFullPath, files);
                }
                isSucceed = true;
                // Replace code
                AppliedRule appliedRule = new AppliedRule(changeType);
                appliedRule.ParserObject = itemObject;
                Common.AppliedRuleList.Add(appliedRule);
                Common.ContextXMLParserList.Insert(itemObject);
            }
            catch (Exception)
            {
                isSucceed = false;
            }

            return isSucceed;
        }

        private List<string> GetCsprojFiles(string slnFilePath)
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

        private List<string> FindFallCSFiles(List<string> csprojFileFullPath)
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

        private Dictionary<string, List<string>> FindFAllCSFiles(string fileSlnPath, List<string> csprojFileFullPath)
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

        #region From StyleCop code

        /// <summary>
        /// Checks the given code analysis suppression call to ensure that it contains a justification parameter.
        /// </summary>
        /// <param name="element">
        /// The element that contains the suppression attribute.
        /// </param>
        /// <param name="suppression">
        /// The suppression to check.
        /// </param>
        private void CheckCodeAnalysisAttribute(CsElement element, MethodInvocationExpression suppression)
        {
            Param.AssertNotNull(element, "element");
            Param.AssertNotNull(suppression, "suppression");

            bool justification = false;
            foreach (Argument argument in suppression.Arguments)
            {
                if (argument.Expression.ExpressionType == ExpressionType.Assignment)
                {
                    AssignmentExpression assignmentExpression = (AssignmentExpression)argument.Expression;
                    if (assignmentExpression.LeftHandSide.Tokens.First.Value.Text.Equals("Justification", StringComparison.Ordinal))
                    {
                        Expression rightHandSide = assignmentExpression.RightHandSide;

                        if (rightHandSide == null || rightHandSide.Tokens == null)
                        {
                            break;
                        }

                        Node<CsToken> rightSideTokenNode = rightHandSide.Tokens.First;
                        if (rightSideTokenNode == null)
                        {
                            break;
                        }

                        if (rightHandSide.ExpressionType == ExpressionType.MemberAccess)
                        {
                            justification = true;
                            break;
                        }

                        if (rightSideTokenNode.Value.CsTokenType == CsTokenType.Other && rightHandSide.ExpressionType == ExpressionType.Literal)
                        {
                            justification = true;
                            break;
                        }

                        if (rightSideTokenNode.Value.CsTokenType == CsTokenType.String && rightSideTokenNode.Value.Text != null
                            && !IsEmptyString(rightSideTokenNode.Value.Text))
                        {
                            justification = true;
                            break;
                        }
                    }
                }
            }

            if (!justification)
            {
                //this.AddViolation(element, suppression.LineNumber, Rules.CodeAnalysisSuppressionMustHaveJustification);
            }
        }

        /// <summary>
        /// Determines whether the given text contains an empty string, which can be represented as "" or @"".
        /// </summary>
        /// <param name="text">
        /// The text to check.
        /// </param>
        /// <returns>
        /// Returns true if the
        /// </returns>
        private bool IsEmptyString(string text)
        {
            Param.AssertNotNull(text, "text");

            // A string is always considered empty if it is two characters or less, because then it must have at least
            // the opening and closing quotes plus something in between.
            if (text.Length > 2)
            {
                // If this is a literal string, then it must be more than three characters.
                if (text[0] == '@')
                {
                    if (text.Length > 3)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            // This is an empty string.
            return true;
        }

        /// <summary>
        /// Determines whether the given method invocation expression contains a code analysis SuppressMessage call.
        /// </summary>
        /// <param name="expression">
        /// The expression.
        /// </param>
        /// <returns>
        /// Returns true if the method is SuppressMessage.
        /// </returns>
        private bool IsSuppressMessage(MethodInvocationExpression expression)
        {
            Param.AssertNotNull(expression, "expression");

            Node<CsToken> first = expression.Name.Tokens.First;
            if (first != null)
            {
                string text = first.Value.Text;
                if (text.Equals("SuppressMessage", StringComparison.Ordinal) || text.Equals("SuppressMessageAttribute", StringComparison.Ordinal))
                {
                    return true;
                }

                string expressionText = expression.Name.Text;
                if (expressionText.EndsWith(".SuppressMessage", StringComparison.Ordinal)
                    || expressionText.EndsWith(".SuppressMessageAttribute", StringComparison.Ordinal))
                {
                    return true;
                }

                if (text.Equals("System"))
                {
                    if (expression.Name.Tokens.MatchTokens(new[] { "System", ".", "Diagnostics", ".", "CodeAnalysis", ".", "SuppressMessage" })
                        || expression.Name.Tokens.MatchTokens(new[] { "System", ".", "Diagnostics", ".", "CodeAnalysis", ".", "SuppressMessageAttribute" }))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

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

        #endregion

    }
}