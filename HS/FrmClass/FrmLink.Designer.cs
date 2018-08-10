namespace HS
{
    partial class FrmLink
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblServer = new System.Windows.Forms.Label();
            this.lblDB = new System.Windows.Forms.Label();
            this.lblUser = new System.Windows.Forms.Label();
            this.lblPsWord = new System.Windows.Forms.Label();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.txtDB = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.txtPsword = new System.Windows.Forms.TextBox();
            this.btnConn = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblServer.ForeColor = System.Drawing.SystemColors.Control;
            this.lblServer.Location = new System.Drawing.Point(90, 90);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(129, 20);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "服务器名称：";
            // 
            // lblDB
            // 
            this.lblDB.AutoSize = true;
            this.lblDB.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblDB.ForeColor = System.Drawing.SystemColors.Control;
            this.lblDB.Location = new System.Drawing.Point(90, 150);
            this.lblDB.Name = "lblDB";
            this.lblDB.Size = new System.Drawing.Size(129, 20);
            this.lblDB.TabIndex = 1;
            this.lblDB.Text = "数据库名称：";
            // 
            // lblUser
            // 
            this.lblUser.AutoSize = true;
            this.lblUser.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblUser.ForeColor = System.Drawing.SystemColors.Control;
            this.lblUser.Location = new System.Drawing.Point(130, 210);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(89, 20);
            this.lblUser.TabIndex = 2;
            this.lblUser.Text = "登录名：";
            // 
            // lblPsWord
            // 
            this.lblPsWord.AutoSize = true;
            this.lblPsWord.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblPsWord.ForeColor = System.Drawing.SystemColors.Control;
            this.lblPsWord.Location = new System.Drawing.Point(150, 270);
            this.lblPsWord.Name = "lblPsWord";
            this.lblPsWord.Size = new System.Drawing.Size(69, 20);
            this.lblPsWord.TabIndex = 3;
            this.lblPsWord.Text = "密码：";
            // 
            // txtServer
            // 
            this.txtServer.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtServer.Location = new System.Drawing.Point(280, 85);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(200, 30);
            this.txtServer.TabIndex = 4;
            // 
            // txtDB
            // 
            this.txtDB.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtDB.Location = new System.Drawing.Point(280, 145);
            this.txtDB.Name = "txtDB";
            this.txtDB.Size = new System.Drawing.Size(200, 30);
            this.txtDB.TabIndex = 5;
            // 
            // txtUser
            // 
            this.txtUser.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtUser.Location = new System.Drawing.Point(280, 205);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(200, 30);
            this.txtUser.TabIndex = 6;
            // 
            // txtPsword
            // 
            this.txtPsword.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtPsword.Location = new System.Drawing.Point(280, 265);
            this.txtPsword.Name = "txtPsword";
            this.txtPsword.Size = new System.Drawing.Size(200, 30);
            this.txtPsword.TabIndex = 7;
            // 
            // btnConn
            // 
            this.btnConn.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnConn.Location = new System.Drawing.Point(155, 360);
            this.btnConn.Name = "btnConn";
            this.btnConn.Size = new System.Drawing.Size(90, 30);
            this.btnConn.TabIndex = 8;
            this.btnConn.Text = "连接";
            this.btnConn.UseVisualStyleBackColor = true;
            this.btnConn.Click += new System.EventHandler(this.btnConn_Click);
            // 
            // btnExit
            // 
            this.btnExit.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnExit.Location = new System.Drawing.Point(350, 360);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(90, 30);
            this.btnExit.TabIndex = 10;
            this.btnExit.Text = "退出";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // FrmLink
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ClientSize = new System.Drawing.Size(640, 469);
            this.ControlBox = false;
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnConn);
            this.Controls.Add(this.txtPsword);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.txtDB);
            this.Controls.Add(this.txtServer);
            this.Controls.Add(this.lblPsWord);
            this.Controls.Add(this.lblUser);
            this.Controls.Add(this.lblDB);
            this.Controls.Add(this.lblServer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FrmLink";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "                                                              服务器连接";
            this.Load += new System.EventHandler(this.FrmLink_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Label lblDB;
        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Label lblPsWord;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.TextBox txtDB;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.TextBox txtPsword;
        private System.Windows.Forms.Button btnConn;
        private System.Windows.Forms.Button btnExit;
    }
}