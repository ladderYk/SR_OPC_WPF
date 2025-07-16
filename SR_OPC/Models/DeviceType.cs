using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_OPC_WPF.Models
{
   public class DeviceType
    {
        public Guid ID;
        public string Name;
        public object[] Type;
        public bool IsBig;
        public TypeRead[] Read;
        public TypeWrite[] Write;
        public TypeConfig[] Config;
    }
    public class TypeRead
    {
        public string Addr;
        public int Len;
        public string Tag;
        public string Name;
    }
    public class TypeReadVal
    {
        public string Tag;
        public string Name;
        public object Data;

    }
    public class TypeWrite
    {
        public string Addr;
        public string Tag;
    }
    public class TypeConfig
    {
        public string ID;
        public string Addr;
        public string Name;
        public int Offset;
        public int DType;
        public bool Disable;
    }
}
