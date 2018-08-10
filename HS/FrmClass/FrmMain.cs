using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
namespace HS
{
    public partial class FrmMain : Form
    {
        public static FrmMain frm;
        public static AsyncSocketServer AsyncSocketSvr;
        public static string FileDirectory { get; set; }
        public bool Receive = true;
        bool Send = true;
        bool ConnectNum = false;
        public Socket socketWatch;




        public FrmMain()
        {
            InitializeComponent();
            frm = this;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.Hide();
            FrmLogin login = new FrmLogin();
            login.ShowDialog();
            login.Dispose();
            this.Show();
            lblTime.Text = DateTime.Now.ToString();
            Control.CheckForIllegalCrossThreadCalls = false;


        }
        private void maxConnect_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (int.Parse(maxConnect.Text) > 0)
                {
                    btnStart.Enabled = true;
                }
            }
            catch
            {
            }
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            maxConnect.Enabled = false;
            btnClose.BackColor = Color.FromArgb(255, 255, 255);
            GetServer._getServer = AsyncSocketSvr;
            try
            {
                ConnectNum = true;
                int max = int.Parse(maxConnect.Text);
                AsyncSocketSvr = new AsyncSocketServer(max);//最大连接数
                AsyncSocketSvr.SocketTimeOutMS = 60 * 1000;//最大超时时间
                IPAddress ip = IPAddress.Any;
                //创建端口号对象
                IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(txtPort.Text));
                Thread th = new Thread(SocketStart);
                th.IsBackground = true;
                th.Start(point);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SocketStart(object o)
        {
            IPEndPoint point = o as IPEndPoint;
            AsyncSocketSvr.Init();
            ///监听
            AsyncSocketSvr.Start(point);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            ConnectNum = false;
            AsyncSocketUserToken[] userTokenArray = null;
            AsyncSocketSvr.AsyncSocketUserTokenList.CopyList(ref userTokenArray);
            for (int i = 0; i < userTokenArray.Length; i++)
            {
                try
                {
                    lock (userTokenArray[i])
                    {
                        AsyncSocketSvr.CloseClientSocket(userTokenArray[i]);
                    }
                }
                catch (Exception E)
                {
                    MessageBox.Show(E.ToString());
                }
            }
            AsyncSocketSvr.CloseAllClient();
            txtLink.AppendText(DateTime.Now.ToString() + " :停止监听" + "\r\n");
            btnStart.BackColor = Color.FromArgb(255, 255, 255);
            maxConnect.Enabled = true;
            btnStart.Enabled = true;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            //string str = txtInput.Text;
            // byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
            List<byte> listSend = new List<byte>();
            byte[] buffer = { 0X68, 0, 0, 0, 1, 0, 0, 0, 1, 90, 3, 0, 1, 1, 1 };
            //list.Add(0);
            listSend.AddRange(buffer);
            byte[] CRC = AuxiliaryMethod.crc16(listSend.ToArray(), listSend.Count);//调用校验
            listSend.AddRange(CRC);
            listSend.Add(0x16);
            //将泛型集合转换为数组
            byte[] newBuffer = listSend.ToArray();
            //获得用户在下拉框中选中的IP地址
            string ip = cboUser.SelectedItem.ToString();
            lock (AsyncSocketSvr.AsyncSocketUserTokenList.UserEndPoint(ip))
            {
                AsyncSocketSvr.AsyncSocketUserTokenList.UserEndPoint(ip).Send(newBuffer);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtReceive.Clear();
        }

        private void btnStopReceive_Click(object sender, EventArgs e)
        {
            if (txtReceive.Enabled == true)
            {
                btnStopReceive.Text = "开启显示";
                txtReceive.Enabled = false;
            }
            else
            {
                btnStopReceive.Text = "停止显示";
                txtReceive.Enabled = true;
            }
        }

        private void btnSendClear_Click(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {

        }

        private void btnSave1_Click(object sender, EventArgs e)
        {

        }

        private void btnStart_MouseDown(object sender, MouseEventArgs e)
        {
            btnStart.BackColor = Color.FromArgb(64, 64, 64);
        }

        private void btnClose_MouseDown(object sender, MouseEventArgs e)
        {
            btnClose.BackColor = Color.FromArgb(64, 64, 64);
        }

        private void FrmMain_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Red, 0, 0, this.Width - 1, this.Height - 1);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblTime.Text = DateTime.Now.ToString();
            if (ConnectNum)
            {
                lblCount.Text = AsyncSocketSvr.AsyncSocketUserTokenList.Count().ToString();
            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            txtReceive.Text = "";
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                Environment.Exit(0);
            }
            catch
            {

            }
        }

        private void EXEClose_ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Environment.Exit(0);
            }
            catch
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] buf = { 0X68, 0, 0, 0, 1, 0, 0, 0, 1, 0 };

            //获得要发送文件的路径
            string path = @"D:\HS\18032801\lasttime.xml";
            using (FileStream fsRead = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
 
                byte[] buffer = new byte[1024 * 1024 * 5];
                int r = fsRead.Read(buffer, 0, buffer.Length);
                byte[] copy = new byte[r];
                List<byte> listSend = new List<byte>();
                listSend.AddRange(buf);
                listSend.Add ((byte)(r & 0x00FF));//低字节
                listSend.Add((byte)(r >> 8));//高字节
                Array.Copy(buffer, copy, r);
                listSend.AddRange(copy);
                byte[] CRC = AuxiliaryMethod.crc16(listSend.ToArray(), listSend.Count);//调用校验
                listSend.AddRange(CRC);
                listSend.Add(0x16);
                //将泛型集合转换为数组
                byte[] newBuffer = listSend.ToArray();
                //获得用户在下拉框中选中的IP地址
                string ip = cboUser.SelectedItem.ToString();
                lock (AsyncSocketSvr.AsyncSocketUserTokenList.UserEndPoint(ip))
                {
                    AsyncSocketSvr.AsyncSocketUserTokenList.UserEndPoint(ip).Send(newBuffer, 0, newBuffer.Length, SocketFlags.None);
                }
            }

        }


    }
}
