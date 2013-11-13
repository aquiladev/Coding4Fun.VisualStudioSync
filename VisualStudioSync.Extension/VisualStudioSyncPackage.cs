using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using VisualStudioSync.Live;

namespace VisualStudioSync.Extension
{
	[ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	[Guid(GuidList.guidVisualStudioSyncPkgString)]
	[ProvideOptionPage(typeof(OptionsPage), "Extension Sync", "General", 0, 0, true)]
	public sealed class VisualStudioSyncPackage : Package, IVsShellPropertyEvents
	{
		private static IContainer Container { get; set; }

		private uint _cookie;
		private OptionsPage _options;
		private ISyncManager _manager;
		private DTE2 _applicationObject;
		private DTEEvents _packageDTEEvents;
		private IFileWatcher _fileWatcher;

		public DTE2 ApplicationObject
		{
			get
			{
				if (_applicationObject != null)
				{
					return _applicationObject;
				}
				// Get an instance of the currently running Visual Studio IDE
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
			InitializeWatcher();
			var shellService = GetService(typeof(SVsShell)) as IVsShell;
			if (shellService != null)
			{
				ErrorHandler.ThrowOnFailure(shellService.AdviseShellPropertyChanges(this, out _cookie));
			}

			_packageDTEEvents = ApplicationObject.Events.DTEEvents;
			_packageDTEEvents.OnBeginShutdown += _packageDTEEvents_OnBeginShutdown;

			_manager = Container.Resolve<ISyncManager>();
		}

		#endregion

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

		void _packageDTEEvents_OnBeginShutdown()
		{
			Synchronize();
			_fileWatcher.Stop();
		}

		void OptionsPageSettingsUpdated(string settings)
		{
			_fileWatcher.Stop();
			_manager.Push();
			_fileWatcher.Start();
		}

		private void OnFileWatcherOnChanged(object s, EventArgs e)
		{
			Synchronize();
		}

		void Synchronize()
		{
			lock (this)
			{
				_manager.Sync();
			}
		}

		private static string GetPackageInstallationFolder()
		{
			var packageType = typeof(VisualStudioSyncPackage);
			var assemblyCodeBaseUri = new Uri(packageType.Assembly.CodeBase, UriKind.Absolute);
			var assemblyFileInfo = new FileInfo(assemblyCodeBaseUri.LocalPath);
			return assemblyFileInfo.Directory != null
				? assemblyFileInfo.Directory.FullName
				: string.Empty;
		}

		private void InitializeWatcher()
		{
			_fileWatcher = Container.Resolve<IFileWatcher>();
			_fileWatcher.Interval = 10;
			_fileWatcher.Changed += OnFileWatcherOnChanged;
		}

		#region Init Container

		private static void InitializeContainer()
		{
			var path = GetPackageInstallationFolder();
			var builder = new ContainerBuilder();
			builder.RegisterType<SyncManager>()
				.As<ISyncManager>();
			builder.RegisterType<LiveRepository>()
				.As<ISyncRepository>();
			builder.RegisterType<SettingsController>()
				.As<ISyncController>()
				.WithParameter(new NamedParameter("extPath", path))
				.WithParameter(new NamedParameter("manager", GetGlobalService(typeof(SVsProfileDataManager)) as IVsProfileDataManager))
				.PreserveExistingDefaults();
			builder.RegisterType<XmlRepository>()
				.As<IXmlRepository>();
			builder.RegisterType<LiveWatcher>()
				.As<IFileWatcher>()
				.WithParameter(new NamedParameter("controller", new LiveController()));
			Container = builder.Build();
		}

		#endregion
	}
}
