using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication3.Mcu
{
    public class ModbusUdp
    {
        public class SharedMemory
        {
            public byte[] Ram;
            public byte Addr;
            public byte Len;

            public SharedMemory(byte SetAddr, byte SetLen)
            {
                Ram = new byte[SetLen];
                Addr = SetAddr;
                Len = SetLen;
            }

            public byte[] Get()
            {
                return Ram;
            }

            public void Set(byte[] SetRam)
            {
                Ram = SetRam;
            }
        };

        public static SharedMemory r_rom_uuid = new SharedMemory(0, 12);  //addr:0;len:12
        public static SharedMemory rw_rom_date = new SharedMemory(12, 32); //addr:12;len:32
        public static SharedMemory rw_ram_high = new SharedMemory(44, 3);  //addr:44;len:3
        public static SharedMemory rw_ram_sp = new SharedMemory(47, 2);  //addr:47;len:2
        public static SharedMemory rw_ram_addr = new SharedMemory(49, 1);  //addr:49;len:1

        public static byte[] MBReqUuid()
        {
            byte[] Data = new byte[1];
            Data[0] = 0; //read
            return GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.GetId, Data));
        }

        public static byte[] MBReqWrite( byte[] Data)
        {
            return GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.Write, Data));
        }

        public static byte[] MBRsp(byte[] Adu)
        {
            byte Address;
            byte FunctionCode;

            if (CheckCRC(Adu) == true)
            {
                Address = GetAddress(Adu);
                switch (Address)
                {
                    case (byte)MBAddress.Broadcast:
                        break;
                    case (byte)MBAddress.Reserve:
                        FunctionCode = GetFunctionCode(Adu);
                        switch (FunctionCode)
                        {
                            case (byte)MBFunctionCode.Connect:
                                return GetData(Adu);                                
                            case (byte)MBFunctionCode.Monitor:
                                break;
                            case (byte)MBFunctionCode.Read:
                                return GetData(Adu);
                            case (byte)MBFunctionCode.Write:
                                return GetData(Adu);
                            case (byte)MBFunctionCode.ReadChip:
                                return GetData(Adu);
                            case (byte)MBFunctionCode.WriteChip:
                                return GetData(Adu);
                            case (byte)MBFunctionCode.Control:
                                return GetData(Adu);
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }

            return null;
        }

        public enum MBFunctionCode : byte  //显示指定枚举的底层数据类型
        {
            Connect = 100,  //100
            Monitor = 101,
            Read = 102,
            Write = 103,
            ReadChip=104,
            WriteChip=105,
            GetId=106,
            GetTimeCode=107,
            Control = 200,
        }

        enum MBAddress : byte  //显示指定枚举的底层数据类型
        {
            Broadcast = 0x00,
            Reserve = 0xff
        }

        //public static byte[] MBReqConnect = GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.Connect, null));

                   
        /// <summary>
        /// 发送连接请求
        /// </summary>
        /// <returns></returns>
        public static byte[] MBReqConnect()
        {

            int password = 123;
            byte[] Data = System.BitConverter.GetBytes(password);
            Array.Reverse(Data);

            return  GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.Connect, Data));
        }

        /// <summary>
        /// 返回Read的ADU
        /// </summary>
        /// <param name="Data">地址+长度+数据</param>
        /// <returns></returns>
        public static byte[] MBReqRead(byte [] Data)
        {
           
            return GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.Read, Data));
        }


        /// <summary>
        /// 将地址跟数据长度和数据合并成一个数组并返回
        /// </summary>
        /// <param name="addr">地址</param>
        /// <param name="len">数据长度</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static byte[] ArrayAdd(ushort addr, ushort len,byte [] data)
        {
            byte[] Data = new byte[4+data.Length];

            byte[] byteAddr = System.BitConverter.GetBytes(addr);
            Array.Reverse(byteAddr);
            byte[] byteLen = System.BitConverter.GetBytes(len);
            Array.Reverse(byteLen);
            byteAddr.CopyTo(Data, 0);
            byteLen.CopyTo(Data, byteAddr.Length);
            data.CopyTo(Data,byteAddr.Length +byteLen.Length);
            return Data;

        }

        /// <summary>
        /// 返回ReadChip的ADU
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static byte[] MBReqReadChip(byte[] Data)
        {
           
            return GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.ReadChip, Data));
        }


        /// <summary>
        /// 返回WriteChip的ADU
        /// </summary>
        /// <param name="Data"></param>
        /// <returns></returns>
        public static byte[] MBReqWriteChip( byte[] Data)
        {
           
            return GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.WriteChip, Data));
        }



        public static byte[] MBReqMonitor(byte MonitorTick)
        {
            byte[] Data = new byte[1];
            Data[0] = MonitorTick;
            return GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.Monitor, Data));
        }

        public static byte[] MBReqGetTimeCode(byte [] Data)
        {
            
            return GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.GetTimeCode, Data));
        }


        public static byte[] MBReqControl(byte RW, byte Addr, byte Len, byte[] Data)
        {
            if (RW == 0) //read
            {
                byte[] newData = new byte[3];
                newData[0] = RW;
                newData[1] = Addr;
                newData[2] = Len;
            }
            else //write
            {
                byte[] newData = new byte[3 + Len];
                newData[0] = RW;
                newData[1] = Addr;
                newData[2] = Len;
                Array.Copy(Data, 0, newData, 3, Len);
            }
            return GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.Control, Data));
        }


        public static byte[] MBReqControlTest()
        {
            byte[] WriteData = new byte[1];
            WriteData[0] = 0xbb;
            byte[] Data = new byte[3 + 1];
            Data[0] = 1;
            Data[1] = 0;
            Data[2] = 1;
            Array.Copy(WriteData, 0, Data, 3, 1);
            return GetADU((byte)MBAddress.Reserve, GetPDU((byte)MBFunctionCode.Control, Data));
        }

        public static bool MBrspConnect(byte[] ADU)
        {
            if (CheckCRC(ADU) == true)
            {
                if (GetAddress(ADU) == (byte)MBAddress.Reserve)
                {
                    if (GetFunctionCode(ADU) == (byte)MBFunctionCode.Connect)
                    {
                        if (GetData(ADU) == null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static byte GetAddress(byte[] ADU)
        {
            if (ADU != null)
            {
                if (ADU.Length > 0)
                {
                    return ADU[0];
                }
            }
            return 0;
        }

        public static byte GetFunctionCode(byte[] ADU)
        {
            if (ADU != null)
            {
                if (ADU.Length > 1)
                {
                    return ADU[1];
                }
            }
            return 0;
        }

        public static byte[] GetData(byte[] ADU)
        {
            if (ADU != null)
            {
                if (ADU.Length > 4)
                {
                    byte[] Data = new byte[ADU.Length - 4];
                    Array.Copy(ADU, 2, Data, 0, (ADU.Length - 4));
                    return Data;
                }
            }
            return null;
        }

        public static bool CheckCRC(byte[] ADU)
        {
            ushort Crc16;
            if (ADU != null)
            {
                if (ADU.Length >= 2)
                {
                    Crc16 = (ushort)(ADU[ADU.Length - 2] << 8 | ADU[ADU.Length - 1]);
                    if (Crc16 == CRC16.Get(ADU, (ADU.Length - 2)))
                    {
                        return true;
                    }
                }
            }
            return true;
        }

        public static byte[] GetPDU(byte FunctionCode, byte[] Data)
        {
            byte[] PDU;
            if (Data == null)
            {
                PDU = new byte[1];
                PDU[0] = FunctionCode;
            }
            else
            {
                PDU = new byte[1 + Data.Length];
                PDU[0] = FunctionCode;
                Array.Copy(Data, 0, PDU, 1, Data.Length);
            }
            return PDU;
        }

        public static byte[] GetADU(byte Address, byte[] PDU)
        {
            ushort Crc16;
            byte[] ADU = new byte[1 + PDU.Length + 2];
            ADU[0] = Address;
            Array.Copy(PDU, 0, ADU, 1, PDU.Length);
            Crc16 = CRC16.Get(ADU, (ADU.Length - 2));
            ADU[1 + PDU.Length] = (byte)Crc16;
            ADU[1 + PDU.Length + 1] = (byte)(Crc16 >> 8);
            return ADU;
        }

        public static byte[] GetConnectFrame(byte Address, byte FunctionCode, byte[] Data)
        {
            return GetADU(Address, GetPDU(FunctionCode, Data));
        }

        public static string ByteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                    returnStr += " ";
                }
            }
            return returnStr;
        }
    }
    class CRC16
    {
        private static byte[] auchCRCHi = {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1,
            0x81, 0x40, 0x01,
            0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01,
            0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80,
            0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01,
            0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00,
            0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81,
            0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1,
            0x81, 0x40, 0x01,
            0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01,
            0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81,
            0x40, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01,
            0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01,
            0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81,
            0x40
        };

        private static byte[] auchCRCLo = {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06, 0x07, 0xC7,
            0x05, 0xC5, 0xC4,
            0x04, 0xCC, 0x0C, 0x0D, 0xCD, 0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB,
            0x0B, 0xC9, 0x09,
            0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A, 0x1E, 0xDE,
            0xDF, 0x1F, 0xDD,
            0x1D, 0x1C, 0xDC, 0x14, 0xD4, 0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2,
            0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3, 0xF2, 0x32,
            0x36, 0xF6, 0xF7,
            0x37, 0xF5, 0x35, 0x34, 0xF4, 0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E,
            0xFE, 0xFA, 0x3A,
            0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29, 0xEB, 0x2B,
            0x2A, 0xEA, 0xEE,
            0x2E, 0x2F, 0xEF, 0x2D, 0xED, 0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27,
            0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60, 0x61, 0xA1,
            0x63, 0xA3, 0xA2,
            0x62, 0x66, 0xA6, 0xA7, 0x67, 0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD,
            0x6D, 0xAF, 0x6F,
            0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68, 0x78, 0xB8,
            0xB9, 0x79, 0xBB,
            0x7B, 0x7A, 0xBA, 0xBE, 0x7E, 0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4,
            0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71, 0x70, 0xB0,
            0x50, 0x90, 0x91,
            0x51, 0x93, 0x53, 0x52, 0x92, 0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94,
            0x54, 0x9C, 0x5C,
            0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B, 0x99, 0x59,
            0x58, 0x98, 0x88,
            0x48, 0x49, 0x89, 0x4B, 0x8B, 0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D,
            0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42, 0x43, 0x83,
            0x41, 0x81, 0x80,
            0x40
        };

        public static ushort Get(byte[] CRCData, int usDataLen) /* 函数以unsigned short 类型返回CRC */
        {
            byte puchMsg = CRCData[0];
            int i = 0;
            byte uchCRCHi = 0xFF; /* CRC 的高字节初始化*/
            byte uchCRCLo = 0xFF; /* CRC 的低字节初始化*/
            byte uIndex; /* CRC 查询表索引*/
            while (usDataLen != 0) /* 完成整个报文缓冲区*/
            {
                usDataLen--;
                uIndex = (byte)(uchCRCLo ^ puchMsg); /* 计算CRC */
                i++;
                puchMsg = CRCData[i];
                uchCRCLo = (byte)(uchCRCHi ^ auchCRCHi[uIndex]);
                uchCRCHi = auchCRCLo[uIndex];
            }
            return (ushort)(uchCRCHi << 8 | uchCRCLo);
        }
    }
}
