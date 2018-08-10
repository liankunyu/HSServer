using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HS
{
    public partial class FrmLink : Form
    {
        
        public FrmLink()
        {
            InitializeComponent();
        }

        private void btnConn_Click(object sender, EventArgs e)
        {
            
            try
            {
                if ((txtServer.Text.Trim() != "") || (txtUser.Text.Trim() != "") || (txtDB.Text.Trim() != "") || (txtPsword.Text.Trim() != ""))
                {
                    btnConn.Enabled = false;
                    XMLHealper opxml = new XMLHealper("Data.xml");
                    opxml.SetXmlNodeValue("system/datasource", txtServer.Text.Trim());
                    opxml.SetXmlNodeValue("system/database",txtDB.Text.Trim());
                    opxml.SetXmlNodeValue("system/userid", txtUser.Text.Trim());
                    opxml.SetXmlNodeValue("system/password", txtPsword.Text.Trim());
                    opxml.SavexmlDocument();
                    if (DbHelperSQL.ExConnect())
                    {
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("数据库连接出错！请核查");
                        btnConn.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("文本框不能为空！");
                }
            }
            catch
            {

            }
            
        }

       

        private void FrmLink_Load(object sender, EventArgs e)
        {
            try
            {
                MessageBox.Show("数据库连接出错！请核查");
                XMLHealper opxml = new XMLHealper("Data.xml");
                txtServer.Text = opxml.PathGetNodeValue("system/datasource");
                txtDB.Text = opxml.PathGetNodeValue("system/database");
                txtUser.Text = opxml.PathGetNodeValue("system/userid");
                txtPsword.Text = opxml.PathGetNodeValue("system/password");
                this.AcceptButton = btnConn;
                this.CancelButton = btnExit;
            }
            catch
            { 
            	
            }
           
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
                   Environment.Exit(0);
        }

       
    }
}
