using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace HS
{
    public class DownloadSocketProtocol : BaseSocketProtocol
    {
        private string m_fileName;
        public string FileName { get { return m_fileName; } }
        private FileStream m_fileStream;
        private bool m_sendFile;
        private int m_packetSize;
        private byte[] m_readBuffer;

        public DownloadSocketProtocol(AsyncSocketServer asyncSocketServer, AsyncSocketUserToken asyncSocketUserToken)
            : base(asyncSocketServer, asyncSocketUserToken)
        {
            m_socketFlag = "Download";
            m_fileName = "";
            m_fileStream = null;
            m_sendFile = false;
            m_packetSize = 64 * 1024;
            lock (m_asyncSocketServer.DownloadSocketProtocolMgr)
            {
                m_asyncSocketServer.DownloadSocketProtocolMgr.Add(this);
            }
        }

        public override void Close()
        {
            base.Close();
            m_fileName = "";
            m_sendFile = false;
            if (m_fileStream != null)
            {
                m_fileStream.Close();
                m_fileStream = null;
            }
            lock (m_asyncSocketServer)
            {
                m_asyncSocketServer.DownloadSocketProtocolMgr.Remove(this);
            }
        }

        public override bool ProcessReceive(byte[] buffer, int offset, int count) //接收异步事件返回的数据，用于对数据进行缓存和分包
        {
            m_activeDT = DateTime.UtcNow;
            List<byte> DataList = new List<byte>();
            DataList.AddRange(buffer.Take(count));

            try
            {
                while (DataList.Count > 6)//(DataCount > sizeof(int))
                {
                    if (DataList[0] == 129)
                    {

                        string clientMsg = AnalyticData(DataList.Take(count).ToArray(), count);
                        DataList.RemoveRange(0, count);
                        return SendPacket(clientMsg);
                    }
                    else
                    {
                        DataList.RemoveAt(0);
                    }
                }//while结束点
            }                               //TRY
            catch (Exception e)
            {
                //以后要注释掉
                //MessageBox.Show(e.ToString());
            }

            finally
            {
                GC.Collect();
            }
            return true;
        }

        private static string AnalyticData(byte[] recBytes, int recByteLength)
        {
            if (recByteLength < 2) { return string.Empty; }

            bool fin = (recBytes[0] & 0x80) == 0x80; // 1bit，1表示最后一帧
            if (!fin)
            {
                return string.Empty;// 超过一帧暂不处理
            }

            bool mask_flag = (recBytes[1] & 0x80) == 0x80; // 是否包含掩码
            if (!mask_flag)
            {
                return string.Empty;// 不包含掩码的暂不处理
            }

            int payload_len = recBytes[1] & 0x7F; // 数据长度

            byte[] masks = new byte[4];
            byte[] payload_data;

            if (payload_len == 126)
            {
                Array.Copy(recBytes, 4, masks, 0, 4);
                payload_len = (UInt16)(recBytes[2] << 8 | recBytes[3]);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 8, payload_data, 0, payload_len);

            }
            else if (payload_len == 127)
            {
                Array.Copy(recBytes, 10, masks, 0, 4);
                byte[] uInt64Bytes = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    uInt64Bytes[i] = recBytes[9 - i];
                }
                UInt64 len = BitConverter.ToUInt64(uInt64Bytes, 0);

                payload_data = new byte[len];
                for (UInt64 i = 0; i < len; i++)
                {
                    payload_data[i] = recBytes[i + 14];
                }
            }
            else
            {
                Array.Copy(recBytes, 2, masks, 0, 4);
                payload_data = new byte[payload_len];
                Array.Copy(recBytes, 6, payload_data, 0, payload_len);

            }

            for (var i = 0; i < payload_len; i++)
            {
                payload_data[i] = (byte)(payload_data[i] ^ masks[i % 4]);
            }
            return Encoding.UTF8.GetString(payload_data);
        }
        public bool SendPacket(string str)
        {
            #region  发送函数
            //Web发送的命令格式为deviceID-文件编号
            string[] sArray = str.Split('-');
            int device_ID = int.Parse(sArray[0]);
            int fileCode = int.Parse(sArray[1]);
            string cmd = "select pathName from DeviceInfo where DeviceID='" + device_ID + "'";//这一步可以不用，直接用变量加路径
            string path = DbHelperSQL.Execute(cmd).Trim();
            //for (int i = 99; i <= 115; i++)
            //{
            string newPath = "";
            //switch (fileCode)
            //{
            //    case 0x5A:
            //        newPath = Path.Combine(path, "name.xml");
            //        break;
            //    case 0:
                    newPath = Path.Combine(path, "tmp.xml");
            //        break;
            //    case 1:
            //        newPath = Path.Combine(path, "mode1.xml");
            //        break;
            //    case 2:
            //        newPath = Path.Combine(path, "mode2.xml");
            //        break;
            //    case 3:
            //        newPath = Path.Combine(path, "mode3.xml");
            //        break;
            //    case 4:
            //        newPath = Path.Combine(path, "mode4.xml");
            //        break;
            //    case 5:
            //        newPath = Path.Combine(path, "mode5.xml");
            //        break;
            //    case 6:
            //        newPath = Path.Combine(path, "mode6.xml");
            //        break;
            //    case 7:
            //        newPath = Path.Combine(path, "mode7.xml");
            //        break;
            //    case 8:
            //        newPath = Path.Combine(path, "mode8.xml");
            //        break;
            //    case 9:
            //        newPath = Path.Combine(path, "mode9.xml");
            //        break;
            //    case 10:
            //        newPath = Path.Combine(path, "mode10.xml");
            //        break;
            //    case 11:
            //        newPath = Path.Combine(path, "mode11.xml");
            //        break;
            //    case 12:
            //        newPath = Path.Combine(path, "mode12.xml");
            //        break;
            //    case 13:
            //        newPath = Path.Combine(path, "mode13.xml");
            //        break;
            //    case 14:
            //        newPath = Path.Combine(path, "mode14.xml");
            //        break;
            //    case 15:
            //        newPath = Path.Combine(path, "mode15.xml");
            //        break;
            //    case 16:
            //        newPath = Path.Combine(path, "mode16.xml");
            //        break;
            //    case 17:
            //        newPath = Path.Combine(path, "mode17.xml");
            //        break;
            //    case 18:
            //        newPath = Path.Combine(path, "mode18.xml");
            //        break;
            //    case 19:
            //        newPath = Path.Combine(path, "mode19.xml");
            //        break;
            //    case 20:
            //        newPath = Path.Combine(path, "mode20.xml");
            //        break;
            //    case 21:
            //        newPath = Path.Combine(path, "mode21.xml");
            //        break;
            //    case 22:
            //        newPath = Path.Combine(path, "mode22.xml");
            //        break;
            //    case 23:
            //        newPath = Path.Combine(path, "mode23.xml");
            //        break;
            //    case 24:
            //        newPath = Path.Combine(path, "mode24.xml");
            //        break;
            //    default:
            //        break;
            //}
            byte[] buf = { 0X68, 0, 0, 0, 0, 0, 0, 0, 0, (byte)fileCode };
            using (FileStream fsRead = new FileStream(newPath, FileMode.Open, FileAccess.Read))
            {

                byte[] bf = new byte[1024 * 100];
                int r = fsRead.Read(bf, 0, bf.Length);
                byte[] copy = new byte[r];
                List<byte> listSend = new List<byte>();
                listSend.AddRange(buf);
                listSend.Add((byte)(r & 0x00FF));//低字节
                listSend.Add((byte)(r >> 8));//高字节
                Array.Copy(bf, copy, r);
                listSend.AddRange(copy);
                byte[] CRC = AuxiliaryMethod.crc16(listSend.ToArray(), listSend.Count);//调用校验
                listSend.AddRange(CRC);
                listSend.Add(0x16);
                AsyncSocketUserToken UserToken = m_asyncSocketServer.AsyncSocketUserTokenList.UseKey(device_ID);
                lock (UserToken)
                {
                    UserToken.ConnectSocket.Send(listSend.ToArray());
                    //    UserToken.SendEventArgs.ConnectSocket.Send(listSend.ToArray());
                }
                listSend.Clear();
            }
            //}
            string m_message = "send Success";
            m_asyncSocketUserToken.ConnectSocket.Send(PackData(m_message).ToArray());
            Thread.Sleep(20);
            return true;
            #endregion
        }

        /// <summary>
        /// 打包服务器数据
        /// </summary>
        /// <param name="message">数据</param>
        /// <returns>数据包</returns>
        private static byte[] PackData(string message)
        {
            byte[] contentBytes = null;
            byte[] temp = Encoding.UTF8.GetBytes(message);

            if (temp.Length < 126)
            {
                contentBytes = new byte[temp.Length + 2];
                contentBytes[0] = 0x81;
                contentBytes[1] = (byte)temp.Length;
                Array.Copy(temp, 0, contentBytes, 2, temp.Length);
            }
            else if (temp.Length < 0xFFFF)
            {
                contentBytes = new byte[temp.Length + 4];
                contentBytes[0] = 0x81;
                contentBytes[1] = 126;
                contentBytes[2] = (byte)(temp.Length & 0xFF);
                contentBytes[3] = (byte)(temp.Length >> 8 & 0xFF);
                Array.Copy(temp, 0, contentBytes, 4, temp.Length);
            }
            else
            {
                // 暂不处理超长内容
            }

            return contentBytes;
        }

    }

    public class DownloadSocketProtocolMgr : Object
    {
        private List<DownloadSocketProtocol> m_list;

        public DownloadSocketProtocolMgr()
        {
            m_list = new List<DownloadSocketProtocol>();
        }

        public int Count()
        {
            return m_list.Count;
        }

        public DownloadSocketProtocol ElementAt(int index)
        {
            return m_list.ElementAt(index);
        }

        public void Add(DownloadSocketProtocol value)
        {
            m_list.Add(value);
        }

        public void Remove(DownloadSocketProtocol value)
        {
            m_list.Remove(value);
        }
    }
}
