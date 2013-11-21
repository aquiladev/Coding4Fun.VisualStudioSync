using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace VisualStudioSync.Live
{
	public class LiveWatcher : IFileWatcher
	{
		public int Interval { get; set; }
		public event EventHandler Changed;

		private readonly LiveController _controller;
		private readonly Timer _timer;
		private string _lastSum;
		private string _actSum;

		public LiveWatcher(LiveController controller)
		{
			_controller = controller;
			_timer = new Timer(_ => OnTick(), null, Timeout.Infinite, Timeout.Infinite);
			_lastSum = string.Empty;
			_actSum = string.Empty;
			Interval = 10;
		}

		public void Start()
		{
			_timer.Change(0, GetInterval());
		}

		public void Stop()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		protected virtual void OnShapeChanged(EventArgs e)
		{
			if (Changed != null)
			{
				Changed(this, e);
			}
		}

		private void OnTick()
		{
			_timer.Change(Timeout.Infinite, Timeout.Infinite);
			try
			{
				var thread = new Thread(Checksum);
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
			}
			finally
			{
				_timer.Change(GetInterval(), GetInterval());
			}
		}

		private int GetInterval()
		{
			return (int)TimeSpan.FromSeconds(Interval).TotalMilliseconds;
		}

		private async void Checksum()
		{
			var blob = await _controller.GetBlob();
			if (blob == null)
			{
				return;
			}

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(blob.Value ?? string.Empty)))
			{
				using (var md5 = MD5.Create())
				{
					_actSum = BitConverter.ToString(md5.ComputeHash(stream));
				}
			}

			if (_actSum == _lastSum)
			{
				return;
			}

			OnShapeChanged(new EventArgs());
			_lastSum = _actSum;
		}
	}
}
