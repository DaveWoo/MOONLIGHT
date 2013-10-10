// -----------------------------------------------------------------------
// <copyright file="XMLParserBase.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace XMLParserConversion
{
    using System;
    using XMLParserConversion.Code;

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
    }
}