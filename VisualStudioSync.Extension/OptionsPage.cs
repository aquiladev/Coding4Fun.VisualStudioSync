using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;

namespace VisualStudioSync.Extension
{
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[CLSCompliant(false), ComVisible(true)]
	[Guid("2C039A41-6DF6-4236-A179-B176CBD9FE03")]
	public class OptionsPage : DialogPage
	{
		private OptionsUserControl _optionsWindow;
		public event Action<string> SettingsUpdated;

		[Category("Sync"), DisplayName(@"Settings"), Description("Settings for sync")]
		public string Settings { get; set; }

		[Category("Sync"), DisplayName(@"Logging Enabled"), Description("Enable or disable logging")]
		public bool LoggingEnabled { get; set; }

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override IWin32Window Window
		{
			get
			{
				_optionsWindow = new OptionsUserControl { OptionsPage = this };
				_optionsWindow.Initialize();
				return _optionsWindow;
			}
		}

		protected override void OnApply(PageApplyEventArgs e)
		{
			base.OnApply(e);
			OnSettingsDirectoryPathUpdated();
		}

		void OnSettingsDirectoryPathUpdated()
		{
			if (SettingsUpdated != null)
			{
				SettingsUpdated.Invoke(String.Empty);
			}
		}
	}
}
