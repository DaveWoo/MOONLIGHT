// -----------------------------------------------------------------------
// <copyright file="Common.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace XMLParserConversion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using StyleCop;
    using XMLParserConversion.Code;
    using StyleCop.CSharp;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Common
    {
        public const string PrePatten = @"(\.?\s?";
        public const string SufPatten = @"\.?\s?)";
        public static List<IParserObject> ContextXMLParserList;
        public static List<AppliedRule> AppliedRuleList;
        public static Dictionary<string, List<string>> CSFileList { get; set; }
        public static StringBuilder OutputContext = null;
        public static CodeLocationX ConvertLocationToLocal(this CodeLocation location)
        {
            CodeLocationX locationLocation = new CodeLocationX();
            locationLocation.EndPoint = new CodePointX();
            locationLocation.EndPoint.Index = location.EndPoint.Index;
            locationLocation.EndPoint.IndexOnLine = location.EndPoint.IndexOnLine;
            locationLocation.EndPoint.LineNumber = location.EndPoint.LineNumber;

            locationLocation.LineNumber = location.LineNumber;
            locationLocation.LineSpan = location.LineNumber;
            locationLocation.StartPoint = new CodePointX();

            locationLocation.StartPoint.Index = location.StartPoint.Index;
            locationLocation.StartPoint.IndexOnLine = location.StartPoint.IndexOnLine;
            locationLocation.StartPoint.LineNumber = location.StartPoint.LineNumber;

            return locationLocation;
        }

        public static string GetCsprojFileWithCsFileName(string csFileFullPath)
        {
            Dictionary<string, List<string>> csFileList2 = XMLParserConversion.Common.CSFileList;
            if (csFileList2 != null && csFileList2.Keys.Count > 0)
            {
                foreach (var itemCSfileList in csFileList2)
                {
                    foreach (var itemFile in itemCSfileList.Value)
                    {
                        if (itemFile == csFileFullPath)
                        {
                            return itemCSfileList.Key;
                        }
                    }
                }
            }

            return null;
        }

        //public static void ConvertStyleCopTypeToLocal<T>(T parsedClass, CsElement element)
        //{
        //    if (parsedClass is Class)
        //    {
        //        Class item = parsedClass as Class;
        //        ParserObject<Class> xmlParserClass = new ParserObject<Class>(item);
        //        xmlParserClass.Name = item.Declaration.Name;
        //        xmlParserClass.Location = item.Location.ConvertLocationToLocal();
        //        xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
        //        xmlParserClass.FullNamespaceName = item.FullNamespaceName;
        //        xmlParserClass.Element = element;
        //        xmlParserClass.Parent = item.Parent;
        //        xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
        //        ContextXMLParserList.Add(xmlParserClass);
        //    }
        //    else if (parsedClass is Property)
        //    {
        //        Property item = parsedClass as Property;
        //        ParserObject<Property> xmlParserClass = new ParserObject<Property>(item);
        //        xmlParserClass.Name = item.Declaration.Name;
        //        xmlParserClass.Location = item.Location.ConvertLocationToLocal();
        //        xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
        //        xmlParserClass.FullNamespaceName = item.FullNamespaceName;
        //        xmlParserClass.Element = element;
        //        xmlParserClass.Parent = item.Parent;
        //        xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
        //        ContextXMLParserList.Add(xmlParserClass);
        //    }
        //    else if (parsedClass is Field)
        //    {
        //        Field item = parsedClass as Field;
        //        ParserObject<Field> xmlParserClass = new ParserObject<Field>(item);
        //        xmlParserClass.Name = item.Declaration.Name;
        //        xmlParserClass.Location = item.Location.ConvertLocationToLocal();
        //        xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
        //        xmlParserClass.FullNamespaceName = item.FullNamespaceName;
        //        //  xmlParserClass.Tag=item.ty
        //        xmlParserClass.Element = element;
        //        xmlParserClass.Parent = item.Parent;
        //        xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
        //        ContextXMLParserList.Add(xmlParserClass);
        //    }
        //    else if (parsedClass is Method)
        //    {
        //        Method item = parsedClass as Method;
        //        ParserObject<Method> xmlParserClass = new ParserObject<Method>(item);
        //        xmlParserClass.Name = item.Declaration.Name;
        //        xmlParserClass.Location = item.Location.ConvertLocationToLocal();
        //        xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
        //        xmlParserClass.FullNamespaceName = item.FullNamespaceName;
        //        xmlParserClass.Element = element;
        //        xmlParserClass.Parent = item.Parent;
        //        xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
        //        ContextXMLParserList.Add(xmlParserClass);
        //    }
        //    else if (parsedClass is Variable)
        //    {
        //        Variable item = parsedClass as Variable;
        //        ParserObject<Variable> xmlParserClass = new ParserObject<Variable>(item);
        //        xmlParserClass.Name = item.Name;
        //        xmlParserClass.Location = item.Location.ConvertLocationToLocal();
        //        xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
        //        xmlParserClass.FullNamespaceName = string.Empty;
        //        xmlParserClass.Tag = item.Type;
        //        xmlParserClass.Element = element;
        //        xmlParserClass.Parent = item.Parent;
        //        xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
        //        ContextXMLParserList.Add(xmlParserClass);
        //    }
        //    else if (parsedClass is UsingDirective)
        //    {
        //        UsingDirective item = parsedClass as UsingDirective;
        //        ParserObject<UsingDirective> xmlParserClass = new ParserObject<UsingDirective>(item);
        //        xmlParserClass.Name = item.Name;
        //        xmlParserClass.Location = item.Location.ConvertLocationToLocal();
        //        xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
        //        xmlParserClass.FullNamespaceName = string.Empty;
        //        xmlParserClass.Tag = null;
        //        xmlParserClass.Element = element;
        //        xmlParserClass.Parent = item.Parent;
        //        xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
        //        ContextXMLParserList.Add(xmlParserClass);
        //    }
        //    else if (parsedClass is Expression)
        //    {
        //        Expression item = parsedClass as Expression;
        //        ParserObject<Expression> xmlParserClass = new ParserObject<Expression>(item);
        //        xmlParserClass.Name = item.Text;
        //        xmlParserClass.Location = item.Location.ConvertLocationToLocal();
        //        xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
        //        xmlParserClass.FullNamespaceName = string.Empty;

        //        CodePartType codePartType = item.CodePartType;
        //        Expression tempExpression = item.Parent as Expression;
        //        Expression orignalExpression = item;
        //        while (tempExpression != null && codePartType == tempExpression.CodePartType)
        //        {
        //            orignalExpression = tempExpression;
        //            tempExpression = tempExpression.Parent as Expression;
        //        }
        //        xmlParserClass.Tag = orignalExpression;
        //        xmlParserClass.Element = element;
        //        xmlParserClass.Parent = item.Parent;
        //        xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
        //        ContextXMLParserList.Add(xmlParserClass);
        //    }
        //    else if (parsedClass is MethodInvocationExpression)
        //    {
        //        MethodInvocationExpression item = parsedClass as MethodInvocationExpression;
        //        ParserObject<MethodInvocationExpression> xmlParserClass = new ParserObject<MethodInvocationExpression>(item);
        //        xmlParserClass.Name = item.Name.Text;
        //        xmlParserClass.Location = item.Location.ConvertLocationToLocal();
        //        xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
        //        xmlParserClass.FullNamespaceName = string.Empty;
        //        xmlParserClass.Element = element;
        //        xmlParserClass.Parent = item.Parent;
        //        xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
        //        ContextXMLParserList.Add(xmlParserClass);
        //    }
        //}

        public static ParserObject<T> ConvertStyleCopTypeToLocal<T>(T parsedClass, CsElement element)
        {
            ParserObject<T> xmlParserClass = new ParserObject<T>(parsedClass);

            if (parsedClass is Class)
            {
                Class item = parsedClass as Class;
                xmlParserClass.Name = item.Declaration.Name;
                xmlParserClass.Location = item.Location.ConvertLocationToLocal();
                xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
                xmlParserClass.FullNamespaceName = item.FullNamespaceName;
                xmlParserClass.Element = element;
                xmlParserClass.Parent = item.Parent;
                xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
            }
            else if (parsedClass is Property)
            {
                Property item = parsedClass as Property;
                xmlParserClass.Name = item.Declaration.Name;
                xmlParserClass.Location = item.Location.ConvertLocationToLocal();
                xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
                xmlParserClass.FullNamespaceName = item.FullNamespaceName;
                xmlParserClass.Element = element;
                xmlParserClass.Parent = item.Parent;
                xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
            }
            else if (parsedClass is Field)
            {
                Field item = parsedClass as Field;
                xmlParserClass.Name = item.Declaration.Name;
                xmlParserClass.Location = item.Location.ConvertLocationToLocal();
                xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
                xmlParserClass.FullNamespaceName = item.FullNamespaceName;
                //  xmlParserClass.Tag=item.ty
                xmlParserClass.Element = element;
                xmlParserClass.Parent = item.Parent;
                xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
            }
            else if (parsedClass is Method)
            {
                Method item = parsedClass as Method;
                xmlParserClass.Name = item.Declaration.Name;
                xmlParserClass.Location = item.Location.ConvertLocationToLocal();
                xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
                xmlParserClass.FullNamespaceName = item.FullNamespaceName;
                xmlParserClass.Element = element;
                xmlParserClass.Parent = item.Parent;
                xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
            }
            else if (parsedClass is Variable)
            {
                Variable item = parsedClass as Variable;
                xmlParserClass.Name = item.Name;
                xmlParserClass.Location = item.Location.ConvertLocationToLocal();
                xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
                xmlParserClass.FullNamespaceName = string.Empty;
                xmlParserClass.Tag = item.Type;
                xmlParserClass.Element = element;
                xmlParserClass.Parent = item.Parent;
                xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
            }
            else if (parsedClass is UsingDirective)
            {
                UsingDirective item = parsedClass as UsingDirective;
                xmlParserClass.Name = item.Name;
                xmlParserClass.Location = item.Location.ConvertLocationToLocal();
                xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
                xmlParserClass.FullNamespaceName = string.Empty;
                xmlParserClass.Tag = null;
                xmlParserClass.Element = element;
                xmlParserClass.Parent = item.Parent;
                xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
            }
            else if (parsedClass is Expression)
            {
                Expression item = parsedClass as Expression;
                xmlParserClass.Name = item.Text;
                xmlParserClass.Location = item.Location.ConvertLocationToLocal();
                xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
                xmlParserClass.FullNamespaceName = string.Empty;

                CodePartType codePartType = item.CodePartType;
                Expression tempExpression = item.Parent as Expression;
                Expression orignalExpression = item;
                while (tempExpression != null && codePartType == tempExpression.CodePartType)
                {
                    orignalExpression = tempExpression;
                    tempExpression = tempExpression.Parent as Expression;
                }
                xmlParserClass.Tag = orignalExpression;
                xmlParserClass.Element = element;
                xmlParserClass.Parent = item.Parent;
                xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
            }
            else if (parsedClass is MethodInvocationExpression)
            {
                MethodInvocationExpression item = parsedClass as MethodInvocationExpression;
                xmlParserClass.Name = item.Name.Text;
                xmlParserClass.Location = item.Location.ConvertLocationToLocal();
                xmlParserClass.FileFullPath = element.Document.SourceCode.Path;
                xmlParserClass.FullNamespaceName = string.Empty;
                xmlParserClass.Element = element;
                xmlParserClass.Parent = item.Parent;
                xmlParserClass.ProjectName = GetCsprojFileWithCsFileName(element.Document.SourceCode.Path);
            }

            return xmlParserClass;
        }


        public static BuildResultCode BuildSolution(string path, BuildMode mode, string resultFile)
        {
            string buildResultFilePath = string.Format(@"logfile={0}", resultFile);
            ProjectCollection projectCollection = new ProjectCollection();

            Dictionary<string, string> GlobalProperty = new Dictionary<string, string>();

            GlobalProperty.Add("Configuration", mode.ToString());

            GlobalProperty.Add("Platform", "Any CPU");

            BuildRequestData buildRequest = new BuildRequestData(path, GlobalProperty, null, new string[] { "ReBuild" }, null);

            ILogger logger = new Microsoft.Build.Logging.FileLogger();
            logger.Parameters = buildResultFilePath;

            MyLogger myLogger = new MyLogger();

            IList<ILogger> loggers = new List<ILogger>();
            loggers.Add(logger);
            loggers.Add(myLogger);

            BuildParameters buildParameters = new BuildParameters(projectCollection);

            buildParameters.Loggers = loggers;
            BuildResult buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);

            // According to platinum guidance, no error or warning should exist. So treat them equally.
            if (myLogger.ErrorCount > 0 || myLogger.WarningCount > 0)
            {
                return BuildResultCode.Failure;
            }
            else
            {
                return BuildResultCode.Success;
            }
        }

    }

    public enum BuildMode
    {
        Debug,
        Release,
    }

    public class MyLogger : Logger
    {
        public int ErrorCount { get; set; }

        public int WarningCount { get; set; }

        public override void Initialize(IEventSource eventSource)
        {
            ErrorCount = 0;
            WarningCount = 0;
            eventSource.ErrorRaised += new BuildErrorEventHandler(eventSource_ErrorRaised);
            eventSource.WarningRaised += new BuildWarningEventHandler(eventSource_WarningRaised);
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            ErrorCount++;
            //string line= String.Format (":ERROR {0}({1},{2}):",e.File,e.LineNumber,e.ColumnNumber);
            //line  += e.Message;
        }

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            WarningCount++;
            //string line = String.Format(":WARNING {0}({1},{2}):", e.File, e.LineNumber, e.ColumnNumber);
            //line += e.Message;
        }
    }


}
