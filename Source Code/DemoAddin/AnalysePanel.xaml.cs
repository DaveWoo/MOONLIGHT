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
    using DemoAddin.LoadOnDemand;
    using EnvDTE;
    using EnvDTE80;
    using XinYu.XSD2Code;
    using XinYu.SOMDiff;

    /// <summary>
    /// Interaction logic for AnalysePanel.xaml
    /// </summary>
    public partial class AnalysePanel : UserControl, IPTFConfigEditorUIControl
    {
        private List<CodeStructure> commentedCode = null;
        private bool needToDelete = false;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private string souceXSDFile = string.Empty;
        private string changedXSDFile = string.Empty;

        public AnalysePanel()
        {
            InitializeComponent();
            this.Initial();
        }

        private void Initial()
        {
            // this.btnRun.IsEnabled = false;
            this.btnRemove.IsEnabled = false;
            string souceXSDFile = string.Empty;
            string changedXSDFile = string.Empty;
        }

        public void Refresh()
        {
        }

        public DTE2 ApplicationObject { get; set; }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region Step 1 - Select xsd file
                // string xsdPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MatchedXSDReq.xml");
                string xsdPath = System.IO.Path.Combine(@"D:\360云盘\GitHub\MOONLIGHT\XSDConversion\DemoAddin\bin\Debug", "MatchedXSDReq.xml");

                XSDPanel xsdPanel = new XSDPanel();
                xsdPanel.InitialData(xsdPath);
                xsdPanel.ShowDialog();

                string filePath = DataModel.ApplicationObject.Solution.FullName;
                  filePath = @"D:\O15\TestSuites\Exchange\Platinum\ExchangeServerEASProtocolTestSuites\Exchange Server EAS Protocol Test Suites\Source\MS-ASCMD - Copy\MS-ASCMD.sln";
                if (xsdPanel.XSDInfo == null)
                {
                    MessageBox.Show("NO xsd file was selected, Please select a couple xsd files firstly.", "Demo", MessageBoxButton.OK);
                    return;
                }

                souceXSDFile = xsdPanel.XSDInfo.SourceFullPath;
                changedXSDFile = xsdPanel.XSDInfo.ChangedFullPath;

                #endregion

                #region Step 2 - Build

                //BuildMode mode = BuildMode.Debug;
                //BuildResultCode actual = Common.BuildSolution(filePath, mode, @"BuildLog.txt");

                //if (BuildResultCode.Success != actual)
                //{
                //    throw new Exception("Build Exception occurred.");
                //}

                #endregion Step 1 - Build

                #region Step 3 - Anaylse

                #region SOMDiff invocation

                SOMDiff.Initial();
                this.SOMDiffFiles(Path.GetDirectoryName(souceXSDFile), Path.GetDirectoryName(changedXSDFile), souceXSDFile, changedXSDFile);
                List<MismatchedPair> result = SOMDiff.Result;
                if (result != null && result.Count > 0)
                {
                    MessageBox.Show(string.Format("Source:{0}\\\r\nChanged:{1}\r\nCount: {2}", souceXSDFile, changedXSDFile, result.Count));
                }
                else if (result != null && result.Count == 0)
                {
                    this.NavigateTree.DataContext = null;
                    MessageBox.Show(string.Format("Source:{0}\\\r\nChanged:{1}\r\nCount: {2}", souceXSDFile, changedXSDFile, result.Count));
                    return;
                }
                #endregion Step 1: SOMDiff invocation
                System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                Analyze analyze = new Analyze();
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    MessageBox.Show("Please select one solution file firstly.", "Demo", MessageBoxButton.OK);
                    return;
                }
                analyze.Initialize(filePath);
                analyze.Run(this.souceXSDFile, this.changedXSDFile, result);
                watch.Stop();
                List<AppliedRule> appliedRuleList = XinYu.XSD2Code.Common.AppliedRuleList;

                if (appliedRuleList != null && appliedRuleList.Count == 0)
                {
                    this.NavigateTree.DataContext = null;
                }
                else if (appliedRuleList != null && appliedRuleList.Count > 0)
                {
                    #region Generate Project

                    #region Get all project name

                    List<string> projects = null;
                    foreach (var item in appliedRuleList)
                    {
                        if (item.CodeDocument == null)
                        {
                            continue;
                        }
                        projects = new List<string>();
                        string projectName = item.CodeDocument.ProjectName;
                        if (!projects.Contains(projectName))
                        {
                            projects.Add(projectName);
                        }
                    }

                    #endregion Get all project name

                    // Get Level 1 data
                    if (projects == null)
                    {
                        projects = new List<string>();
                        projects.Add(Path.GetFileNameWithoutExtension(filePath));
                    }
                    List<string> projectsList = DataModel.GetProtocols(projects);

                    #endregion Generate Project

                    #region Generate ExplorerViewModel and Bing Data

                    ExplorerViewModel viewModel = new ExplorerViewModel(projectsList, watch.Elapsed.ToString());
                    this.NavigateTree.DataContext = viewModel;

                    #endregion Generate ExplorerViewModel and Bing Data
                }

                // Get commented code
                commentedCode = GetCommendCode();

                #endregion Step 2 - Anaylse
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message.ToString());
            }
            MessageBox.Show("Run Completed.", "Demo", MessageBoxButton.OK);
        }

        private void SOMDiffFiles(string sourcepath, string changepath, string sourefile, string changefile)
        {
            if (string.IsNullOrWhiteSpace(sourcepath))
            {
                throw new ArgumentNullException(sourcepath);
            }

            if (string.IsNullOrWhiteSpace(changepath))
            {
                throw new ArgumentNullException(changepath);
            }
            if (string.IsNullOrWhiteSpace(sourefile))
            {
                throw new ArgumentNullException(sourefile);
            }
            if (string.IsNullOrWhiteSpace(changefile))
            {
                throw new ArgumentNullException(changefile);
            }

            if (!System.IO.File.Exists(sourefile))
            {
                throw new Exception("File not exist: " + sourefile);
            }
            if (!System.IO.File.Exists(changefile))
            {
                throw new Exception("File not exist: " + changefile);
            }

            try
            {
                SOMDiff sdiff = new SOMDiff();

                sdiff.ParseSchemaDependency(sourcepath);
                sdiff.ParseSchemaDependency(changepath);

                sdiff.DiffSchemas(sourefile, changefile);
            }
            catch (Exception)
            {
                throw;
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

                    if (string.IsNullOrWhiteSpace(detailInfo.FullPath))
                    {
                        return;
                    }
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
                    int line = detailInfo.LineNumber;
                    EnvDTE.TextSelection ts = (EnvDTE.TextSelection)projectItem.Document.Selection;
                    ts.MoveToLineAndOffset(line, 1, false);
                    ts.EndOfLine(false);
                    ts.MoveToLineAndOffset(line, 1, true);
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
            foreach (EnvDTE.Project project in DataModel.ApplicationObject.Solution.Projects)
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

        private void btnRemoveClick(object sender, RoutedEventArgs e)
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

            List<AppliedRule> appliedRuleList = XinYu.XSD2Code.Common.AppliedRuleList;
            if (appliedRuleList != null && appliedRuleList.Count > 0)
            {
                #region Move to Project

                List<string> projects = new List<string>();
                foreach (var item in appliedRuleList)
                {
                    if (item.CodeDocument == null)
                    {
                        continue;
                    }

                    commentedCode.Add(item.CodeDocument);
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