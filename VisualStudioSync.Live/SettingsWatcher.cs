using System;
using System.Timers;

namespace VisualStudioSync.Live
{
	public class SettingsWatcher : ISettingsWatcher
	{
		private const int Interval = 60000;
		private readonly Timer _timer;

		public event EventHandler Changed;

		public SettingsWatcher()
		{
			_timer = new Timer(Interval) { Enabled = true };
			_timer.Elapsed += OnTimedEvent;
		}

		~SettingsWatcher()
		{
			Dispose(false);
		}

		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			if (Changed != null)
			{
				Changed(this, e);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			_timer.Stop();
			_timer.Dispose();
		}
	}
}
