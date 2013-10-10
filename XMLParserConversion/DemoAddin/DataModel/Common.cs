// -----------------------------------------------------------------------
// <copyright file="common.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace DemoAddin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using XMLParserConversion;
    using System.IO;
    using EnvDTE80;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Common
    {
        #region Dte
        public static DTE2 ApplicationObject { get; set; }
        #endregion

        #region GetProtocols

        public static List<string> GetProtocols(List<string> protocolList)
        {
            List<string> protocol = new List<string>();
            List<string> help = new List<string>();
            string protocolName = string.Empty;
            foreach (var item in protocolList)
            {

                if (item.Split(new char['-']) != null)
                {
                    protocolName = item.Split('_').FirstOrDefault();
                }
                if (!help.Contains(protocolName))
                {
                    help.Add(protocolName);
                    protocol.Add(protocolName);
                }
            }
            return protocol;
        }

        #endregion

        #region GetChangedTypes

        public static List<string> GetChangedTypes()
        {
            List<string> help = new List<string>();
            List<AppliedRule> appliedRuleList = XMLParserConversion.Common.AppliedRuleList;
            if (appliedRuleList != null && appliedRuleList.Count > 0)
            {
                foreach (var item in appliedRuleList)
                {
                    if (!help.Contains(item.ChangedType.ToString()))
                    {
                        help.Add(item.ChangedType.ToString());
                    }
                }
            }

            return help;
        }

        #endregion

        #region GetRuleDetails
        public static List<AppliedRule> GetRuleDetails(string changeType)
        {
            List<AppliedRule> temp = new List<AppliedRule>();
            List<AppliedRule> appliedRuleList = XMLParserConversion.Common.AppliedRuleList;
            if (appliedRuleList != null && appliedRuleList.Count > 0)
            {
                foreach (var item in appliedRuleList)
                {
                    string csFileName = item.CodeDocument.FullPath;
                    if (changeType == item.ChangedType.ToString())
                    {
                        if (!item.CodeDocument.IsCommented)
                            temp.Add(item);
                    }
                }
            }
            return temp;
        }

        #endregion
    }
}
