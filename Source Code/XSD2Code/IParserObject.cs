// -----------------------------------------------------------------------
// <copyright file="XMLParserBase.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace XinYu.XSD2Code
{
    using System;
    using XinYu.XSD2Code.Code;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IParserObject
    {
        CodeLocationX Location { get; }

        string Name { get; }

        string ProjectName { get; set; }

        string FileFullPath { set; get; }

        string FullNamespaceName { set; get; }

        object OriginalT { get; set; }

        object Tag { get; set; }

        object Parent { get; set; }

        object Element { get; set; }

        int Key { get; }

    }
}