namespace VisualStudioSync.Extension
{
	partial class OptionsUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.cbxEnableLogging = new System.Windows.Forms.CheckBox();
			this.txtSettings = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// cbxEnableLogging
			// 
			this.cbxEnableLogging.AutoSize = true;
			this.cbxEnableLogging.Location = new System.Drawing.Point(4, 44);
			this.cbxEnableLogging.Margin = new System.Windows.Forms.Padding(4);
			this.cbxEnableLogging.Name = "cbxEnableLogging";
			this.cbxEnableLogging.Size = new System.Drawing.Size(129, 21);
			this.cbxEnableLogging.TabIndex = 7;
			this.cbxEnableLogging.Text = "Enable Logging";
			this.cbxEnableLogging.UseVisualStyleBackColor = true;
			this.cbxEnableLogging.CheckedChanged += new System.EventHandler(this.CbxEnableLoggingCheckedChanged);
			// 
			// txtSettings
			// 
			this.txtSettings.Location = new System.Drawing.Point(4, 14);
			this.txtSettings.Margin = new System.Windows.Forms.Padding(4);
			this.txtSettings.Name = "txtSettings";
			this.txtSettings.ReadOnly = true;
			this.txtSettings.Size = new System.Drawing.Size(394, 22);
			this.txtSettings.TabIndex = 8;
			// 
			// OptionsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.txtSettings);
			this.Controls.Add(this.cbxEnableLogging);
			this.Name = "OptionsUserControl";
			this.Size = new System.Drawing.Size(530, 200);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.CheckBox cbxEnableLogging;
		private System.Windows.Forms.TextBox txtSettings;
	}
}
