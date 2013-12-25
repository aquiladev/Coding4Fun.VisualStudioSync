using System.Threading.Tasks;
using SkyDrive;

namespace VisualStudioSync.Live
{
	public class LiveRepository : ISyncRepository
	{
		private readonly ILiveController _controller;

		public LiveRepository(ILiveController controller)
		{
			_controller = controller;
		}

		public async Task<string> Pull()
		{
			return await _controller.GetBlob();
		}

		public void Push(string value)
		{
			_controller.SaveBlob(value);
		}
	}
}
