using System.Threading.Tasks;
using VisualStudioSync.Models;

namespace VisualStudioSync.Live
{
	public class LiveRepository : ISyncRepository
	{
		private readonly LiveController _controller;

		public LiveRepository(LiveController controller)
		{
			_controller = controller;
		}

		public async Task<Blob> Pull()
		{
			return await _controller.GetBlob();
		}

		public void Push(string value)
		{
			_controller.SaveBlob(value);
		}
	}
}
