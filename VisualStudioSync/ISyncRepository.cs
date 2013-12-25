using System.Threading.Tasks;

namespace VisualStudioSync
{
	public interface ISyncRepository
	{
		Task<string> Pull();
		void Push(string value);
	}
}
