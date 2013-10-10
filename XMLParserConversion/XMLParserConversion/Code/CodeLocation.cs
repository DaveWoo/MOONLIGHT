// -----------------------------------------------------------------------
// <copyright file="CodeLocation.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace XMLParserConversion.Code
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class CodeLocationX
    {
        public static readonly CodeLocationX Empty;

        public CodeLocationX() { }

        public CodePointX EndPoint { set; get; }
        public int LineNumber { set; get; }
        public int LineSpan { set; get; }
        public CodePointX StartPoint { set; get; }

    }
}
