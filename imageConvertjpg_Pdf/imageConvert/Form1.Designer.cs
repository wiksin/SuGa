namespace imageConvert
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnStart = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtFrompath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtToPath = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.process1 = new System.Diagnostics.Process();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(174, 95);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "开始转换";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(24, 14);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 1;
            this.btnSelect.Text = "选择路径";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // txtFrompath
            // 
            this.txtFrompath.Enabled = false;
            this.txtFrompath.Location = new System.Drawing.Point(105, 14);
            this.txtFrompath.Name = "txtFrompath";
            this.txtFrompath.Size = new System.Drawing.Size(272, 21);
            this.txtFrompath.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 60);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "输出路径：";
            // 
            // txtToPath
            // 
            this.txtToPath.Location = new System.Drawing.Point(105, 57);
            this.txtToPath.Name = "txtToPath";
            this.txtToPath.Size = new System.Drawing.Size(272, 21);
            this.txtToPath.TabIndex = 4;
            this.txtToPath.Text = "D:\\toPath";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // process1
            // 
            this.process1.StartInfo.Domain = "";
            this.process1.StartInfo.LoadUserProfile = false;
            this.process1.StartInfo.Password = null;
            this.process1.StartInfo.StandardErrorEncoding = null;
            this.process1.StartInfo.StandardOutputEncoding = null;
            this.process1.StartInfo.UserName = "";
            this.process1.SynchronizingObject = this;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 262);
            this.Controls.Add(this.txtToPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtFrompath);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox txtFrompath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtToPath;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Diagnostics.Process process1;
    }
}

