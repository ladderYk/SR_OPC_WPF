using SR_OPC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SR_OPC_WPF
{
    class S7CmdUtil
    {
        public static int CmdNum;

        public static int GetCmdNum(int cmdNum)
        {
            cmdNum = ((cmdNum < int.MaxValue) ? (cmdNum + 1) : 0);
            return cmdNum;
        }

        public static byte[] AGVRegistertCmd(string agvid)
        {
            return GetCmdBytes(agvid, 0, 0);
        }

        public static byte[] AGVStopCmd(string agvid)
        {
            return GetCmdBytes(agvid, 6, 0);
        }

        public static byte[] AGVStartCmd(string agvid, ushort type)
        {
            return GetCmdBytes(agvid, 5, type);
        }

        public static byte[] AGVTurnCmd(string agvid, ushort type)
        {
            return GetCmdBytes(agvid, 7, type);
        }

        public static byte[] LiftCtrlCmd(string agvid, ushort type)
        {
            return GetCmdBytes(agvid, 10, type);
        }

        public static byte[] AGVStateCmd(string agvid)
        {
            return GetCmdBytes(agvid, 2, 0);
        }

        private static byte[] GetCmdBytes(string agvid, int cmdcode, ushort type = 0)
        {
            byte[] vliadBytes = GetVliadBytes(cmdcode, type);
            CmdNum = GetCmdNum(CmdNum);
            return GetByte(Convert.ToInt32(agvid), vliadBytes, CmdNum);
        }

        private static byte[] GetByte(int agvid, byte[] validbuf, int cmdNum)
        {
            byte[] array = new byte[26];
            array[0] = 03;
            array[1] = Convert.ToByte(agvid);
            cmdNum.ToString("X4");
            byte[] array2 = HexStrTobyte(cmdNum.ToString("X4")).Reverse().ToArray();
            for (int i = 0; i < array2.Length; i++)
            {
                array[i + 2] = array2[i];
            }
            for (int j = 0; j < validbuf.Length; j++)
            {
                array[j + 4] = validbuf[j];
            }
            array[24] = Convert.ToByte(GetVerifyValue(ValidValueSum(validbuf, array2), agvid));
            array[25] = 170;
            return array;
        }

        private static int GetVerifyValue(int val, int agvid)
        {
            int result = 0;
            for (int i = 0; i < 256; i++)
            {
                if ((val + agvid + i) % 256 == 0)
                {
                    result = i;
                    break;
                }
            }
            return result;
        }

        private static int ValidValueSum(byte[] buf, byte[] cmdBuf)
        {
            int num = 0;
            byte[] array = buf;
            foreach (byte b in array)
            {
                num += b;
            }
            array = cmdBuf;
            foreach (byte b2 in array)
            {
                num += b2;
            }
            return num;
        }
        private static byte[] GetCotpBytes(int commandcode, ushort type)
        {
            return new byte[20]
            {
                Convert.ToByte(commandcode),
                Convert.ToByte(type),
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0
            };
        }
        public static byte[] Test()
        {
            List<byte> reqBytes = new List<byte>();
            // TPKT
            reqBytes.Add(0x03);
            reqBytes.Add(0x00);
            // 初始化无意义，只做点位，后续做修改  ，注意第161行
            reqBytes.Add(0x00);
            reqBytes.Add(0x00);// 注意下后台再修改---
                               // COTP
            reqBytes.Add(0x02);
            reqBytes.Add(0xf0);
            reqBytes.Add(0x80);   // 1000 0000
                                  // S7-Header
            reqBytes.Add(0x32);
            reqBytes.Add(0x01);// ROSCTR

            reqBytes.Add(0x00);
            reqBytes.Add(0x00);
            reqBytes.Add(0x00);
            reqBytes.Add(0x00);

            reqBytes.Add(0x00);
            reqBytes.Add(0x26);// Paremeter部分字节长度。注意后面修改    14   26   1A     38  26

            reqBytes.Add(0x00);
            reqBytes.Add(0x00);// Data部分字节长度

            // S7-Parameter
            reqBytes.Add(0x04);// Function
            reqBytes.Add(0x03);// Item的个数

            reqBytes.AddRange(ItemQ0_4());// 组装请求Q0.4的Item
                                          //reqBytes.AddRange(ItemMB10());// 组装请求MB10的Item
                                          //reqBytes.AddRange(ItemVW100());// 组装请求VW100的Item
                                          //reqBytes.AddRange(ItemVD0());// 组装请求VD0的Item
                                          //reqBytes.AddRange(ItemDBD2());// 组装请求VD0的Item

            ushort len = (ushort)reqBytes.Count;
            byte[] lenBytes = BitConverter.GetBytes(len);// 小端
            reqBytes[2] = lenBytes[1];
            reqBytes[3] = lenBytes[0];

            return reqBytes.ToArray();
        }
        // Q0.4  位  Q区0号字节中的4号位
        static List<byte> ItemQ0_4()
        {
            List<byte> items = new List<byte>();
            items.Add(0x12);

            items.Add(0x0a);
            items.Add(0x10);
            items.Add(0x01);

            items.Add(0x00);
            items.Add(0x01);//请求一个

            items.Add(0x00);
            items.Add(0x00);

            items.Add(0x82);

            // 地址计算
            int byteAddr = 0;
            byte bitAddr = 4;
            byteAddr = (byteAddr << 3) + bitAddr;

            //BitConverter.GetBytes(byteAddr);  // [0][1][2][]
            //  /256  进行计算
            items.Add((byte)(byteAddr / 256 / 256 % 256));
            items.Add((byte)(byteAddr / 256 % 256));
            items.Add((byte)(byteAddr % 256));

            return items;
        }
        private static byte[] GetVliadBytes(int commandcode, ushort type)
        {
            return new byte[20]
            {
                Convert.ToByte(commandcode),
                Convert.ToByte(type),
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0
            };
        }

        public static byte[] SetSiteCmd(string agvid, string siteid, List<byte> bytes)
        {
            return GetSiteBytes(agvid, 16, 0, siteid, bytes);
        }

        public static byte[] ClearSiteCmd(string agvid, string siteid, List<byte> bytes)
        {
            return GetSiteBytes(agvid, 16, 1, siteid, bytes);
        }

        public static byte[] ClearAllSiteCmd(string agvid, List<byte> bytes)
        {
            return GetSiteBytes(agvid, 16, 2, "0", bytes);
        }

        public static byte[] ReadSiteCmd(string agvid, string siteid, List<byte> bytes)
        {
            return GetSiteBytes(agvid, 17, 1, siteid, bytes);
        }

        private static byte[] GetSiteBytes(string agvid, int cmdcode, int actCode, string siteId, List<byte> bytes)
        {
            byte[] siteTempBytes = GetSiteTempBytes(cmdcode, actCode, siteId, bytes);
            CmdNum = GetCmdNum(CmdNum);
            return GetByte(Convert.ToInt32(agvid), siteTempBytes, CmdNum);
        }

        private static byte[] GetSiteTempBytes(int cmdCode, int actcode, string siteId, List<byte> bytes)
        {
            byte[] array = HexStrTobyte(Convert.ToByte(siteId).ToString("X4"));
            new List<byte>();
            byte[] array2 = new byte[20]
            {
                Convert.ToByte(cmdCode),
                Convert.ToByte(actcode),
                array[1],
                array[0],
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0
            };
            for (int i = 0; i < bytes.Count; i++)
            {
                array2[i + 4] = bytes[i];
            }
            return array2;
        }
    public static byte[] HexStrTobyte(string hexString)
    {
        hexString = hexString.Replace(" ", "");
        if (hexString.Length % 2 != 0)
        {
            hexString += " ";
        }
        byte[] array = new byte[hexString.Length / 2];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
        }
        return array;
    }
    }
}
