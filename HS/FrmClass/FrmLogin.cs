using System;
using System.Windows.Forms;


namespace HS
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }
        private void FrmLogin_Load(object sender, EventArgs e)
        {
            this.Hide();
            if (DbHelperSQL.ExConnect())
            {
                this.Show();
                this.AcceptButton = btnLogin;
                this.CancelButton = btnExit;
            }
            else
            {
                FrmLink link = new FrmLink();
                link.ShowDialog();
                link.Dispose();
                this.Show();
            }

        }
        private void btnLogin_Click(object sender, EventArgs e)
        {
            
            string Id = txtUid.Text.Trim();
            string password = txtPwd.Text.Trim();
            try
            {
                if (Id != "")
                {
                    btnLogin.Enabled = false;
                    //DbHelperSQL.UseXml();
                    string cmd = "select passWord from UserInfo where userName='" + Id + "'";
                    if (DbHelperSQL.Execute(cmd).Trim() == password)
                    {
                        MessageBox.Show("      登陆成功！");
                        string Sqlin = "insert into dlsjb values ('" + Id + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "')";
                        DbHelperSQL.ExecuteSql(Sqlin);
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("登录失败！");
                        txtUid.Clear();
                        txtPwd.Clear();
                        btnLogin.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("账号不能为空！");
                }
                    
                
            }
            catch 
            {
                
            }
           
        }

        #region
        //    if (rdoAdmin.Checked || rdoOperator.Checked)
        //    {


        //        if (rdoAdmin.Checked)
        //        {
        //            if (Id == "1" && password == "1")
        //            {
        //                MessageBox.Show("用户登录权限为管理员");
        //                this.Hide();
        //                FrmMain frm = new FrmMain();
        //                frm.Show();
        //            }
        //            else
        //            {
        //                MessageBox.Show("登录失败");
        //                txtUid.Clear();
        //                txtPwd.Clear();
        //            }
        //        }
        //        else
        //        {
        //            if (Id == "2" && password == "2")
        //            {
        //                MessageBox.Show("用户登录权限为操作员");
        //                this.Hide();
        //                FrmMain form = new FrmMain();
        //                form.Show();
        //            }
        //            else
        //            {
        //                MessageBox.Show("登录失败");
        //                txtUid.Clear();
        //                txtPwd.Clear();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        MessageBox.Show("请先选择登录身份");
        //    }
        #endregion

        private void btnExit_Click(object sender, EventArgs e)
        {
            
                Environment.Exit(0);
                        
        }
    }
}
