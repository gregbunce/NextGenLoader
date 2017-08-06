using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Geodatabase;

namespace NexGenRoadLoader.contracts
{
    public interface ILoader
    {
        void Load(IWorkspace output);
        IWorkspace GetOutputWorkspace();
    }
}
