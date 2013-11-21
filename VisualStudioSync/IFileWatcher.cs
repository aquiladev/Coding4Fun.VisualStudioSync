using System;

namespace VisualStudioSync
{
	public interface IFileWatcher
	{
		int Interval { get; set; }
		event EventHandler Changed;
		void Start();
		void Stop();
	}
}
