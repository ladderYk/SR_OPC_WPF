using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_OPC_WPF.Client
{
    public interface IClient
    {
        void ConnectServer();
        void DisConnected();
        byte[] ReadByte(string address, ushort iLen);
        bool SendBool(string address, bool data);
    }
}
