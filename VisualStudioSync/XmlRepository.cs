using System.Xml;

namespace VisualStudioSync
{
	public class XmlRepository : IXmlRepository
	{
		public string GetXml(string path)
		{
			using (var reader = XmlReader.Create(path))
			{
				return reader.ReadOuterXml();
			}
		}

		public void SetXml(string path, string value)
		{
			using (var writer = XmlWriter.Create(path))
			{
				writer.WriteString(value);
			}
		}
	}
}
