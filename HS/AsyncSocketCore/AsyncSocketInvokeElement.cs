using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace HS
{
    //异步Socket调用对象，所有的协议处理都从本类继承
    public class AsyncSocketInvokeElement
    {
        protected AsyncSocketServer m_asyncSocketServer;
        protected AsyncSocketUserToken m_asyncSocketUserToken;
        public AsyncSocketUserToken AsyncSocketUserToken { get { return m_asyncSocketUserToken; } }

        private bool m_netByteOrder;
        public bool NetByteOrder { get { return m_netByteOrder; } set { m_netByteOrder = value; } } //长度是否使用网络字节顺序

        protected IncomingDataParser m_incomingDataParser; //协议解析器，用来解析客户端接收到的命令
        protected OutgoingDataAssembler m_outgoingDataAssembler; //协议组装器，用来组织服务端返回的命令

        protected bool m_sendAsync; //标识是否有发送异步事件

        protected DateTime m_connectDT;
        public DateTime ConnectDT { get { return m_connectDT; } }
        protected DateTime m_activeDT;
        public DateTime ActiveDT { get { return m_activeDT; } }

        public AsyncSocketInvokeElement(AsyncSocketServer asyncSocketServer, AsyncSocketUserToken asyncSocketUserToken)
        {
            m_asyncSocketServer = asyncSocketServer;
            m_asyncSocketUserToken = asyncSocketUserToken;

            m_netByteOrder = false;

            m_incomingDataParser = new IncomingDataParser();
            m_outgoingDataAssembler = new OutgoingDataAssembler();

            m_sendAsync = false;

            m_connectDT = DateTime.UtcNow;
            m_activeDT = DateTime.UtcNow;
        }

        public virtual void Close()
        {
        }

        public virtual bool ProcessReceive(byte[] buffer, int offset, int count) //接收异步事件返回的数据，用于对数据进行缓存和分包
        {
            return true;
        }
 
        //需要用redis存储网关编号和对应的IP地址，由于没有心跳包注册信息实时数据之分，所以在数据帧中直接解析出来的网管编号和对应的IP地址直接写入
        public virtual bool ProcessPacket(byte[] buffer) //处理分完包后的数据，把命令和数据分开，并对命令进行解析
        {
            int deviceID = AuxiliaryMethod.byte4ToInt(buffer, 1);
            //string gateWay = gate.ToString("d8");
            if (buffer[9] == 0xFF)
            {
                m_asyncSocketUserToken.GateWayId = deviceID;
                m_asyncSocketUserToken.EndPoints = m_asyncSocketUserToken.ConnectSocket.RemoteEndPoint;
                string sql = "UPDATE DeviceInfo SET connectDT='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' WHERE DeviceID = '" + deviceID + "' ";
                DbHelperSQL.ExecuteSql(sql);
                sql = "UPDATE DeviceInfo SET [ip]='" + m_asyncSocketUserToken.EndPoints.ToString() + "' WHERE DeviceID = '" + deviceID + "' ";
                DbHelperSQL.ExecuteSql(sql);
                sql = "select * from DevicePing where DeviceID = "+ deviceID.ToString() ;
                //在DevicePing表中添加连接
                if (DbHelperSQL.Execute(sql)=="")
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
                string sql = "UPDATE DevicePing SET activeDT='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' WHERE DeviceID = '" + deviceID + "' ";
                DbHelperSQL.ExecuteSql(sql);
                sql = "UPDATE DevicePing SET ip='" + m_asyncSocketUserToken.EndPoints.ToString() + "' WHERE DeviceID = '" + deviceID + "' ";
                DbHelperSQL.ExecuteSql(sql);
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
                switch (buffer[9])
                {
                    case 0x5A:
                        newPath = Path.Combine(newPath, "name.xml");
                        break;
                    case 0:
                        newPath = Path.Combine(newPath, "lasttime.xml");
                        break;
                    case 1:
                        newPath = Path.Combine(newPath, "mode1.xml");
                        break;
                    case 2:
                        newPath = Path.Combine(newPath, "mode2.xml");
                        break;
                    case 3:
                        newPath = Path.Combine(newPath, "mode3.xml");
                        break;
                    case 4:
                        newPath = Path.Combine(newPath, "mode4.xml");
                        break;
                    case 5:
                        newPath = Path.Combine(newPath, "mode5.xml");
                        break;
                    case 6:
                        newPath = Path.Combine(newPath, "mode6.xml");
                        break;
                    case 7:
                        newPath = Path.Combine(newPath, "mode7.xml");
                        break;
                    case 8:
                        newPath = Path.Combine(newPath, "mode8.xml");
                        break;
                    case 9:
                        newPath = Path.Combine(newPath, "mode9.xml");
                        break;
                    case 10:
                        newPath = Path.Combine(newPath, "mode10.xml");
                        break;
                    case 11:
                        newPath = Path.Combine(newPath, "mode11.xml");
                        break;
                    case 12:
                        newPath = Path.Combine(newPath, "mode12.xml");
                        break;
                    case 13:
                        newPath = Path.Combine(newPath, "mode13.xml");
                        break;
                    case 14:
                        newPath = Path.Combine(newPath, "mode14.xml");
                        break;
                    case 15:
                        newPath = Path.Combine(newPath, "mode15.xml");
                        break;
                    case 16:
                        newPath = Path.Combine(newPath, "mode16.xml");
                        break;
                    case 17:
                        newPath = Path.Combine(newPath, "mode17.xml");
                        break;
                    case 18:
                        newPath = Path.Combine(newPath, "mode18.xml");
                        break;
                    case 19:
                        newPath = Path.Combine(newPath, "mode19.xml");
                        break;
                    case 20:
                        newPath = Path.Combine(newPath, "mode20.xml");
                        break;
                    case 21:
                        newPath = Path.Combine(newPath, "mode21.xml");
                        break;
                    case 22:
                        newPath = Path.Combine(newPath, "mode22.xml");
                        break;
                    case 23:
                        newPath = Path.Combine(newPath, "mode23.xml");
                        break;
                    case 24:
                        newPath = Path.Combine(newPath, "mode24.xml");
                        break;
                    default:
                        break;
                }
                using (FileStream fsWrite = new FileStream(newPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fsWrite.Write(buffer, 12, buffer.Length - 15);  //需要修改
                }
                return true;
                #endregion
            }
            //if (buffer[9] == 0x5A)
            //{
            //    #region 接收lasttime.xml文件
            //    int gatewayId = AuxiliaryMethod.byte4ToInt(buffer, 1);
            //    //m_asyncSocketServer.Redis.HashSet<string>(gatewayId.ToString(), "lasttime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));  //当前心跳时间
            //    //m_asyncSocketServer.Redis.HashIncrement(gatewayId.ToString(), "heartbeat", 1);
            //    //lock (m_asyncSocketUserToken)
            //    //{
            //    //    m_asyncSocketUserToken.ConnectSocket.Send(buffer);
            //    //}
            //    path = @"C:\Users\LKY\Desktop\lasttime.xml";
            //    using (FileStream fsWrite = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            //    {
            //        fsWrite.Write(buffer, 12, buffer.Length-15);  //需要修改
            //    }
            //    return true;
            //    #endregion


            //}
            //else if (buffer[9] == 0xF0)
            //{
            //    #region //注册信息
            //    return true;
            //    #endregion
            //}
            //else if (buffer[9] == 0x50)
            //{
            //    #region //实时数据
            //    int[] data = new int[10000];
            //    int[,] allData = new int[21, 200];
            //    for (int i = 0; i < 3250; i++)
            //    {
            //        data[i] = BitConverter.ToInt16(buffer, 2 * i + 12);
            //    }

            //    for (int i = 0; i < 50; i++)
            //    {
            //        allData[0, i] = data[i];

            //    }
            //    string sql = "UPDATE zhuban SET [dz1]= " + allData[0, 0] + " ,[dz2]=" + allData[0, 1] + " ,[dz3]=" + allData[0, 2] + " ,[dz4]=" + allData[0, 3] + " ,[dz5]= " + allData[0, 4] + " ,[dz6]= " + allData[0, 5] + " ,[dz7]= " + allData[0, 6] + " ,[dz8]= " + allData[0, 7] + " ,[dz9]= "
            //                           + allData[0, 8] + ",[dz10]=  " + allData[0, 9] + " ,[dz11] = " + allData[0, 10] + ",[dz12] = " + allData[0, 11] + ",[dz13] = " + allData[0, 12] + ",[dz14] = " + allData[0, 13] + ",[dz15] = " + allData[0, 14] + ",[dz16] = " + allData[0, 15] + ",[dz17] = "
            //                           + allData[0, 16] + ",[dz18] = " + allData[0, 17] + " ,[dz19] = " + allData[0, 18] + ",[dz20] = " + allData[0, 19] + ",[dz21] = " + allData[0, 20] + ",[dz22] = " + allData[0, 21] + ",[dz23] = " + allData[0, 22] + ",[dz24] = " + allData[0, 23] + ",[dz25] = "
            //                           + allData[0, 24] + ",[dz26] = " + allData[0, 25] + " ,[dz27] = " + allData[0, 26] + ",[dz28] = " + allData[0, 27] + ",[dz29] = " + allData[0, 28] + ",[dz30] = " + allData[0, 29] + ",[dz31] = " + allData[0, 30] + ",[dz32] = " + allData[0, 31] + ",[dz33] = "
            //                           + allData[0, 32] + ",[dz34] = " + allData[0, 33] + " ,[dz35] = " + allData[0, 34] + ",[dz36] = " + allData[0, 35] + ",[dz37] = " + allData[0, 36] + ",[dz38] = " + allData[0, 37] + ",[dz39] = " + allData[0, 38] + ",[dz40] = " + allData[0, 39] + ",[dz41] = "
            //                           + allData[0, 40] + ",[dz42] = " + allData[0, 41] + ",[dz43] = " + allData[0, 42] + ",[dz44] = " + allData[0, 43] + ",[dz45] = " + allData[0, 44] + ",[dz46] = " + allData[0, 45] + ",[dz47] = " + allData[0, 46] + ",[dz48] = " + allData[0, 47] + ",[dz49] = " + allData[0, 48] + ",[dz50] = " + allData[0, 49] + " WHERE wgbh =" + gateWay + " ";
            //    DbHelperSQL.ExecuteSql(sql);




            //    for (int i = 0; i < 20; i++)
            //    {
            //        for (int j = 0; j < 160; j++)
            //        {
            //            allData[i + 1, j] = data[50 + i * 160 + j];   //allData[1,0]-allData[1,149]对应的是1-150地址位
            //        }
            //        string xg = "xiangji" + (i + 1);
            //        string up_sql = "UPDATE " + xg + " SET [dz1]= " + allData[i + 1, 0] + " ,[dz4]=" + allData[i + 1, 3] + " ,[dz5]=" + allData[i + 1, 4] + " ,[dz8]=" + allData[i + 1, 7] + " ,[dz9]= " + allData[i + 1, 8] + " ,[dz10]= " + allData[i + 1, 9] + " ,[dz11]= " + allData[i + 1, 10] + " ,[dz12]= " + allData[i + 1, 11] + " ,[dz13]= "
            //                         + allData[i + 1, 12] + ",[dz19]=  " + allData[i + 1, 18] + " ,[dz20] = " + allData[i + 1, 19] + ",[dz21] = " + allData[i + 1, 20] + ",[dz22] = " + allData[i + 1, 21] + ",[dz23] = " + allData[i + 1, 22] + ",[dz25] = " + allData[i + 1, 24] + ",[dz26] = " + allData[i + 1, 25] + ",[dz27] = "
            //                         + allData[i + 1, 26] + ",[dz28] = " + allData[i + 1, 27] + " ,[dz29] = " + allData[i + 1, 28] + ",[dz30] = " + allData[i + 1, 29] + ",[dz31] = " + allData[i + 1, 30] + ",[dz32] = " + allData[i + 1, 31] + ",[dz33] = " + allData[i + 1, 32] + ",[dz34] = " + allData[i + 1, 33] + ",[dz35] = "
            //                         + allData[i + 1, 34] + ",[dz36] = " + allData[i + 1, 35] + " ,[dz37] = " + allData[i + 1, 36] + ",[dz38] = " + allData[i + 1, 37] + ",[dz39] = " + allData[i + 1, 38] + ",[dz40] = " + allData[i + 1, 39] + ",[dz41] = " + allData[i + 1, 40] + ",[dz42] = " + allData[i + 1, 41] + ",[dz43] = "
            //                         + allData[i + 1, 42] + ",[dz44] = " + allData[i + 1, 43] + " ,[dz45] = " + allData[i + 1, 44] + ",[dz46] = " + allData[i + 1, 45] + ",[dz47] = " + allData[i + 1, 46] + ",[dz48] = " + allData[i + 1, 47] + ",[dz49] = " + allData[i + 1, 48] + ",[dz50] = " + allData[i + 1, 49] + ",[dz51] = "
            //                         + allData[i + 1, 50] + ",[dz52] = " + allData[i + 1, 51] + " ,[dz53] = " + allData[i + 1, 52] + ",[dz54] = " + allData[i + 1, 53] + ",[dz55] = " + allData[i + 1, 54] + ",[dz56] = " + allData[i + 1, 55] + ",[dz57] = " + allData[i + 1, 56] + ",[dz58] = " + allData[i + 1, 57] + ",[dz59] = "
            //                         + allData[i + 1, 58] + ",[dz60] = " + allData[i + 1, 59] + " ,[dz61] = " + allData[i + 1, 60] + ",[dz62] = " + allData[i + 1, 61] + ",[dz63] = " + allData[i + 1, 62] + ",[dz64] = " + allData[i + 1, 63] + ",[dz65] = " + allData[i + 1, 64] + ",[dz66] = " + allData[i + 1, 65] + ",[dz67] = "
            //                         + allData[i + 1, 66] + ",[dz68] = " + allData[i + 1, 67] + " ,[dz69] = " + allData[i + 1, 68] + ",[dz70] = " + allData[i + 1, 69] + ",[dz71] = " + allData[i + 1, 70] + ",[dz72] = " + allData[i + 1, 71] + ",[dz73] = " + allData[i + 1, 72] + ",[dz74] = " + allData[i + 1, 73] + ",[dz75] = "
            //                         + allData[i + 1, 74] + ",[dz77] = " + allData[i + 1, 76] + " ,[dz78] = " + allData[i + 1, 77] + ",[dz79] = " + allData[i + 1, 78] + ",[dz81] = " + allData[i + 1, 80] + ",[dz82] = " + allData[i + 1, 81] + ",[dz83] = " + allData[i + 1, 82] + ",[dz84] = " + allData[i + 1, 83] + ",[dz85] = "
            //                         + allData[i + 1, 84] + ",[dz86] = " + allData[i + 1, 85] + " ,[dz87] = " + allData[i + 1, 86] + ",[dz88] = " + allData[i + 1, 87] + ",[dz89] = " + allData[i + 1, 88] + ",[dz90] = " + allData[i + 1, 89] + ",[dz91] = " + allData[i + 1, 90] + ",[dz92] = " + allData[i + 1, 91] + " ,[dz93] = "
            //                         + allData[i + 1, 92] + ",[dz94] = " + allData[i + 1, 93] + " ,[dz95] = " + allData[i + 1, 94] + ",[dz96] = " + allData[i + 1, 95] + ",[dz97] = " + allData[i + 1, 96] + ",[dz98] = " + allData[i + 1, 97] + ",[dz99] = " + allData[i + 1, 98] + ",[dz100] = " + allData[i + 1, 99] + ",[dz102] = "
            //                         + allData[i + 1, 101] + ",[dz103] = " + allData[i + 1, 102] + ",[dz104] = " + allData[i + 1, 103] + ",[dz105] = " + allData[i + 1, 104] + ",[dz106] = " + allData[i + 1, 105] + ",[dz113] = " + allData[i + 1, 112] + ",[dz114] = " + allData[i + 1, 113] + ",[dz115] = " + allData[i + 1, 114] + ",[dz121] = "
            //                         + allData[i + 1, 120] + ",[dz350] = " + allData[i + 1, 150] + ",[dz351] = " + allData[i + 1, 151] + ",[dz352] = " + allData[i + 1, 152] + ",[dz353] = " + allData[i + 1, 153] + ",[dz354] = " + allData[i + 1, 154] + ",[dz355] = " + allData[i + 1, 155] + ",[dz356] = " + allData[i + 1, 156] + ",[dz357] = "
            //                         + allData[i + 1, 157] + ",[dz358] = " + allData[i + 1, 158] + " WHERE wgbh =" + gateWay + " ";
            //        DbHelperSQL.ExecuteSql(up_sql);
            //    }

            //    return true;
            //    #endregion
            //}

        }

    
        public virtual bool ProcessCommand(byte[] buffer, int offset, int count) //处理具体命令，子类从这个方法继承，buffer是收到的数据
        {
            return true;
        }

        public virtual bool SendCompleted()
        {
            m_activeDT = DateTime.UtcNow;
            m_sendAsync = false;
            AsyncSendBufferManager asyncSendBufferManager = m_asyncSocketUserToken.SendBuffer;
            asyncSendBufferManager.ClearFirstPacket(); //清除已发送的包
            int offset = 0;
            int count = 0;
            if (asyncSendBufferManager.GetFirstPacket(ref offset, ref count))
            {
                m_sendAsync = true;
                return m_asyncSocketServer.SendAsyncEvent(m_asyncSocketUserToken.ConnectSocket, m_asyncSocketUserToken.SendEventArgs,
                    asyncSendBufferManager.DynamicBufferManager.Buffer, offset, count);
            }
            else
                return SendCallback();
        }

        //发送回调函数，用于连续下发数据
        public virtual bool SendCallback()
        {
            return true;
        }

        public bool DoSendResult()
        {
            string commandText = m_outgoingDataAssembler.GetProtocolText();
            byte[] bufferUTF8 = Encoding.UTF8.GetBytes(commandText);
            int totalLength = sizeof(int) + bufferUTF8.Length; //获取总大小
            AsyncSendBufferManager asyncSendBufferManager = m_asyncSocketUserToken.SendBuffer;
            asyncSendBufferManager.StartPacket();
            asyncSendBufferManager.DynamicBufferManager.WriteInt(totalLength, false); //写入总大小
            asyncSendBufferManager.DynamicBufferManager.WriteInt(bufferUTF8.Length, false); //写入命令大小
            asyncSendBufferManager.DynamicBufferManager.WriteBuffer(bufferUTF8); //写入命令内容
            asyncSendBufferManager.EndPacket();

            bool result = true;
            if (!m_sendAsync)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (asyncSendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    m_sendAsync = true;
                    result = m_asyncSocketServer.SendAsyncEvent(m_asyncSocketUserToken.ConnectSocket, m_asyncSocketUserToken.SendEventArgs,
                        asyncSendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }

        public bool DoSendResult(byte[] buffer, int offset, int count)
        {
            string commandText = m_outgoingDataAssembler.GetProtocolText();
            byte[] bufferUTF8 = Encoding.UTF8.GetBytes(commandText);
            int totalLength = sizeof(int) + bufferUTF8.Length + count; //获取总大小
            AsyncSendBufferManager asyncSendBufferManager = m_asyncSocketUserToken.SendBuffer;
            asyncSendBufferManager.StartPacket();
            asyncSendBufferManager.DynamicBufferManager.WriteInt(totalLength, false); //写入总大小
            asyncSendBufferManager.DynamicBufferManager.WriteInt(bufferUTF8.Length, false); //写入命令大小
            asyncSendBufferManager.DynamicBufferManager.WriteBuffer(bufferUTF8); //写入命令内容
            asyncSendBufferManager.DynamicBufferManager.WriteBuffer(buffer, offset, count); //写入二进制数据
            asyncSendBufferManager.EndPacket();

            bool result = true;
            if (!m_sendAsync)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (asyncSendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    m_sendAsync = true;
                    result = m_asyncSocketServer.SendAsyncEvent(m_asyncSocketUserToken.ConnectSocket, m_asyncSocketUserToken.SendEventArgs,
                        asyncSendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }

        public bool DoSendBuffer(byte[] buffer, int offset, int count) //不是按包格式下发一个内存块，用于日志这类下发协议
        {
            AsyncSendBufferManager asyncSendBufferManager = m_asyncSocketUserToken.SendBuffer;
            asyncSendBufferManager.StartPacket();
            asyncSendBufferManager.DynamicBufferManager.WriteBuffer(buffer, offset, count);
            asyncSendBufferManager.EndPacket();

            bool result = true;
            if (!m_sendAsync)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (asyncSendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    m_sendAsync = true;
                    result = m_asyncSocketServer.SendAsyncEvent(m_asyncSocketUserToken.ConnectSocket, m_asyncSocketUserToken.SendEventArgs,
                        asyncSendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }
    }
}