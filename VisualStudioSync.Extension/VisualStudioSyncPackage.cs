using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Autofac;
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
		private uint _cookie;
		private OptionsPage _optionsPage;
		private ISettingsWatcher _watcher;
		private static IContainer Container { get; set; }

		private string Settings
		{
			get
			{
				if (String.IsNullOrEmpty(_optionsPage.Settings))
				{
					//LogMessage(string.Format("Invalid Directory configured for persisting settings. Defaulting to {0}", UserLocalDataPath));
				}
				return _optionsPage.Settings;
			}
		}

		#region Package Members

		protected override void Initialize()
		{
			base.Initialize();
			InitializeContainer();
			var shellService = GetService(typeof(SVsShell)) as IVsShell;
			if (shellService != null)
			{
				ErrorHandler.ThrowOnFailure(shellService.AdviseShellPropertyChanges(this, out _cookie));
			}
		}

		#endregion

		public int OnShellPropertyChange(int propid, object var)
		{
			if ((int)__VSSPROPID.VSSPROPID_Zombie == propid)
			{
				if ((bool)var == false)
				{
					//Visual Studio is now ready and loaded up
					var shellService = GetService(typeof(SVsShell)) as IVsShell;
					if (shellService != null)
						ErrorHandler.ThrowOnFailure(shellService.UnadviseShellPropertyChanges(_cookie));
					_cookie = 0;

					_optionsPage = (OptionsPage)GetDialogPage(typeof(OptionsPage));
					_optionsPage.SettingsUpdated += OptionsPageSettingsUpdated;

					SetupWatcher(); 
					Synchronize();
				}
			}
			return VSConstants.S_OK;
		}

		void OptionsPageSettingsUpdated(string settings)
		{
			SetupWatcher();
			Synchronize();
		}

		void SetupWatcher()
		{
			//if (_watcher != null)
			//{
			//	return;
			//}
			//_watcher = Container.Resolve<ISettingsWatcher>();
			//_watcher.Changed += OnWatcherOnChanged;
		}

		private void OnWatcherOnChanged(object sender, EventArgs e)
		{
			Synchronize();
		}

		void Synchronize()
		{
			lock (this)
			{
				var path = Path.Combine(UserLocalDataPath, "vs_sync_test.txt");
				using (var stream = new StreamWriter(path))
				{
					var repository = Container.Resolve<ISettingsRepository>();
					stream.Write(repository.GetSettins());
				}
			}
		}

		private static void InitializeContainer()
		{
			var builder = new ContainerBuilder();
			builder.RegisterType<SettingsRepository>()
				.As<ISettingsRepository>();
			builder.RegisterType<SettingsWatcher>()
				.As<ISettingsWatcher>();
			Container = builder.Build();
		}
	}
}
