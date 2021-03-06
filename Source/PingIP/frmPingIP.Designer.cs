﻿namespace PingIP
{
    partial class frmPingIPs
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
            this.txtPingResult = new System.Windows.Forms.TextBox();
            this.btnStartPing = new System.Windows.Forms.Button();
            this.pbProcessing = new System.Windows.Forms.ProgressBar();
            this.btnClipBoard = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ddlCountry = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // txtPingResult
            // 
            this.txtPingResult.Location = new System.Drawing.Point(13, 42);
            this.txtPingResult.Multiline = true;
            this.txtPingResult.Name = "txtPingResult";
            this.txtPingResult.ReadOnly = true;
            this.txtPingResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPingResult.Size = new System.Drawing.Size(706, 374);
            this.txtPingResult.TabIndex = 0;
            this.txtPingResult.TextChanged += new System.EventHandler(this.txtPingResult_TextChanged);
            // 
            // btnStartPing
            // 
            this.btnStartPing.Location = new System.Drawing.Point(251, 12);
            this.btnStartPing.Name = "btnStartPing";
            this.btnStartPing.Size = new System.Drawing.Size(75, 23);
            this.btnStartPing.TabIndex = 1;
            this.btnStartPing.Text = "Start Test";
            this.btnStartPing.UseVisualStyleBackColor = true;
            this.btnStartPing.Click += new System.EventHandler(this.btnStartPing_Click);
            // 
            // pbProcessing
            // 
            this.pbProcessing.Location = new System.Drawing.Point(13, 423);
            this.pbProcessing.Name = "pbProcessing";
            this.pbProcessing.Size = new System.Drawing.Size(470, 25);
            this.pbProcessing.TabIndex = 2;
            // 
            // btnClipBoard
            // 
            this.btnClipBoard.Location = new System.Drawing.Point(501, 425);
            this.btnClipBoard.Name = "btnClipBoard";
            this.btnClipBoard.Size = new System.Drawing.Size(116, 23);
            this.btnClipBoard.TabIndex = 3;
            this.btnClipBoard.Text = "Copy to Clipboard";
            this.btnClipBoard.UseVisualStyleBackColor = true;
            this.btnClipBoard.Click += new System.EventHandler(this.btnClipBoard_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(633, 425);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(86, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save to File";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(43, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Country";
            // 
            // ddlCountry
            // 
            this.ddlCountry.FormattingEnabled = true;
            this.ddlCountry.Location = new System.Drawing.Point(61, 14);
            this.ddlCountry.Name = "ddlCountry";
            this.ddlCountry.Size = new System.Drawing.Size(163, 21);
            this.ddlCountry.TabIndex = 6;
            // 
            // frmPingIPs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(731, 464);
            this.Controls.Add(this.ddlCountry);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClipBoard);
            this.Controls.Add(this.pbProcessing);
            this.Controls.Add(this.btnStartPing);
            this.Controls.Add(this.txtPingResult);
            this.Name = "frmPingIPs";
            this.Text = "Server Connection Analyzer";
            this.Shown += new System.EventHandler(this.frmPingIPs_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtPingResult;
        private System.Windows.Forms.Button btnStartPing;
        private System.Windows.Forms.ProgressBar pbProcessing;
        private System.Windows.Forms.Button btnClipBoard;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddlCountry;
    }
}

