using System;

namespace VisualStudioSync
{
	public interface ISettingsWatcher : IDisposable
	{
		event EventHandler Changed;
	}
}
