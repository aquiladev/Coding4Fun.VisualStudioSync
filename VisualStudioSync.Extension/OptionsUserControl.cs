using System;
using System.Windows.Forms;

namespace VisualStudioSync.Extension
{
	public partial class OptionsUserControl : UserControl
	{
		public OptionsPage OptionsPage { get; set; }

		public OptionsUserControl()
		{
			InitializeComponent();
		}

		public void Initialize()
		{
			UpdateTextBoxes();
			cbxEnableLogging.Checked = OptionsPage.LoggingEnabled;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateTextBoxes();
		}

		private void UpdateTextBoxes()
		{
			txtSettings.Text = OptionsPage.Settings;
		}

		private void CbxEnableLoggingCheckedChanged(object sender, EventArgs e)
		{
			OptionsPage.LoggingEnabled = cbxEnableLogging.Checked;
		}
	}
}
