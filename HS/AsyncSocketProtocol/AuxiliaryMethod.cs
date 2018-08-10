using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HS_Socket
{
    public static class AuxiliaryMethod
    {
        /// 将16进制数转变为string
        /// </summary>
        /// <param name="bts">16进制数组</param>
        /// <returns>bts_string</returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 十六进制字符串转换成十进制字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string str16Tostr10(string str)
        {
            string returnStr = "";
            if (str != null)
            {
                for (int i = 0; i < str.Length / 2; i++)
                {
                    returnStr += Int32.Parse(str.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 十进制字符串转换为十六进制字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string str10Tostr16(string str)
        {
            string returnStr = "";
            if (str != null)
            {
                for (int i = 0; i < str.Length / 2; i++)
                {
                    returnStr += int.Parse(str.Substring(i * 2, 2)).ToString("x2");
                }
            }
            return returnStr;
        }

        /// <summary>
        /// 将4个int字节转换为byte数组
        /// </summary>
        /// <param name="dat">转换目标</param>
        /// <returns>返回一个byte数组</returns>
        public static byte[] intTo4Bytes(int dat)
        {
            byte[] byteData = BitConverter.GetBytes(dat);
            if (BitConverter.IsLittleEndian)
            {
                byte[] tmp = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    tmp[i] = byteData[3 - i];
                }
                return tmp;
            }
            return byteData;
        }
        /// <summary>
        /// 将2个int类型转换为byte数组
        /// </summary>
        /// <param name="dat">转换目标</param>
        /// <returns>返回一个byte数组</returns>
        public static byte[] intTo2Bytes(int dat)
        {
            byte[] byteData = BitConverter.GetBytes((short)dat);
            if (BitConverter.IsLittleEndian)
            {
                byte[] tmp = new byte[2];
                for (int i = 0; i < 2; i++)
                {
                    tmp[i] = byteData[1 - i];
                }
                return tmp;
            }
            return byteData;
        }
        /// <summary>
        /// 将bytep[4] 数组转换为int类型
        /// </summary>
        /// <param name="bytes">数组名称</param>
        /// <param name="start">开始的index</param>
        /// <returns>返回一个int类型</returns>
        public static int byte4ToInt(byte[] bytes, int start)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] tmp = new byte[4];
                tmp[0] = bytes[start + 3];
                tmp[1] = bytes[start + 2];
                tmp[2] = bytes[start + 1];
                tmp[3] = bytes[start];
                return BitConverter.ToInt32(tmp, 0);
            }
            else
            {
                return BitConverter.ToInt32(bytes, start);
            }
        }
        /// <summary>
        /// 将bytep[2] 数组转换为int类型
        /// </summary>
        /// <param name="bytes">数组名称</param>
        /// <param name="start">开始的index</param>
        /// <returns>返回一个int类型</returns>
        public static int byte2ToInt(byte[] bytes, int start)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] tmp = new byte[2];
                tmp[0] = bytes[start + 1];
                tmp[1] = bytes[start];
                return BitConverter.ToInt16(tmp, 0);
            }
            else
            {
                return BitConverter.ToInt16(bytes, start);
            }
        }
        #region C# CRC16校验算法
        /// <summary>
        /// C# CRC16校验算法
        /// </summary>
        /// <param name="data"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte[] crc16(byte[] data, int len)
        {
            byte[] temdata = new byte[2];
            int xda, xdapoly;
            int i, j, xdabit;
            xda = 0xFFFF;
            xdapoly = 0xA001;
            for (i = 0; i < len; i++)
            {
                xda ^= data[i];
                for (j = 0; j < 8; j++)
                {
                    xdabit = (int)(xda & 0x01);
                    xda >>= 1;
                    if (xdabit == 1)
                        xda ^= xdapoly;
                }
            }
            temdata[0] = (byte)(xda & 0x00FF);//低字节
            temdata[1] = (byte)(xda >> 8);//高字节
            return temdata;
        }
        #endregion
        #region CRC16校验
        /// <summary>
        /// CRC16校验
        /// </summary>
        /// <param name="data">要进行计算的数组</param>
        /// <returns>计算后的数组</returns>
        public static byte[] CRC16(byte[] data)
        {
            byte[] returnVal = new byte[2];
            byte CRC16Lo, CRC16Hi, CL, CH, SaveHi, SaveLo;
            int i, Flag;
            CRC16Lo = 0xFF;
            CRC16Hi = 0xFF;
            CL = 0x86;
            CH = 0x68;
            for (i = 0; i < data.Length; i++)
            {
                CRC16Lo = (byte)(CRC16Lo ^ data[i]);//每一个数据与CRC寄存器进行异或
                for (Flag = 0; Flag <= 7; Flag++)
                {
                    SaveHi = CRC16Hi;
                    SaveLo = CRC16Lo;
                    CRC16Hi = (byte)(CRC16Hi >> 1);//高位右移一位
                    CRC16Lo = (byte)(CRC16Lo >> 1);//低位右移一位
                    if ((SaveHi & 0x01) == 0x01)//如果高位字节最后一位为
                    {
                        CRC16Lo = (byte)(CRC16Lo | 0x80);//则低位字节右移后前面补 否则自动补0
                    }
                    if ((SaveLo & 0x01) == 0x01)//如果LSB为1，则与多项式码进行异或
                    {
                        CRC16Hi = (byte)(CRC16Hi ^ CH);
                        CRC16Lo = (byte)(CRC16Lo ^ CL);
                    }
                }
            }
            returnVal[0] = CRC16Lo;//CRC低位
            returnVal[1] = CRC16Hi;//CRC高位
            return returnVal;
        }
        #endregion
        #region 字节转换
        /// <summary>
        /// 把一个存储16进制数的字符串转化为存储16进制数的字节数组
        /// </summary>
        /// <param name="HexString">存储16进制数的字符串</param>
        /// <returns>返回一个字节数组</returns>
        public static byte[] StringToBytes(string HexString)
        {
            byte[] temdata = new byte[HexString.Length / 2];
            for (int i = 0; i < temdata.Length; i++)
            {
                temdata[i] = Convert.ToByte(HexString.Substring(i * 2, 2), 16);
            }
            return temdata;
        }
        /// <summary>
        /// 把一个存储Hex数据的数组转换为string
        /// </summary>
        /// <param name="bytes">字节数组</param>
        /// <returns>string类型数据</returns>
        public static string ToHexString(byte[] bytes) // 如0xae00cf => "AE00CF "  
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
        #endregion
        #region 数组数据处理
        /// <summary>
        /// 合并数组
        /// </summary>
        /// <param name="First">第一个数组</param>
        /// <param name="Second">第二个数组</param>
        /// <returns>合并后的数组(第一个数组+第二个数组，长度为两个数组的长度)</returns>
        public static string[] MergerArray(string[] First, string[] Second)
        {
            string[] result = new string[First.Length + Second.Length];
            First.CopyTo(result, 0);
            Second.CopyTo(result, First.Length);
            return result;
        }
        /// <summary>
        /// 数组追加
        /// </summary>
        /// <param name="Source">原数组</param>
        /// <param name="str">字符串</param>
        /// <returns>合并后的数组(数组+字符串)</returns>
        public static string[] MergerArray(string[] Source, string str)
        {
            string[] result = new string[Source.Length + 1];
            Source.CopyTo(result, 0);
            result[Source.Length] = str;
            return result;
        }
        /// <summary>
        /// 从数组中截取一部分成新的数组
        /// </summary>
        /// <param name="Source">原数组</param>
        /// <param name="StartIndex">原数组的起始位置</param>
        /// <param name="EndIndex">原数组的截止位置</param>
        /// <returns></returns>
        public static string[] SplitArray(string[] Source, int StartIndex, int EndIndex)
        {
            try
            {
                string[] result = new string[EndIndex - StartIndex + 1];
                for (int i = 0; i <= EndIndex - StartIndex; i++) result[i] = Source[i + StartIndex];
                return result;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// byte字节数组截取
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="StartIndex"></param>
        /// <param name="EndIndex"></param>
        /// <returns></returns>
        public static byte[] SplitbyteArray(byte[] Source, int StartIndex, int EndIndex)
        {
            try
            {
                byte[] result = new byte[EndIndex - StartIndex + 1];
                for (int i = 0; i <= EndIndex - StartIndex; i++) result[i] = Source[i + StartIndex];
                return result;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 不足长度的前面补空格,超出长度的前面部分去掉
        /// </summary>
        /// <param name="First">要处理的数组</param>
        /// <param name="byteLen">数组长度</param>
        /// <returns></returns>
        public static string[] MergerArray(string[] First, int byteLen)
        {
            string[] result;
            if (First.Length > byteLen)
            {
                result = new string[byteLen];
                for (int i = 0; i < byteLen; i++) result[i] = First[i + First.Length - byteLen];
                return result;
            }
            else
            {
                result = new string[byteLen];
                for (int i = 0; i < byteLen; i++) result[i] = " ";
                First.CopyTo(result, byteLen - First.Length);
                return result;
            }
        }
        #endregion
    }
}
