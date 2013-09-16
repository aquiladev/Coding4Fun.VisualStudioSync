using System.Xml;

namespace VisualStudioSync
{
	public class XmlRepository : IXmlRepository
	{
		public string GetXml(string path)
		{
			var doc = new XmlDocument();
			doc.Load(path);
			return doc.OuterXml;
		}

		public void SetXml(string path, string value)
		{
			var doc = new XmlDocument();
			doc.LoadXml(value);
			doc.Save(path);
		}
	}
}
