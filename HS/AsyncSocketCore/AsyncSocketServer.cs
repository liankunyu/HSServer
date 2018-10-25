using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace HS
{
    public class AsyncSocketServer
    {
        // private RedisHelper _redis=new RedisHelper();

        //public RedisHelper Redis
        //{
        //    get { return _redis; }
        //    set { _redis = value; }
        //}
        private Socket listenSocket;

        private int m_numConnections; //最大支持连接个数
        private int m_receiveBufferSize; //每个连接接收缓存大小
        private Semaphore m_maxNumberAcceptedClients; //限制访问接收连接的线程数，用来控制最大并发数

        private int m_socketTimeOutMS; //Socket最大超时时间，单位为MS
        public int SocketTimeOutMS { get { return m_socketTimeOutMS; } set { m_socketTimeOutMS = value; } }

        private AsyncSocketUserTokenPool m_asyncSocketUserTokenPool;
        private AsyncSocketUserTokenList m_asyncSocketUserTokenList;
        public AsyncSocketUserTokenList AsyncSocketUserTokenList { get { return m_asyncSocketUserTokenList; } }

        private LogOutputSocketProtocolMgr m_logOutputSocketProtocolMgr;
        public LogOutputSocketProtocolMgr LogOutputSocketProtocolMgr { get { return m_logOutputSocketProtocolMgr; } }

        private UploadSocketProtocolMgr m_uploadSocketProtocolMgr;
        public UploadSocketProtocolMgr UploadSocketProtocolMgr { get { return m_uploadSocketProtocolMgr; } }

        private DownloadSocketProtocolMgr m_downloadSocketProtocolMgr;
        public DownloadSocketProtocolMgr DownloadSocketProtocolMgr { get { return m_downloadSocketProtocolMgr; } }

        private DaemonThread m_daemonThread;//监听线程
        private SendThread m_sendThread;//发送线程

        public AsyncSocketServer(int numConnections)
        {
            m_numConnections = numConnections;
            m_receiveBufferSize = ProtocolConst.ReceiveBufferSize;

            m_asyncSocketUserTokenPool = new AsyncSocketUserTokenPool(numConnections);
            m_asyncSocketUserTokenList = new AsyncSocketUserTokenList();
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);

            m_logOutputSocketProtocolMgr = new LogOutputSocketProtocolMgr();
            m_uploadSocketProtocolMgr = new UploadSocketProtocolMgr();
            m_downloadSocketProtocolMgr = new DownloadSocketProtocolMgr();
        }

        public void Init()
        {
            AsyncSocketUserToken userToken;
            for (int i = 0; i < m_numConnections; i++) //按照连接数建立读写对象
            {
                userToken = new AsyncSocketUserToken(m_receiveBufferSize);
                userToken.ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                userToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                m_asyncSocketUserTokenPool.Push(userToken);
            }
        }

        public void Start(IPEndPoint localEndPoint)
        {
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            listenSocket.Listen(m_numConnections);
            FrmMain.frm.txtLink.AppendText("开始监听" + "\r\n");
            //for (int i = 0; i < 64; i++) //不能循环投递多次AcceptAsync，会造成只接收8000连接后不接收连接了
            StartAccept(null);
            m_daemonThread = new DaemonThread(this);    //守护线程，超时断开连接
            m_sendThread = new SendThread(this);        //发送线程，接收Web发送过来的消息
        }

        public void StartAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs == null)
            {
                acceptEventArgs = new SocketAsyncEventArgs();
                acceptEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                acceptEventArgs.AcceptSocket = null; //释放上次绑定的Socket，等待下一个Socket连接
            }

            m_maxNumberAcceptedClients.WaitOne(); //获取信号量semphore
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArgs);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArgs);
            }
        }

        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch
            {
            }
        }

        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            //DbHelperSQL.UseXml();
            //string cmd = "insert into 连接表 values ('"+acceptEventArgs.AcceptSocket.RemoteEndPoint.ToString()+"','"+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+"')";
            //DbHelperSQL.Execute(cmd);
            FrmMain.frm.txtLink.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ":" + acceptEventArgs.AcceptSocket.RemoteEndPoint.ToString() + "\r\n");
            FrmMain.frm.cboUser.Items.Add(acceptEventArgs.AcceptSocket.RemoteEndPoint.ToString());
            AsyncSocketUserToken userToken = m_asyncSocketUserTokenPool.Pop();
            m_asyncSocketUserTokenList.Add(userToken); //添加到正在连接列表
            FrmMain.frm.lblCount.Text = m_asyncSocketUserTokenList.Count().ToString();
            userToken.ConnectSocket = acceptEventArgs.AcceptSocket;
            userToken.ConnectDateTime = DateTime.Now;

            try
            {
                bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                if (!willRaiseEvent)
                {
                    lock (userToken)
                    {
                        ProcessReceive(userToken.ReceiveEventArgs);
                    }
                }
            }
            catch
            {
            }

            StartAccept(acceptEventArgs); //把当前异步事件释放，等待下次连接
        }

        void IO_Completed(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            AsyncSocketUserToken userToken = asyncEventArgs.UserToken as AsyncSocketUserToken;
            userToken.ActiveDateTime = DateTime.Now;
            try
            {
                lock (userToken)
                {
                    if (asyncEventArgs.LastOperation == SocketAsyncOperation.Receive)
                        ProcessReceive(asyncEventArgs);
                    else if (asyncEventArgs.LastOperation == SocketAsyncOperation.Send)
                        ProcessSend(asyncEventArgs);
                    else
                        throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                }
            }
            catch (Exception E)
            {
                MessageBox.Show(E.ToString());
            }
        }

        private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            AsyncSocketUserToken userToken = receiveEventArgs.UserToken as AsyncSocketUserToken;
            if (userToken.ConnectSocket == null)
                return;
            userToken.ActiveDateTime = DateTime.Now;
            if (userToken.ReceiveEventArgs.BytesTransferred > 0 && userToken.ReceiveEventArgs.SocketError == SocketError.Success)
            {
                int offset = userToken.ReceiveEventArgs.Offset;
                int count = userToken.ReceiveEventArgs.BytesTransferred;
                userToken.AsyncSocketInvokeElement = new UploadSocketProtocol(this, userToken);
                if ((userToken.AsyncSocketInvokeElement == null) & (userToken.ConnectSocket != null)) //存在Socket对象，并且没有绑定协议对象，则进行协议对象绑定
                {
                    offset = offset + 1;
                    count = count - 1;
                }
                if (userToken.AsyncSocketInvokeElement == null) //如果没有解析对象，提示非法连接并关闭连接
                {
                    CloseClientSocket(userToken);
                }
                else
                {
                    if (count > 0) //处理接收数据
                    {
                        if (!userToken.AsyncSocketInvokeElement.ProcessReceive(userToken.ReceiveEventArgs.Buffer, offset, count))
                        { //如果处理数据返回失败，则断开连接
                            CloseClientSocket(userToken);
                        }
                        else //否则投递下次介绍数据请求
                        {
                            bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                            if (!willRaiseEvent)
                                ProcessReceive(userToken.ReceiveEventArgs);
                        }
                    }
                    else
                    {
                        bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                        if (!willRaiseEvent)
                            ProcessReceive(userToken.ReceiveEventArgs);
                    }
                }
            }
            else
            {
                CloseClientSocket(userToken);
            }
        }
        #region 打包请求连接数据
        /// <summary>
        /// 打包请求连接数据
        /// </summary>
        /// <param name="handShakeBytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private byte[] PackageHandShakeData(byte[] handShakeBytes, int length)
        {
            string handShakeText = Encoding.UTF8.GetString(handShakeBytes, 0, length);
            string key = string.Empty;
            Regex reg = new Regex(@"Sec\-WebSocket\-Key:(.*?)\r\n");
            Match m = reg.Match(handShakeText);
            if (m.Value != "")
            {
                key = Regex.Replace(m.Value, @"Sec\-WebSocket\-Key:(.*?)\r\n", "$1").Trim();
            }
            byte[] secKeyBytes = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
            string secKey = Convert.ToBase64String(secKeyBytes);
            var responseBuilder = new StringBuilder();
            responseBuilder.Append("HTTP/1.1 101 Switching Protocols" + "\r\n");
            responseBuilder.Append("Upgrade: websocket" + "\r\n");
            responseBuilder.Append("Connection: Upgrade" + "\r\n");
            responseBuilder.Append("Sec-WebSocket-Accept: " + secKey + "\r\n\r\n");
            return Encoding.UTF8.GetBytes(responseBuilder.ToString());
        }
        #endregion



        private bool ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            AsyncSocketUserToken userToken = sendEventArgs.UserToken as AsyncSocketUserToken;
            if (userToken.AsyncSocketInvokeElement == null)
                return false;
            userToken.ActiveDateTime = DateTime.Now;
            if (sendEventArgs.SocketError == SocketError.Success)
                return userToken.AsyncSocketInvokeElement.SendCompleted(); //调用子类回调函数
            else
            {
                CloseClientSocket(userToken);
                return false;
            }
        }

        public bool SendAsyncEvent(Socket connectSocket, SocketAsyncEventArgs sendEventArgs, byte[] buffer, int offset, int count)
        {
            if (connectSocket == null)
                return false;
            sendEventArgs.SetBuffer(buffer, offset, count);
            bool willRaiseEvent = connectSocket.SendAsync(sendEventArgs);
            if (!willRaiseEvent)
            {
                return ProcessSend(sendEventArgs);
            }
            else
                return true;
        }

        public void CloseClientSocket(AsyncSocketUserToken userToken)
        {
            if (userToken.ConnectSocket == null)
                return;
            string socketInfo = string.Format("Local Address: {0} Remote Address: {1}", userToken.ConnectSocket.LocalEndPoint,
                userToken.ConnectSocket.RemoteEndPoint);

            try
            {
                string sql = "DELETE FROM  DevicePing WHERE ip='" + userToken.ConnectSocket.RemoteEndPoint.ToString() + "'";
                DbHelperSQL.ExecuteSql(sql);
                userToken.ConnectSocket.Shutdown(SocketShutdown.Both);
                userToken.ConnectSocket.Close();
                userToken.ConnectSocket = null; //释放引用，并清理缓存，包括释放协议对象等资源

                m_maxNumberAcceptedClients.Release();
                m_asyncSocketUserTokenPool.Push(userToken);
                m_asyncSocketUserTokenList.Remove(userToken);
                //FrmMain.frm.txtLink.AppendText(".....关闭成功！\r\n");
                FrmMain.frm.lblCount.Text = m_asyncSocketUserTokenList.Count().ToString();
            }
            catch
            {

            }
            //DbHelperSQL.UseXml();
            //string cmd = "insert into 退出表 values ('" + userToken.ConnectSocket.RemoteEndPoint.ToString() + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "')";
            //DbHelperSQL.Execute(cmd);

            //FrmMain.frm.txtLink.AppendText(DateTime.Now.ToString()+userToken.ConnectSocket.RemoteEndPoint.ToString()+ ":正在关闭");

        }

        public void CloseAllClient()
        {
            FrmMain.frm.cboUser.Items.Clear();
            listenSocket.Close();
            listenSocket.Dispose();
        }
    }
}
