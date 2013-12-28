using System.Threading.Tasks;

namespace VisualStudioSync
{
	public interface ISyncRepository
	{
		Task<string> Pull(string path);
		void Push(string path, string value);
	}
}
