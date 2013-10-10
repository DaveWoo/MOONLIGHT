namespace DemoAddin
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using EnvDTE;
    using EnvDTE80;
    using DemoAddin.LoadOnDemand;
    using XMLParserConversion;

    /// <summary>
    /// Interaction logic for AnalysePanel.xaml
    /// </summary>
    public partial class AnalysePanel : UserControl, IPTFConfigEditorUIControl
    {
        private List<CodeStructure> commentedCode = null;
        private bool needToDelete = false;

        public AnalysePanel()
        {
            InitializeComponent();
        }

        public void Refresh()
        {
        }

        public DTE2 ApplicationObject { get; set; }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.NavigateTree.DataContext = null;

                string filePath = @"D:\O15\TestSuites\Exchange\Platinum\ExchangeServerEASProtocolTestSuites\Exchange Server EAS Protocol Test Suites\Source\MS-ASCMD - Copy\MS-ASCMD.sln";

                #region Step 1  -   Build

                //BuildMode mode = BuildMode.Debug;
                //BuildResultCode actual = Common.BuildSolution(filePath, mode, @"BuildLog.txt");

                //if (BuildResultCode.Success != actual)
                //{
                //    throw new Exception("Build Exception occurred.");
                //}

                #endregion Step 1  -   Build

                List<string> originalProxy = new List<string>();
                List<string> changedProxy = new List<string>();
                string dir = @"D:\Study\XMLParserConversion\DemoAddin\bin\Debug";
                originalProxy.Add(Path.Combine(dir, "Proxy\\OriginalActiveSyncRequestProxy.cs"));
                originalProxy.Add(Path.Combine(dir, "Proxy\\OriginalActiveSyncResponseProxy.cs"));
                changedProxy.Add(Path.Combine(dir, "Proxy\\ChangedActiveSyncResponseProxy.cs"));
                changedProxy.Add(Path.Combine(dir, "Proxy\\ChangedActiveSyncRequestProxy.cs"));
                Analyze analyze = new Analyze();
                analyze.Initialize(filePath);
                analyze.Run(originalProxy, changedProxy);

                List<AppliedRule> appliedRuleList = XMLParserConversion.Common.AppliedRuleList;

                if (appliedRuleList != null && appliedRuleList.Count > 0)
                {
                    #region Generate Project

                    #region Get all project name
                    List<string> projects = new List<string>();
                    foreach (var item in appliedRuleList)
                    {
                        string projectName = item.CodeDocument.ProjectName;
                        if (!projects.Contains(projectName))
                        {
                            projects.Add(projectName);
                        }
                    }
                    #endregion

                    // Get Level 1 data
                    List<string> projectsList = Common.GetProtocols(projects);

                    #endregion Generate Project

                    #region Generate ExplorerViewModel and Bing Data

                    ExplorerViewModel viewModel = new ExplorerViewModel(projectsList);
                    this.NavigateTree.DataContext = viewModel;
                    this.TextOutput.Text = XMLParserConversion.Common.OutputContext.ToString();

                    #endregion Generate ExplorerViewModel and Bing Data
                }

                // Get commented code
                commentedCode = GetCommendCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message.ToString());
            }
        }

        private void NavigateTree_TreeView_SelectedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                RuleViewModel detailInfo = this.NavigateTree.TreeviewControl.SelectedItem as RuleViewModel;
                if (detailInfo != null)
                {
                    detailInfo.IsSelected = true;

                    ProjectItem projectItem = this.FindSpecialProjectItem(detailInfo.FullPath);
                    if (projectItem == null)
                    {
                        MessageBox.Show("Make sure *.sln file is opened.");
                        return;
                    }
                    if (!projectItem.IsOpen)
                    {
                        projectItem.Open();
                    }
                    projectItem.Document.Activate();
                    EnvDTE.TextSelection ts = (EnvDTE.TextSelection)projectItem.Document.Selection;
                    ts.MoveToLineAndOffset(Convert.ToInt32(detailInfo.LineNumber), 1, false);
                    ts.EndOfLine(false);
                    ts.MoveToLineAndOffset(Convert.ToInt32(detailInfo.LineNumber), 1, true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message.ToString());
            }
        }

        /// <summary>
        /// 1. If return value is null, please make sure *.sln is opened.
        /// 2. No related csFile was found.
        /// </summary>
        /// <param name="csFile"></param>
        /// <returns></returns>
        private ProjectItem FindSpecialProjectItem(string csFile)
        {
            ProjectItem itemProject = null;
            foreach (EnvDTE.Project project in Common.ApplicationObject.Solution.Projects)
            {
                if (project.Kind == "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")
                {
                    foreach (ProjectItem item in project.ProjectItems)
                    {
                        string fileName = item.FileNames[0];
                        if (!fileName.EndsWith(".cs"))
                        {
                            continue;
                        }
                        if (fileName.IndexOf(csFile) == 0)
                        {
                            itemProject = item;
                            break;
                        }
                    }
                }
                if (itemProject != null)
                    break;
            }

            return itemProject;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (commentedCode == null)
                {
                    MessageBox.Show("No commented code need to be removed.");
                    return;
                }

                // Remove commented code
                if (commentedCode != null)
                {
                    foreach (var item in commentedCode)
                    {
                        List<string> files = File.ReadAllLines(item.FullPath).ToList();
                        files.RemoveAt(item.LineNumber - 1);
                        File.WriteAllLines(item.FullPath, files);
                    }
                }

                MessageBox.Show("All related commented code has been removed.");
                commentedCode = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message.ToString());
            }
        }

        private List<CodeStructure> GetCommendCode()
        {
            List<CodeStructure> commentedCode = new List<CodeStructure>();

            List<AppliedRule> appliedRuleList = XMLParserConversion.Common.AppliedRuleList;
            if (appliedRuleList != null && appliedRuleList.Count > 0)
            {
                #region Move to Project

                List<string> projects = new List<string>();
                foreach (var item in appliedRuleList)
                {
                    if (item.CodeDocument.IsCommented)
                    {
                        commentedCode.Add(item.CodeDocument);
                    }
                }
                // Sort array
                List<CodeStructure> sortedCodeStructure = (from s in commentedCode
                                                           orderby s.LineNumber descending
                                                           select s).ToList<CodeStructure>();

                return sortedCodeStructure;

                #endregion Move to Project
            }
            return null;
        }
    }

    [ComVisibleAttribute(true)]
    internal interface IPTFConfigEditorUIControl
    {
        // EMPTY
    }
}