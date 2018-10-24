using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HS
{
    class SendThread
    {
        private Thread m_thread;
        private AsyncSocketServer m_asyncSocketServer;

        public SendThread(AsyncSocketServer asyncSocketServer)
        {
            m_asyncSocketServer = asyncSocketServer;
            m_thread = new Thread(SendThreadStart);
            m_thread.Start();
        }

        public void SendThreadStart()
        {
            try
            {
                ////存储发送数据的表
                //List<byte> listSend = new List<byte>();
                ////进行循环遍历表中的指令下发
                DataTable dt = new DataTable();
                string hs_sql, cmd, path, newPath;
                int device_ID;
                byte[] buf = { 0X68, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                while (true)
                {
                    #region
                    //if (m_asyncSocketServer.Redis.ListLength("commands") > 0)
                    //{
                    //    #region
                    //    try
                    //    {
                    //        string commandStr = m_asyncSocketServer.Redis.ListRightPop<string>("commands");
                    //        SendOption sendOption = JsonConvert.DeserializeObject<SendOption>(commandStr);
                    //        int gateWay = Convert.ToInt32(sendOption.Gateway);
                    //        if (m_asyncSocketServer.AsyncSocketUserTokenList.ContainKey(gateWay))
                    //        {
                    //            byte[] gateway_number = AuxiliaryMethod.intTo4Bytes(gateWay);
                    //            byte[] devices_number = AuxiliaryMethod.intTo4Bytes(Convert.ToInt32(sendOption.Devices));
                    //            byte[] commandType = AuxiliaryMethod.StringToBytes(sendOption.Order);
                    //            //string str = sendOption.Command;
                    //            byte[] commandvalues = AuxiliaryMethod.StringToBytes(sendOption.Command);
                    //            if (commandvalues.Length < 9)
                    //            {
                    //                byte[] dataLength = { 0, 8 };
                    //                byte startFrame = 0x68;                       //拼凑帧头
                    //                byte endFrame = 0x16;                       //帧尾
                    //                listSend.Add(startFrame);            //全部添加listSend
                    //                listSend.AddRange(gateway_number);
                    //                listSend.AddRange(devices_number);
                    //                listSend.AddRange(commandType);
                    //                listSend.AddRange(dataLength);
                    //                listSend.AddRange(commandvalues);
                    //                byte[] CRC = AuxiliaryMethod.crc16(listSend.ToArray(), listSend.Count);//调用校验,
                    //                listSend.AddRange(CRC);                                //将校验数据添加listSend
                    //                listSend.Add(endFrame);
                    //            }
                    //            else
                    //            {
                    //                byte[] dataLength = { 0, 0x0A };
                    //                byte startFrame = 0x68;                       //拼凑帧头
                    //                byte endFrame = 0x16;                       //帧尾
                    //                listSend.Add(startFrame);            //全部添加listSend
                    //                listSend.AddRange(gateway_number);
                    //                listSend.AddRange(devices_number);
                    //                listSend.AddRange(commandType);
                    //                listSend.AddRange(dataLength);
                    //                listSend.AddRange(commandvalues);
                    //                byte[] CRC = AuxiliaryMethod.crc16(listSend.ToArray(), listSend.Count);//调用校验,
                    //                listSend.AddRange(CRC);                                //将校验数据添加listSend
                    //                listSend.Add(endFrame);
                    //            }
                    //    #endregion
                    //            AsyncSocketUserToken UserToken = m_asyncSocketServer.AsyncSocketUserTokenList.UseKey(gateWay);
                    //            lock (UserToken)
                    //            {
                    //                UserToken.ConnectSocket.Send(listSend.ToArray());
                    //            }
                    //            listSend.Clear();
                    //            Thread.Sleep(50);
                    //        }

                    //    }
                    //    catch
                    //    {

                    //    }
                    //}
                    //else
                    //{
                    //    Thread.Sleep(50);
                    //}
                    #endregion

                    hs_sql = "select * from SendTable";
                    dt = DbHelperSQL.OpenTable(hs_sql);
                    if (dt.Rows.Count > 0)
                    {
                        #region
                        hs_sql = "Truncate table SendTable";//清除表中所有行，保留表结构、与delete类似比delete速度快,而且效率高，使用的系统和事务日志资源少
                        DbHelperSQL.ExecuteSql(hs_sql);
                        for (int i=0;i<dt.Rows.Count;i++)
                        {
                            device_ID = Convert.ToInt32(dt.Rows[i][2]);
                            cmd = "select pathName from DeviceInfo where DeviceID='" + device_ID + "'";//这一步可以不用，直接用变量加路径
                            path = DbHelperSQL.Execute(cmd).Trim();
                            newPath = Path.Combine(path, "tmp.xml");
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
                                }
                                listSend.Clear();
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }






    }
}
