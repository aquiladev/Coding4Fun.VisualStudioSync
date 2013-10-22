namespace VisualStudioSync
{
	public interface ISyncController
	{
		string Get();
		void Set(string value);
		string Name { get; }
	}
}
