using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Windows.Forms;

namespace HS
{
    public class UploadSocketProtocol : BaseSocketProtocol
    {
        private string m_fileName;
        public string FileName { get { return m_fileName; } }
        private FileStream m_fileStream;

        public UploadSocketProtocol(AsyncSocketServer asyncSocketServer, AsyncSocketUserToken asyncSocketUserToken)
            : base(asyncSocketServer, asyncSocketUserToken)
        {
            m_socketFlag = "Upload";
            m_fileName = "";
            m_fileStream = null;
            lock (m_asyncSocketServer.UploadSocketProtocolMgr)
            {
                m_asyncSocketServer.UploadSocketProtocolMgr.Add(this);
            }
        }

        public override void Close()
        {
            base.Close();
            m_fileName = "";
            if (m_fileStream != null)
            {
                m_fileStream.Close();
                m_fileStream = null;
            }
            lock (m_asyncSocketServer.UploadSocketProtocolMgr)
            {
                m_asyncSocketServer.UploadSocketProtocolMgr.Remove(this);
            }
        }
        public override bool ProcessReceive(byte[] buffer, int offset, int count) //接收异步事件返回的数据，用于对数据进行缓存和分包
        {
            m_activeDT = DateTime.UtcNow;
            List<byte> DataList = new List<byte>();
            DataList.AddRange(buffer.Take(count));
            bool result = true;
            try
            {
                while (DataList.Count > 6)//(DataCount > sizeof(int))
                {
                    if (DataList[0] == 0x68)
                    {
                        //按照长度分包
                        int packetLength = (DataList[10] + DataList[11] * 256);    // AuxiliaryMethod.byte2ToInt(DataList.ToArray(), 10); //获取包长度
                        if (DataList.Count > packetLength + 14)
                        {
                            if (DataList[packetLength + 14] == 0x16)
                            {
                                byte[] CRC = AuxiliaryMethod.crc16(DataList.ToArray(), packetLength + 12);//CRC校验
                                if ((CRC[0] == DataList[packetLength + 12]) && (CRC[1] == DataList[packetLength + 13])) //如果CRC16校验正确，数据接收成功
                                {
                                    result = ProcessPacket(DataList.Take(packetLength + 15).ToArray());
                                    DataList.RemoveRange(0, packetLength + 15); //从缓存中清理
                                }
                                else
                                {
                                    DataList.RemoveRange(0, packetLength + 15); //数据出错从缓存中清理
                                }
                            }
                            else
                            {
                                DataList.RemoveAt(0);
                            }
                        }
                        else
                        {
                            return true;
                        }
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
                MessageBox.Show(e.ToString());
            }

            finally
            {
                GC.Collect();
            }
            return true;
        }

        public override bool ProcessPacket(byte[] buffer) //处理分完包后的数据，把命令和数据分开，并对命令进行解析
        {
            int deviceID = AuxiliaryMethod.byte4ToInt(buffer, 1);
            //string gateWay = gate.ToString("d8");
            if (buffer[9] == 0xFF)
            {
                m_asyncSocketUserToken.GateWayId = deviceID;
                m_asyncSocketUserToken.EndPoints = m_asyncSocketUserToken.ConnectSocket.RemoteEndPoint;
                string sql = "UPDATE DeviceInfo SET connectDT='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',ip='" + m_asyncSocketUserToken.EndPoints.ToString() + "' WHERE DeviceID = '" + deviceID + "' ";
                DbHelperSQL.ExecuteSql(sql);
                //sql = "UPDATE DeviceInfo SET ip='" + m_asyncSocketUserToken.EndPoints.ToString() + "' WHERE DeviceID = '" + deviceID + "' ";
                //DbHelperSQL.ExecuteSql(sql);
                sql = "select * from DevicePing where DeviceID = " + deviceID.ToString();
                //在DevicePing表中添加连接
                if (DbHelperSQL.Execute(sql) == "")
                {
                    sql = "INSERT INTO DevicePing VALUES ('" + deviceID.ToString() + "','" + m_asyncSocketUserToken.EndPoints.ToString() + "','" + DateTime.Now.ToString("yyyy -MM-dd HH:mm:ss.fff") + "') ";
                    // sql = "insert into DevicePing(DeviceID, ip, activeDT) select " + deviceID.ToString() + "," + m_asyncSocketUserToken.EndPoints.ToString() + ",(" + DateTime.Now.ToString("yyyy -MM-dd HH:mm:ss.fff") + ") from DevicePing  where not exists(select * from DevicePing where DeviceID = "+ deviceID.ToString() + ")";
                    DbHelperSQL.ExecuteSql(sql);
                }
                return true;
            }
            if (buffer[9] == 0xF0)
            {
                m_asyncSocketUserToken.EndPoints = m_asyncSocketUserToken.ConnectSocket.RemoteEndPoint;
                string sql = "UPDATE DevicePing SET activeDT='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "',ip='" + m_asyncSocketUserToken.EndPoints.ToString() + "' WHERE DeviceID = '" + deviceID + "' ";
                DbHelperSQL.ExecuteSql(sql);
                //sql = "UPDATE DevicePing SET ip='" + m_asyncSocketUserToken.EndPoints.ToString() + "' WHERE DeviceID = '" + deviceID + "' ";
                //DbHelperSQL.ExecuteSql(sql);
                return true;
            }
            else
            {
                // 创建目录时如果目录已存在，则不会重新创建目录，且不会报错。创建目录时会自动创建路径中各级不存在的目录。
                //通过Path类的Combine方法可以合并路径
                string activeDir = @"D:\HS";
                string newPath = Path.Combine(activeDir, deviceID.ToString());
                Directory.CreateDirectory(newPath);

                #region    上传数据
                string sql = "UPDATE DeviceInfo SET activeDT='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' WHERE DeviceID = '" + deviceID + "' ";
                DbHelperSQL.ExecuteSql(sql);
                //string cmd = "select pathName from DeviceInfo where DeviceID='" + deviceID + "'";
                //string path = DbHelperSQL.Execute(cmd).Trim();
                //switch (buffer[9])
                //{
                //    case 0x5A:
                //        newPath = Path.Combine(newPath, "name.xml");
                //        break;
                //    case 0:
                newPath = Path.Combine(newPath, "lasttime.xml");
                //        break;
                //    case 1:
                //        newPath = Path.Combine(newPath, "mode1.xml");
                //        break;
                //    case 2:
                //        newPath = Path.Combine(newPath, "mode2.xml");
                //        break;
                //    case 3:
                //        newPath = Path.Combine(newPath, "mode3.xml");
                //        break;
                //    case 4:
                //        newPath = Path.Combine(newPath, "mode4.xml");
                //        break;
                //    case 5:
                //        newPath = Path.Combine(newPath, "mode5.xml");
                //        break;
                //    case 6:
                //        newPath = Path.Combine(newPath, "mode6.xml");
                //        break;
                //    case 7:
                //        newPath = Path.Combine(newPath, "mode7.xml");
                //        break;
                //    case 8:
                //        newPath = Path.Combine(newPath, "mode8.xml");
                //        break;
                //    case 9:
                //        newPath = Path.Combine(newPath, "mode9.xml");
                //        break;
                //    case 10:
                //        newPath = Path.Combine(newPath, "mode10.xml");
                //        break;
                //    case 11:
                //        newPath = Path.Combine(newPath, "mode11.xml");
                //        break;
                //    case 12:
                //        newPath = Path.Combine(newPath, "mode12.xml");
                //        break;
                //    case 13:
                //        newPath = Path.Combine(newPath, "mode13.xml");
                //        break;
                //    case 14:
                //        newPath = Path.Combine(newPath, "mode14.xml");
                //        break;
                //    case 15:
                //        newPath = Path.Combine(newPath, "mode15.xml");
                //        break;
                //    case 16:
                //        newPath = Path.Combine(newPath, "mode16.xml");
                //        break;
                //    case 17:
                //        newPath = Path.Combine(newPath, "mode17.xml");
                //        break;
                //    case 18:
                //        newPath = Path.Combine(newPath, "mode18.xml");
                //        break;
                //    case 19:
                //        newPath = Path.Combine(newPath, "mode19.xml");
                //        break;
                //    case 20:
                //        newPath = Path.Combine(newPath, "mode20.xml");
                //        break;
                //    case 21:
                //        newPath = Path.Combine(newPath, "mode21.xml");
                //        break;
                //    case 22:
                //        newPath = Path.Combine(newPath, "mode22.xml");
                //        break;
                //    case 23:
                //        newPath = Path.Combine(newPath, "mode23.xml");
                //        break;
                //    case 24:
                //        newPath = Path.Combine(newPath, "mode24.xml");
                //        break;
                //    default:
                //        break;
                //}
                using (FileStream fsWrite = new FileStream(newPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fsWrite.Write(buffer, 12, buffer.Length - 15);  //需要修改
                }
                return true;
                #endregion
            }

        }
    }

    public class UploadSocketProtocolMgr : Object
    {
        private List<UploadSocketProtocol> m_list;

        public UploadSocketProtocolMgr()
        {
            m_list = new List<UploadSocketProtocol>();
        }

        public int Count()
        {
            return m_list.Count;
        }

        public UploadSocketProtocol ElementAt(int index)
        {
            return m_list.ElementAt(index);
        }

        public void Add(UploadSocketProtocol value)
        {
            m_list.Add(value);
        }

        public void Remove(UploadSocketProtocol value)
        {
            m_list.Remove(value);
        }
    }
}
