using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.VisualStudio.ExtensionManager;

namespace VisualStudioSync.Controllers
{
	public class ExtensionsController : ISyncController
	{
		private readonly IVsExtensionManager _manager;

		public ExtensionsController(IVsExtensionManager manager)
		{
			_manager = manager;
		}

		public string Get()
		{
			var installedExtensions = _manager.GetInstalledExtensions();
			var userExtensions = installedExtensions.Where(ext =>
						!ext.Header.SystemComponent && !ext.Header.InstalledByMsi)
						.OrderBy(ext => ext.Header.Name);

			using (var writer = new StringWriter())
			{
				var serializer = new XmlSerializer(typeof(Extensions));
				serializer.Serialize(writer, new Extensions
				{
					Items = userExtensions
						.Select(e => new Extension
						{
							Name = e.Header.Name,
							Identifier = e.Header.Identifier
						})
						.ToList()
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
				return "extensions";
			}
		}
	}

	[XmlRoot("Extensions")]
	public class Extensions
	{
		[XmlElement("Extension")]
		public List<Extension> Items { get; set; }
	}

	public class Extension : IEquatable<Extension>
	{
		public string Name { get; set; }
		public string Identifier { get; set; }

		public bool Equals(Extension other)
		{
			if (ReferenceEquals(other, null)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Name.Equals(other.Name) && Identifier.Equals(other.Identifier);
		}

		public override int GetHashCode()
		{
			var hashProductName = Name == null ? 0 : Name.GetHashCode();
			var hashIdentifier = Identifier.GetHashCode();
			return hashProductName ^ hashIdentifier;
		}
	}
}