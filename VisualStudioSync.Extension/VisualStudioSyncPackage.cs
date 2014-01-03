using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Autofac;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ExtensionManager;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using SkyDrive;
using SkyDrive.Threading;
using VisualStudioSync.Controllers;
using VisualStudioSync.Live;

namespace VisualStudioSync.Extension
{
	[ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[Guid(GuidList.guidVisualStudioSyncPkgString)]
	[ProvideOptionPage(typeof(OptionsPage), "Sync", "General", 0, 0, true)]
	public sealed class VisualStudioSyncPackage : Package, IVsShellPropertyEvents
	{
		private static IContainer Container { get; set; }

		private uint _cookie;
		private OptionsPage _options;
		private ISyncManager _manager;
		private DTE2 _applicationObject;
		private DTEEvents _packageDTEEvents;
		private IFileWatcher _fileWatcher;
		private const string FilePath = @"VS Sync\vs.sync";

		public DTE2 ApplicationObject
		{
			get
			{
				if (_applicationObject != null)
				{
					return _applicationObject;
				}

				var dte = (DTE)GetService(typeof(DTE));
				_applicationObject = dte as DTE2;
				return _applicationObject;
			}
		}

		#region Package Members

		protected override void Initialize()
		{
			base.Initialize();
			InitializeContainer();
			InitializeManager();
			InitializeWatcher();
			InitializePackage();
		}

		private static void InitializeContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<SyncManager>()
				.WithParameter(new NamedParameter("path", FilePath))
				.As<ISyncManager>();
			builder.RegisterType<LiveRepository>()
				.As<ISyncRepository>();
			builder.RegisterType<SettingsController>()
				.As<ISyncController>()
				.WithParameter(new NamedParameter("extPath", Path.GetTempPath()))
				.WithParameter(new NamedParameter("manager", GetGlobalService(typeof(SVsProfileDataManager)) as IVsProfileDataManager))
				.PreserveExistingDefaults();
			builder.RegisterType<ExtensionsController>()
				.As<ISyncController>()
				.WithParameter(new NamedParameter("manager", GetGlobalService(typeof(SVsExtensionManager)) as IVsExtensionManager))
				.PreserveExistingDefaults();
			//builder.RegisterType<ThemesController>()
			//	.As<ISyncController>()
			//	.PreserveExistingDefaults();
			builder.RegisterType<XmlRepository>()
				.As<IXmlRepository>();
			builder.RegisterType<FileWatcher>()
				.As<IFileWatcher>()
				.WithParameter(new NamedParameter("path", FilePath));
			builder.RegisterType<LiveController>()
				.As<ILiveController>()
				.WithParameter(new NamedParameter("clientId", "00000000481024B2"))
				.WithParameter(new NamedParameter("ensureFolder", true))
				.SingleInstance();
			builder.RegisterType<ThreadingTimer>()
				.As<ITimer>()
				.WithParameter(new NamedParameter("interval", 20))
				.SingleInstance();
			Container = builder.Build();
		}

		private void InitializeManager()
		{
			_manager = Container.Resolve<ISyncManager>();
		}

		private void InitializeWatcher()
		{
			_fileWatcher = Container.Resolve<IFileWatcher>();
			_fileWatcher.Changed += (obj, e) => Synchronize();
		}

		private void InitializePackage()
		{
			var shellService = GetService(typeof(SVsShell)) as IVsShell;
			if (shellService != null)
			{
				ErrorHandler.ThrowOnFailure(shellService.AdviseShellPropertyChanges(this, out _cookie));
			}

			_packageDTEEvents = ApplicationObject.Events.DTEEvents;
			_packageDTEEvents.OnBeginShutdown += OnBeginShutdown;
		}

		void OnBeginShutdown()
		{
			Synchronize();
			_fileWatcher.Stop();
		}

		void Synchronize()
		{
			try
			{
				_manager.Sync();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		#endregion

		#region Implementation IVsShellPropertyEvents

		public int OnShellPropertyChange(int propid, object var)
		{
			if ((int)__VSSPROPID.VSSPROPID_Zombie == propid)
			{
				if ((bool)var == false)
				{
					var shellService = GetService(typeof(SVsShell)) as IVsShell;
					if (shellService != null)
						ErrorHandler.ThrowOnFailure(shellService.UnadviseShellPropertyChanges(_cookie));
					_cookie = 0;

					_options = (OptionsPage)GetDialogPage(typeof(OptionsPage));
					_options.SettingsUpdated += OptionsPageSettingsUpdated;

					_fileWatcher.Start();
					Synchronize();
				}
			}
			return VSConstants.S_OK;
		}

		void OptionsPageSettingsUpdated(string settings)
		{
			try
			{
				_fileWatcher.Stop();
				_manager.Push();
				_fileWatcher.Start();
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		#endregion
	}
}
