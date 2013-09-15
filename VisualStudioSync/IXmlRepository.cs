namespace VisualStudioSync
{
	public interface IXmlRepository
	{
		string GetXml(string path);
		void SetXml(string path, string value);
	}
}
