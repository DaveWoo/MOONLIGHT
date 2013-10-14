using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace DemoAddin
{
    /// <summary>
    /// Interaction logic for XSDPanel.xaml
    /// </summary>
    public partial class XSDPanel : Window
    {
        public XSDPanel()
        {
            InitializeComponent();
        }

        private XSDPareInfo xsdInfo = null;
        public XSDPareInfo XSDInfo
        {
            get
            {
                return xsdInfo;
            }
        }
        public void InitialData(string xsdPath)
        {
            try
            {
                Root root = Load<Root>(xsdPath);
                List<XSDPareInfo> TodoItem2 = new List<XSDPareInfo>();

                if (root != null)
                {
                    foreach (var item in root.Items)
                    {
                        if (item.XSD.Count() > 1)
                        {
                            TodoItem2.Add(
                                new XSDPareInfo()
                                {
                                    SourceFullPath = item.XSD.FirstOrDefault().type == "source" ? item.XSD.FirstOrDefault().Value : string.Empty,
                                    ChangedFullPath = item.XSD.LastOrDefault().type == "changed" ? item.XSD.LastOrDefault().Value : string.Empty,
                                });
                        }

                    }
                }

                lbTodoList.ItemsSource = TodoItem2;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T Load<T>(string xPath)
        {
            T p;
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(T));
                FileStream fileStream = new FileStream(xPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                p = (T)xs.Deserialize(fileStream);

                fileStream.Close();
            }
            catch (Exception)
            {
                throw;
            }
            return p;
        }

        public class XSDPareInfo
        {
            public string Source
            {
                get
                {
                    return System.IO.Path.GetFileName(SourceFullPath);
                }
            }
            public string Changed
            {
                get
                {
                    return System.IO.Path.GetFileName(ChangedFullPath);
                }
            }

            public string SourceFullPath { get; set; }
            public string ChangedFullPath { get; set; }
        }

        private void lbTodoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox != null)
            {
                xsdInfo = listBox.SelectedItem as XSDPareInfo;
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (xsdInfo == null)
            {
                MessageBox.Show("Please select xsd file and then click button \"OK\"", "XSD Panel", MessageBoxButton.OK);
                return;
            }
            this.Close();
        }
    }
}