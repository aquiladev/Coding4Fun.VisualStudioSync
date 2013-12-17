using System;
using Microsoft.VisualStudio.Shell.Interop;
using NUnit.Framework;
using Telerik.JustMock;
using VisualStudioSync.Controllers;

namespace VisualStudioSync.Tests
{
	[TestFixture]
	public class SettingsControllerTest
	{
		private const string Path = "path";
		private const string Value = "test";

		private IVsProfileDataManager _manager;
		private IVsSettingsErrorInformation _errorInformation;
		private IVsProfileSettingsFileCollection _files;
		private IXmlRepository _xmlRepository;
		private IVsProfileSettingsFileInfo _settingsFileInfo;
		private IVsProfileSettingsTree _sets;

		[SetUp]
		public void Setup()
		{
			_manager = Mock.Create<IVsProfileDataManager>();
			_errorInformation = Mock.Create<IVsSettingsErrorInformation>();
			_files = Mock.Create<IVsProfileSettingsFileCollection>();
			_xmlRepository = Mock.Create<IXmlRepository>();
			_settingsFileInfo = Mock.Create<IVsProfileSettingsFileInfo>();
			_sets = Mock.Create<IVsProfileSettingsTree>();
		}

		[Test]
		public void Get_CheckManager_CalledExportAllSettings()
		{
			//Arrange
			Mock.Arrange(() => _manager.ExportAllSettings(
				Arg.Matches<string>(path => path.StartsWith(Path)),
				out _errorInformation))
				.MustBeCalled();

			//Act
			CreateController().Get();

			//Assert
			Mock.AssertAll(_manager);
		}

		[Test]
		public void Get_CheckRepository_CalledGetXml()
		{
			//Arrange
			Mock.Arrange(() => _xmlRepository.GetXml(
				Arg.Matches<string>(path => path.StartsWith(Path))))
				.MustBeCalled();

			//Act
			CreateController().Get();

			//Assert
			Mock.AssertAll(_xmlRepository);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Set_ValueIsNull_ExpectedException()
		{
			//Act
			CreateController().Set(null);
		}

		[Test]
		public void Set_ValueIsntNull_CalledSetXml()
		{
			//Arrange
			Mock.Arrange(() => _xmlRepository.SetXml(
				Arg.Matches<string>(path => path.StartsWith(Path)),
				Value))
				.MustBeCalled();
			Mock.Arrange(() => _files.AddBrowseFile(Arg.AnyString, out _settingsFileInfo)).DoNothing();
			Mock.Arrange(() => _manager.GetSettingsFiles(0, out _files));

			//Act
			CreateController().Set(Value);

			//Assert
			Mock.AssertAll(_xmlRepository);
		}

		[Test]
		public void Set_ValueIsntNull_CalleGetSettingsFiles()
		{
			//Arrange
			Mock.Arrange(() => _manager.GetSettingsFiles(0, out _files))
				.MustBeCalled();
			Mock.Arrange(() => _files.AddBrowseFile(Arg.AnyString, out _settingsFileInfo)).DoNothing();
			Mock.Arrange(() => _settingsFileInfo.GetSettingsForImport(out _sets)).DoNothing();

			//Act
			CreateController().Set(Value);

			//Assert
			Mock.AssertAll(_xmlRepository);
		}

		[Test]
		public void Set_ValueIsntNull_CalleAddBrowseFile()
		{
			//Arrange
			Mock.Arrange(() => _files.AddBrowseFile(
				Arg.Matches<string>(path => path.StartsWith(Path)),
				out _settingsFileInfo))
				.MustBeCalled();
			Mock.Arrange(() => _manager.GetSettingsFiles(0, out _files));

			//Act
			CreateController().Set(Value);

			//Assert
			Mock.AssertAll(_xmlRepository);
		}

		[Test]
		public void Set_ValueIsntNull_CalleGetSettingsForImport()
		{
			//Arrange
			Mock.Arrange(() => _settingsFileInfo.GetSettingsForImport(out _sets))
				.MustBeCalled();
			Mock.Arrange(() => _manager.GetSettingsFiles(0, out _files));
			Mock.Arrange(() => _files.AddBrowseFile(Arg.AnyString, out _settingsFileInfo));

			//Act
			CreateController().Set(Value);

			//Assert
			Mock.AssertAll(_xmlRepository);
		}

		[Test]
		public void Set_ValueIsntNull_CalleImportSettings()
		{
			//Arrange
			Mock.Arrange(() => _manager.ImportSettings(_sets, out _errorInformation))
				.MustBeCalled();
			Mock.Arrange(() => _manager.GetSettingsFiles(0, out _files));
			Mock.Arrange(() => _files.AddBrowseFile(Arg.AnyString, out _settingsFileInfo)).DoNothing();

			//Act
			CreateController().Set(Value);

			//Assert
			Mock.AssertAll(_xmlRepository);
		}

		private SettingsController CreateController()
		{
			return new SettingsController(Path, _manager, _xmlRepository);
		}
	}
}
