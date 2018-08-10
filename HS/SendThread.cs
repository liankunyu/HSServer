using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
                //存储发送数据的表
                List<byte> listSend = new List<byte>();
                //进行循环遍历表中的指令下发
                while (true)
                {
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
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }






    }
}
