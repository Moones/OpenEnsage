using Loader;
using Loader.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Loader.Forms
{
    public class Login : Form
    {
        private string _acc = string.Empty;
        private byte[] _pwMd5;
        //private IContainer components;
        private Label label1;
        private Label label2;
        private MaskedTextBox passwordBox;
        private TextBox accountBox;
        private Button loginButton;
        private CheckBox checkBox1;
        private LinkLabel forumLink;

        public Login()
        {
            this.InitializeComponent();
            this.forumLink.Links.Add(0, -1, (object)"http://zynox.net/forum");
            this.forumLink.TabStop = false;
        }

        private string GetMD5String(IEnumerable<byte> arr)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte num in arr)
                stringBuilder.Append(num.ToString("x2").ToLower());
            return stringBuilder.ToString();
        }

        private void SendLogin()
        {
            Program.UserGroup = 10; // AFFSD BEGIN
            Program.SessionKey = new byte[128];
            Program.StartMainForm = true;
            Program.AccountName = _acc;
            base.Close();
            return; // AFFSD END

            /*
            string md5String = this.GetMD5String((IEnumerable<byte>)this._pwMd5);
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(string.Format("acc={0}&pw={1}", (object)this._acc, (object)md5String));
                WebRequest webRequest = WebRequest.Create("http://www.zynox.net/ensage/login4.php");
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = (long)bytes.Length;
                using (Stream requestStream = webRequest.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Flush();
                }
                using (WebResponse response = webRequest.GetResponse())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        if (responseStream == null)
                            throw new Exception("Can't get response stream");
                        using (BinaryReader binaryReader = new BinaryReader(responseStream))
                        {
                            if ((int)binaryReader.ReadByte() == 48)
                            {
                                Program.UserGroup = int.Parse(new string(binaryReader.ReadChars(2)));
                                Program.SessionKey = binaryReader.ReadBytes(128);
                                Program.StartMainForm = true;
                                Program.AccountName = this._acc;
                                this.Close();
                                return;
                            }
                        }
                    }
                }
                if (MessageBox.Show(Resources.Error_AccPwIncorrect, Resources.Error, MessageBoxButtons.RetryCancel, MessageBoxIcon.Hand) != DialogResult.Cancel)
                    return;
                this.Close();
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
            }
            */
        }

        private void Login_Shown(object sender, EventArgs e)
        {
            try
            {
                using (FileStream fileStream = File.OpenRead("info.dat"))
                {
                    BinaryReader binaryReader = new BinaryReader((Stream)fileStream);
                    while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
                    {
                        switch (binaryReader.ReadByte())
                        {
                            case (byte)0:
                                this._acc = binaryReader.ReadString();
                                this.accountBox.Text = this._acc;
                                this.checkBox1.Checked = true;
                                continue;
                            case (byte)1:
                                this.passwordBox.Text = Resources.Stars;
                                this._pwMd5 = binaryReader.ReadBytes(16);
                                this.checkBox1.Checked = true;
                                continue;
                            default:
                                continue;
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
            }
            if (this.accountBox.Text == string.Empty)
                this.accountBox.Focus();
            else
                this.loginButton.Focus();
        }

        private void passwordBox_Enter(object sender, EventArgs e)
        {
            if (!(this.passwordBox.Text == Resources.Stars))
                return;
            this.passwordBox.Text = string.Empty;
        }

        private void passwordBox_Leave(object sender, EventArgs e)
        {
            if (this._pwMd5 == null || !(this.passwordBox.Text == string.Empty))
                return;
            this.passwordBox.Text = Resources.Stars;
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return)
                return;
            this.loginButton_Click(sender, (EventArgs)null);
        }

        private void LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string fileName = e.Link.LinkData as string;
            if (fileName == null)
                return;
            Process.Start(fileName);
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            if (this._acc == string.Empty || this._acc != this.accountBox.Text)
            {
                this._acc = this.accountBox.Text;
                if (this._acc == string.Empty || this._acc.Length < 4)
                {
                    int num = (int)MessageBox.Show(Resources.Error_AccountName, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
            }
            using (MD5 md5 = MD5.Create())
            {
                string text = this.passwordBox.Text;
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                if (this._pwMd5 != null)
                {
                    if (text != "****")
                    {
                        if (this._pwMd5 == hash)
                            goto label_13;
                    }
                    else
                        goto label_13;
                }
                if (text == string.Empty || text.Length < 8)
                {
                    int num = (int)MessageBox.Show(Resources.Error_Password, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                this._pwMd5 = hash;
            }
            label_13:
            try
            {
                using (FileStream fileStream = File.Create("info.dat", 64, FileOptions.None))
                {
                    BinaryWriter binaryWriter = new BinaryWriter((Stream)fileStream);
                    if (this.checkBox1.Checked)
                    {
                        binaryWriter.Write((byte)0);
                        binaryWriter.Write(this._acc);
                        binaryWriter.Write((byte)1);
                        binaryWriter.Write(this._pwMd5);
                    }
                    binaryWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(Resources.Warning_CantCreateFile + ex.Message, Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            try
            {
                this.SendLogin();
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show("Login error: " + Environment.NewLine + ex.Message, Resources.Error, MessageBoxButtons.OK);
            }
        }

        protected override void Dispose(bool disposing)
        {
            //if (disposing && this.components != null) // always false
            //    this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(Login));
            this.label1 = new Label();
            this.label2 = new Label();
            this.passwordBox = new MaskedTextBox();
            this.accountBox = new TextBox();
            this.loginButton = new Button();
            this.checkBox1 = new CheckBox();
            this.forumLink = new LinkLabel();
            this.SuspendLayout();
            this.label1.AutoSize = true;
            this.label1.Location = new Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Account";
            this.label2.AutoSize = true;
            this.label2.Location = new Point(12, 38);
            this.label2.Name = "label2";
            this.label2.Size = new Size(53, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Password";
            this.passwordBox.Location = new Point(62, 35);
            this.passwordBox.Name = "passwordBox";
            this.passwordBox.PasswordChar = '*';
            this.passwordBox.Size = new Size(298, 20);
            this.passwordBox.TabIndex = 2;
            this.passwordBox.Enter += new EventHandler(this.passwordBox_Enter);
            this.passwordBox.KeyDown += new KeyEventHandler(this.passwordBox_KeyDown);
            this.passwordBox.Leave += new EventHandler(this.passwordBox_Leave);
            this.accountBox.Location = new Point(62, 6);
            this.accountBox.Name = "accountBox";
            this.accountBox.Size = new Size(298, 20);
            this.accountBox.TabIndex = 1;
            this.loginButton.Location = new Point(94, 57);
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new Size(266, 23);
            this.loginButton.TabIndex = 4;
            this.loginButton.Text = "Login";
            this.loginButton.UseVisualStyleBackColor = true;
            this.loginButton.Click += new EventHandler(this.loginButton_Click);
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new Point(12, 61);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new Size(76, 17);
            this.checkBox1.TabIndex = 3;
            this.checkBox1.Text = "Save login";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.forumLink.AutoSize = true;
            this.forumLink.Location = new Point(281, 83);
            this.forumLink.Name = "forumLink";
            this.forumLink.Size = new Size(79, 13);
            this.forumLink.TabIndex = 0;
            this.forumLink.TabStop = true;
            this.forumLink.Text = "www.zynox.net";
            this.forumLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.LinkClicked);
            this.AutoScaleDimensions = new SizeF(6f, 13f);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(366, 103);
            this.Controls.Add((Control)this.forumLink);
            this.Controls.Add((Control)this.checkBox1);
            this.Controls.Add((Control)this.loginButton);
            this.Controls.Add((Control)this.accountBox);
            this.Controls.Add((Control)this.passwordBox);
            this.Controls.Add((Control)this.label2);
            this.Controls.Add((Control)this.label1);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = (Icon)componentResourceManager.GetObject("$this.Icon");
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Login";
            this.Text = "Login";
            this.Shown += new EventHandler(this.Login_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
