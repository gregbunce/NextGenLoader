using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace NexGenRoadLoader.models
{

    public class SpatialFieldValues: IDisposable
    {
        public void Dispose()
        {
        }

        public string IncMuni_R { get; set; }
        public string IncMuni_L { get; set; }
        public string UnIncMuni_R { get; set; }
        public string UnIncMuni_L { get; set; }
        public string Zip_R { get; set; }
        public string Zip_L { get; set; }
        public string PostalComm_R { get; set; }
        public string PostalComm_L { get; set; }
        public string County_R { get; set; }
        public string County_L { get; set; }
        public string AddrSystem_R { get; set; }
        public string AddrSystem_L { get; set; }
        public string AddrSystemQuad_R { get; set; }
        public string AddrSystemQuad_L { get; set; }
        public string Esn_R { get; set; }
        public string Esn_L { get; set; }
        public string MsagGeo_R { get; set; }
        public string MsagGeo_L { get; set; }

    }
}
