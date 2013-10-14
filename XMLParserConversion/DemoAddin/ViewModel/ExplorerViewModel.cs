using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;

namespace DemoAddin.LoadOnDemand
{
    /// <summary>
    /// The ViewModel for the TreeListControl demo.  
    /// </summary>
    public class ExplorerViewModel
    {
        private readonly ReadOnlyCollection<ProtocolViewModel> protocols;

        public ExplorerViewModel(List<string> level1List, string spanTime)
        {
            protocols = new ReadOnlyCollection<ProtocolViewModel>(
                (from protocol in level1List
                 select new ProtocolViewModel(protocol, spanTime))
                .ToList());
        }

        public ReadOnlyCollection<ProtocolViewModel> Protocols
        {
            get { return protocols; }
        }
    }
}