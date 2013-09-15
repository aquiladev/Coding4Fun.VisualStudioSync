using System;
using System.Globalization;

namespace VisualStudioSync.Live
{
	public class SettingsRepository : ISettingsRepository
	{
		private readonly LiveController _controller;

		private string _settingsPath;

		public SettingsRepository(string settingsPath)
		{
			_settingsPath = settingsPath;
			_controller = new LiveController();
		}

		[STAThread]
		public string GetSettins()
		{
			var s = _controller.GetFile();
			return DateTime.Now.ToString(CultureInfo.InvariantCulture);
		}

		public void SetSettins(string value)
		{
			throw new System.NotImplementedException();
		}
	}
}
