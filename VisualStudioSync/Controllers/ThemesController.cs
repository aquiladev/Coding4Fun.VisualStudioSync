using System.IO;
using System.Xml.Serialization;

namespace VisualStudioSync.Controllers
{
	public class ThemesController : ISyncController
	{
		public string Get()
		{
			var themeId = string.Empty;
			var registry = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\11.0\General");
			if (registry != null)
			{
				themeId = registry.GetValue("CurrentTheme").ToString();
			}

			using (var writer = new StringWriter())
			{
				var serializer = new XmlSerializer(typeof(Theme));
				serializer.Serialize(writer, new Theme
				{
					ThemeId = themeId
				});
				return writer.ToString();
			}
		}

		public void Set(string value)
		{

		}

		public string Name
		{
			get
			{
				return "themes";
			}
		}
	}

	public class Theme
	{
		public string ThemeId { get; set; }
	}
}
