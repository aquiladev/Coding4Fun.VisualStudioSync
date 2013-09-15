using System;

namespace VisualStudioSync.Live
{
	public class LiveRepository : ISyncRepository
	{
		private readonly LiveController _controller;
		
		public LiveRepository()
		{
			_controller = new LiveController();
		}

		[STAThread]
		public string Pull()
		{
			return _controller.GetFile();
		}

		public void Push(string value)
		{
			throw new System.NotImplementedException();
		}
	}
}
