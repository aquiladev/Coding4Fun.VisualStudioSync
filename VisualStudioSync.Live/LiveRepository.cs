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

		public async Task<string> Pull(string path)
		{
			return await _controller.GetFile(path);
		}

		public void Push(string path, string value)
		{
			_controller.SaveFile(path, value);
		}
	}
}
