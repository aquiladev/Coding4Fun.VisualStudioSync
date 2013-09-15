namespace VisualStudioSync
{
	public interface ISyncRepository
	{
		string Pull();
		void Push(string value);
	}
}
