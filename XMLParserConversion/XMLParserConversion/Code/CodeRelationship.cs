namespace XMLParserConversion
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using StyleCop;
    using StyleCop.CSharp;

    public partial class Analyze
    {
        private string slnFilePath;

        /// <summary>
        /// Initial special solution.
        /// </summary>
        /// <param name="slnPath">solution file path</param>
        public void Initialize(string slnPath)
        {
            this.slnFilePath = slnPath;
            XMLParserConversion.Common.ContextXMLParserList = new List<IParserObject>();
            XMLParserConversion.Common.AppliedRuleList = new List<AppliedRule>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static StyleCopCore CreateStyleCopCore()
        {
            string path = string.Empty;
            List<string> temp = new List<string>();

            StyleCopCore core = new StyleCopCore();
            core.Initialize(null, true);
            core.DisplayUI = false;

            // App and style code exist in same bin
            if ((core.Parsers.Count != 1) || !(core.Parsers.ElementAt(0) is CsParser))
            {
                path = AppDomain.CurrentDomain.BaseDirectory;
                temp.Clear();
                temp.Add(path);
                core.Initialize(temp, false);
            }

            // App and dll stylecop exist in different location
            if ((core.Parsers.Count != 1) || !(core.Parsers.ElementAt(0) is CsParser))
            {
                Assembly assembly = Assembly.Load("StyleCop.CSharp, Version=4.7.45.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
                if (assembly != null)
                {
                    FileInfo file = new FileInfo(assembly.Location);
                    path = file.DirectoryName;
                    temp.Clear();
                    temp.Add(path);
                    core.Initialize(temp, true);
                }
            }

            if ((core.Parsers.Count != 1) || !(core.Parsers.ElementAt(0) is CsParser))
            {
                throw new InvalidDataException("Can not find CSharp parser in Stylecop or more than 1 parser in StyleCop");
            }

            return core;
        }
    }
}