using System.Threading.Tasks;
using VisualStudioSync.Models;

namespace VisualStudioSync
{
	public interface ISyncRepository
	{
		Task<Blob> Pull();
		void Push(string value);
	}
}
