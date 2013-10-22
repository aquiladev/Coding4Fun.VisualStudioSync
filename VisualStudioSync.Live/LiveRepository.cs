using System;
using VisualStudioSync.Models;

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
		public Blob Pull()
		{
			return _controller.GetBlob();
		}

		public void Push(string value)
		{
			_controller.SaveBlob(value);
		}
	}
}
