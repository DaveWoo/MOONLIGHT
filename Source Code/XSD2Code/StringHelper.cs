// -----------------------------------------------------------------------
// <copyright file="StringHelper.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace XinYu.XSD2Code
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class StringHelper
    {
        public static string TrimX(this string ojb)
        {
            return ojb.Trim(new char[] { '"', ' ' });
        }
    }
}
