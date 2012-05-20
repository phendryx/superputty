namespace SuperPutty
{
    partial class dlgFindPutty
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(dlgFindPutty));
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonBrowsePutty = new System.Windows.Forms.Button();
            this.buttonBrowsePscp = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxPuttyLocation = new System.Windows.Forms.TextBox();
            this.textBoxPscpLocation = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.textBoxMinttyLocation = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonBrowseMintty = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(422, 200);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonBrowsePutty
            // 
            this.buttonBrowsePutty.Location = new System.Drawing.Point(422, 111);
            this.buttonBrowsePutty.Name = "buttonBrowsePutty";
            this.buttonBrowsePutty.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowsePutty.TabIndex = 5;
            this.buttonBrowsePutty.Text = "Browse";
            this.buttonBrowsePutty.UseVisualStyleBackColor = true;
            this.buttonBrowsePutty.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonBrowsePscp
            // 
            this.buttonBrowsePscp.Location = new System.Drawing.Point(422, 171);
            this.buttonBrowsePscp.Name = "buttonBrowsePscp";
            this.buttonBrowsePscp.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowsePscp.TabIndex = 25;
            this.buttonBrowsePscp.Text = "Browse";
            this.buttonBrowsePscp.UseVisualStyleBackColor = true;
            this.buttonBrowsePscp.Click += new System.EventHandler(this.buttonBrowsePscp_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(18, 116);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "putty.exe Location (Required)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(22, 176);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(158, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "pscp.exe Location (Optional)";
            // 
            // textBoxPuttyLocation
            // 
            this.textBoxPuttyLocation.Location = new System.Drawing.Point(180, 113);
            this.textBoxPuttyLocation.Name = "textBoxPuttyLocation";
            this.textBoxPuttyLocation.Size = new System.Drawing.Size(236, 20);
            this.textBoxPuttyLocation.TabIndex = 3;
            // 
            // textBoxPscpLocation
            // 
            this.textBoxPscpLocation.Location = new System.Drawing.Point(180, 173);
            this.textBoxPscpLocation.Name = "textBoxPscpLocation";
            this.textBoxPscpLocation.Size = new System.Drawing.Size(236, 20);
            this.textBoxPscpLocation.TabIndex = 20;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.richTextBox1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBox1.Location = new System.Drawing.Point(12, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(485, 95);
            this.richTextBox1.TabIndex = 30;
            this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
            // 
            // textBoxMinttyLocation
            // 
            this.textBoxMinttyLocation.Location = new System.Drawing.Point(180, 143);
            this.textBoxMinttyLocation.Name = "textBoxMinttyLocation";
            this.textBoxMinttyLocation.Size = new System.Drawing.Size(236, 20);
            this.textBoxMinttyLocation.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 146);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(168, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "mintty.exe Location (Optional)";
            // 
            // buttonBrowseMintty
            // 
            this.buttonBrowseMintty.Location = new System.Drawing.Point(422, 141);
            this.buttonBrowseMintty.Name = "buttonBrowseMintty";
            this.buttonBrowseMintty.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowseMintty.TabIndex = 15;
            this.buttonBrowseMintty.Text = "Browse";
            this.buttonBrowseMintty.UseVisualStyleBackColor = true;
            this.buttonBrowseMintty.Click += new System.EventHandler(this.buttonBrowseMintty_Click);
            // 
            // dlgFindPutty
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 234);
            this.Controls.Add(this.textBoxMinttyLocation);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonBrowseMintty);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.textBoxPscpLocation);
            this.Controls.Add(this.textBoxPuttyLocation);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonBrowsePscp);
            this.Controls.Add(this.buttonBrowsePutty);
            this.Controls.Add(this.buttonOk);
            this.Name = "dlgFindPutty";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find PuTTY Executable";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonBrowsePutty;
        private System.Windows.Forms.Button buttonBrowsePscp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxPuttyLocation;
        private System.Windows.Forms.TextBox textBoxPscpLocation;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TextBox textBoxMinttyLocation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonBrowseMintty;
    }
}