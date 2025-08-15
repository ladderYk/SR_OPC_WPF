using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_OPC_WPF.Models
{
   public class DeviceType_bak
    {
        public Guid ID;
        public string Name;
        public object[] Type;
        public bool IsBig;
        public TypeRead[] Read;
        public TypeWrite[] Write;
        public TypeConfig[] Config;
    }
    public class TypeRead_bak
    {
        public string Addr;
        public int Len;
        public string Tag;
        public string Name;
        
    }
    public class TypeReadVal_bak
    {
        public string Tag;
        public string Name;
        public object Data;

    }
    public class TypeWrite_bak
    {
        public string Addr;
        public string Tag;
    }
    public class TypeConfig_bak
    {
        public string ID;
        public string Addr;
        public string Name;
        public string Tag;
        public int Offset;
        public int DType;
        public bool Disable;
    }
}
