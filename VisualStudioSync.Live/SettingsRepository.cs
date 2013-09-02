using System;
using System.Globalization;

namespace VisualStudioSync.Live
{
	public class SettingsRepository : ISettingsRepository
	{
		[STAThread]
		public string GetSettins()
		{
			var c = new LiveController();
			var s = c.GetFile();
			return DateTime.Now.ToString(CultureInfo.InvariantCulture);
		}

		public void SetSettins(string value)
		{
			throw new System.NotImplementedException();
		}
	}
}
