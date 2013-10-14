using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StyleCop;
using StyleCop.CSharp;
using System.Reflection;
using System.IO;
using Microsoft.Build.Execution;

namespace XinYu.XSD2Code
{
    class Program
    {
        static void Main(string[] args)
        {
            //string file=@"D:\O15\TestSuites\Exchange\Platinum\ExchangeServerEASProtocolTestSuites\Exchange Server EAS Protocol Test Suites\Source\Common\CommonProject.csproj";
            //Assembly ass = Assembly.LoadFile(file);

            string filePath = @"D:\O15\TestSuites\Exchange\Platinum\ExchangeServerEASProtocolTestSuites\Exchange Server EAS Protocol Test Suites\Source\MS-ASCMD - Copy\MS-ASCMD.sln";

            #region Step 1  -   Build
            //BuildMode mode = BuildMode.Debug;
            //BuildResultCode actual = Common.BuildSolution(filePath, mode, @"BuildLog.txt");

            //if (BuildResultCode.Success != actual)
            //{
            //    throw new Exception("Build Exception occurred.");
            //}
            #endregion

            Analyze analyze = new Analyze();
            analyze.Initialize(filePath);
            string sourceXSDPath = @"D:\360云盘\GitHub\MOONLIGHT\XSDConversion\TestData\Resources\OriginalXSD\Response\Calendar.xsd";
            string changedXSDPath = @"D:\360云盘\GitHub\MOONLIGHT\XSDConversion\TestData\Resources\ChangedXSD\Response\cal.xsd";

            analyze.Run(sourceXSDPath, changedXSDPath, null);
        }
    }
}
