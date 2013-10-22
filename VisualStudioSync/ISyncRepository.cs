using VisualStudioSync.Models;

namespace VisualStudioSync
{
	public interface ISyncRepository
	{
		Blob Pull();
		void Push(string value);
	}
}
