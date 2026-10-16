namespace FormApp
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnStartPoll = new Button();
            btnStopPoll = new Button();
            txt_lblStatus = new TextBox();
            lbl_log = new Label();
            label1 = new Label();
            txtD100 = new TextBox();
            label2 = new Label();
            txtD101 = new TextBox();
            label3 = new Label();
            txtD102 = new TextBox();
            label4 = new Label();
            txtD103 = new TextBox();
            label5 = new Label();
            txtD104 = new TextBox();
            btn_write_Click = new Button();
            txtLog = new TextBox();
            SuspendLayout();
            // 
            // btnStartPoll
            // 
            btnStartPoll.Location = new Point(587, 12);
            btnStartPoll.Name = "btnStartPoll";
            btnStartPoll.Size = new Size(201, 80);
            btnStartPoll.TabIndex = 0;
            btnStartPoll.Text = "StartPoll";
            btnStartPoll.UseVisualStyleBackColor = true;
            btnStartPoll.Click += btn_Start_Poll;
            // 
            // btnStopPoll
            // 
            btnStopPoll.Location = new Point(587, 125);
            btnStopPoll.Name = "btnStopPoll";
            btnStopPoll.Size = new Size(201, 80);
            btnStopPoll.TabIndex = 1;
            btnStopPoll.Text = "StopPoll";
            btnStopPoll.UseVisualStyleBackColor = true;
            btnStopPoll.Click += btn_Stop_Poll;
            // 
            // txt_lblStatus
            // 
            txt_lblStatus.Location = new Point(152, 30);
            txt_lblStatus.Name = "txt_lblStatus";
            txt_lblStatus.Size = new Size(150, 27);
            txt_lblStatus.TabIndex = 2;
            txt_lblStatus.UseWaitCursor = true;
            // 
            // lbl_log
            // 
            lbl_log.AutoSize = true;
            lbl_log.Location = new Point(77, 33);
            lbl_log.Name = "lbl_log";
            lbl_log.Size = new Size(32, 19);
            lbl_log.TabIndex = 3;
            lbl_log.Text = "log";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(77, 89);
            label1.Name = "label1";
            label1.Size = new Size(47, 19);
            label1.TabIndex = 5;
            label1.Text = "D100";
            // 
            // txtD100
            // 
            txtD100.Location = new Point(152, 86);
            txtD100.Name = "txtD100";
            txtD100.Size = new Size(150, 27);
            txtD100.TabIndex = 4;
            txtD100.UseWaitCursor = true;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(77, 138);
            label2.Name = "label2";
            label2.Size = new Size(47, 19);
            label2.TabIndex = 7;
            label2.Text = "D101";
            // 
            // txtD101
            // 
            txtD101.Location = new Point(152, 135);
            txtD101.Name = "txtD101";
            txtD101.Size = new Size(150, 27);
            txtD101.TabIndex = 6;
            txtD101.UseWaitCursor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(77, 191);
            label3.Name = "label3";
            label3.Size = new Size(47, 19);
            label3.TabIndex = 9;
            label3.Text = "D102";
            // 
            // txtD102
            // 
            txtD102.Location = new Point(152, 188);
            txtD102.Name = "txtD102";
            txtD102.Size = new Size(150, 27);
            txtD102.TabIndex = 8;
            txtD102.UseWaitCursor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(77, 238);
            label4.Name = "label4";
            label4.Size = new Size(47, 19);
            label4.TabIndex = 11;
            label4.Text = "D103";
            // 
            // txtD103
            // 
            txtD103.Location = new Point(152, 235);
            txtD103.Name = "txtD103";
            txtD103.Size = new Size(150, 27);
            txtD103.TabIndex = 10;
            txtD103.UseWaitCursor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(77, 286);
            label5.Name = "label5";
            label5.Size = new Size(47, 19);
            label5.TabIndex = 13;
            label5.Text = "D104";
            // 
            // txtD104
            // 
            txtD104.Location = new Point(152, 283);
            txtD104.Name = "txtD104";
            txtD104.Size = new Size(150, 27);
            txtD104.TabIndex = 12;
            txtD104.UseWaitCursor = true;
            // 
            // btn_write_Click
            // 
            btn_write_Click.Location = new Point(587, 234);
            btn_write_Click.Name = "btn_write_Click";
            btn_write_Click.Size = new Size(201, 80);
            btn_write_Click.TabIndex = 14;
            btn_write_Click.Text = "Write";
            btn_write_Click.UseVisualStyleBackColor = true;
            btn_write_Click.Click += btn_write_click;
            // 
            // txtLog
            // 
            txtLog.Location = new Point(350, 30);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(198, 27);
            txtLog.TabIndex = 15;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(9F, 19F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(txtLog);
            Controls.Add(btn_write_Click);
            Controls.Add(label5);
            Controls.Add(txtD104);
            Controls.Add(label4);
            Controls.Add(txtD103);
            Controls.Add(label3);
            Controls.Add(txtD102);
            Controls.Add(label2);
            Controls.Add(txtD101);
            Controls.Add(label1);
            Controls.Add(txtD100);
            Controls.Add(lbl_log);
            Controls.Add(txt_lblStatus);
            Controls.Add(btnStopPoll);
            Controls.Add(btnStartPoll);
            Name = "Form1";
            Text = "Form1";
            FormClosing += OnFormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStartPoll;
        private Button btnStopPoll;
        private TextBox txt_lblStatus;
        private Label lbl_log;
        private Label label1;
        private TextBox txtD100;
        private Label label2;
        private TextBox txtD101;
        private Label label3;
        private TextBox txtD102;
        private Label label4;
        private TextBox txtD103;
        private Label label5;
        private TextBox txtD104;
        private Button btn_write_Click;
        private TextBox txtLog;
    }
}
