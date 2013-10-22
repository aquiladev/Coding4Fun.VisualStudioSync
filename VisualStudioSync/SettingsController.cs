using System;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace VisualStudioSync
{
	public class SettingsController : ISyncController
	{
		public const string FileName = "vs_sync_temp.vssettings";

		private readonly string _fullPath;
		private readonly IVsProfileDataManager _manager;
		private readonly IXmlRepository _xmlRepository;
		private IVsSettingsErrorInformation _errorInformation;

		public SettingsController(string extPath, 
			IVsProfileDataManager manager,
			IXmlRepository xmlRepository)
		{
			_fullPath = Path.Combine(extPath, FileName);
			_manager = manager;
			_xmlRepository = xmlRepository;
		}

		public string Get()
		{
			_manager.ExportAllSettings(_fullPath, out _errorInformation);
			return _xmlRepository.GetXml(_fullPath);
		}

		public void Set(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentNullException("value");
			}

			IVsProfileSettingsFileInfo settingsFileInfo;
			IVsProfileSettingsFileCollection files;
			IVsProfileSettingsTree sets;

			_xmlRepository.SetXml(_fullPath, value);
			_manager.GetSettingsFiles(0, out files);
			files.AddBrowseFile(_fullPath, out settingsFileInfo);
			settingsFileInfo.GetSettingsForImport(out sets);
			_manager.ImportSettings(sets, out _errorInformation);
		}

		public string Name
		{
			get
			{
				return "settings";
			}
		}
	}
}
