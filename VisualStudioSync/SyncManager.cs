﻿using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace VisualStudioSync
{
	public class SyncManager : ISyncManager
	{
		private readonly ISyncRepository _repository;
		private readonly IEnumerable<ISyncController> _controllers;
		private readonly string _path;

		public SyncManager(ISyncRepository repository, 
			IEnumerable<ISyncController> controllers,
			string path)
		{
			_repository = repository;
			_controllers = controllers;
			_path = path;
		}

		public async void Sync()
		{
			var repoValue = await _repository.Pull(_path);
			if (repoValue == null
				|| string.IsNullOrEmpty(repoValue))
			{
				Push();
			}
			else
			{
				Pull(repoValue);
			}
		}

		public void Push()
		{
			var value = GetValue();
			_repository.Push(_path, value);
		}

		#region Private methods

		private string GetValue()
		{
			var doc = new XmlDocument();
			var root = doc.CreateElement("vssync");
			doc.AppendChild(root);
			foreach (var controller in _controllers)
			{
				var child = doc.CreateElement("ctrl");
				child.SetAttribute("name", controller.Name);

				var value = controller.Get();
				var controllerDoc = new XmlDocument();
				controllerDoc.LoadXml(value);
				if (controllerDoc.DocumentElement != null)
				{
					var nodeToCopy = doc.ImportNode(controllerDoc.DocumentElement, true);
					child.AppendChild(nodeToCopy);
				}
				root.AppendChild(child);
			}

			return doc.OuterXml;
		}

		private void Pull(string value)
		{
			using (var stream = new StringReader(value))
			{
				var xDoc = XDocument.Load(stream);
				var ctrls = from c in xDoc.Descendants("ctrl")
							select new
							{
								Name = c.Attribute("name").Value.ToString(CultureInfo.InvariantCulture),
								Value = c.FirstNode.ToString()
							};

				foreach (var ctrl in ctrls)
				{
					var controller = _controllers.FirstOrDefault(c => c.Name == ctrl.Name);
					if (controller == null)
					{
						continue;
					}
					controller.Set(ctrl.Value);
				}
			}
		}

		#endregion
	}
}
