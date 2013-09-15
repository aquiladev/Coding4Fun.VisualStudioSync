using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Live;

namespace VisualStudioSync.Live
{
	public class LiveController : IRefreshTokenHandler
	{
		private const string ClientId = "00000000481024B2";
		private const string FolderName = "VS Sync";
		private const string FileName = "vs_sync.settings";
		private LiveAuthForm _authForm;
		private LiveAuthClient _liveAuthClient;
		private LiveConnectClient _liveConnectClient;
		private RefreshTokenInfo _refreshTokenInfo;
		private string _value;

		private LiveAuthClient AuthClient
		{
			get
			{
				if (_liveAuthClient == null)
				{
					AuthClient = new LiveAuthClient(ClientId, this);
				}
				return _liveAuthClient;
			}
			set
			{
				if (_liveAuthClient != null)
				{
					_liveAuthClient.PropertyChanged -= liveAuthClient_PropertyChanged;
				}
				_liveAuthClient = value;
				if (_liveAuthClient != null)
				{
					_liveAuthClient.PropertyChanged += liveAuthClient_PropertyChanged;
				}
				_liveConnectClient = null;
			}
		}

		public LiveController()
		{
			InitLive();
		}

		private void liveAuthClient_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Session")
			{
			}
		}

		private async void InitLive()
		{
			var loginResult = await AuthClient.IntializeAsync();
			if (loginResult.Session != null)
			{
				_liveConnectClient = new LiveConnectClient(loginResult.Session);
			}
		}

		private void AuthForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			CleanupAuthForm();
		}

		private void CleanupAuthForm()
		{
			if (_authForm == null)
			{
				return;
			}
			_authForm.Dispose();
			_authForm = null;
		}

		public Task SaveRefreshTokenAsync(RefreshTokenInfo tokenInfo)
		{
			return Task.Factory.StartNew(() =>
			{
				_refreshTokenInfo = tokenInfo;
			});
		}

		public Task<RefreshTokenInfo> RetrieveRefreshTokenAsync()
		{
			return Task.Factory.StartNew(() => _refreshTokenInfo);
		}

		public string GetFile()
		{
			DoCommand(DoGetFile);
			return _value;
		}

		private void DoCommand(AuthCompletedCallback action)
		{
			if (_authForm == null && _liveConnectClient == null)
			{
				var startUrl = AuthClient.GetLoginUrl(new List<string> { "wl.signin", "wl.skydrive", "wl.skydrive_update" });
				const string endUrl = "https://login.live.com/oauth20_desktop.srf";
				_authForm = new LiveAuthForm(
					startUrl,
					endUrl,
					action);
				_authForm.FormClosed += AuthForm_FormClosed;
				_authForm.ShowDialog();
			}
		}

		/// <summary>
		/// TODO: decide long async\await method
		/// </summary>
		/// <param name="result"></param>
		private async void DoGetFile(AuthResult result)
		{
			CleanupAuthForm();
			if (result.AuthorizeCode != null)
			{
				var session = await AuthClient.ExchangeAuthCodeAsync(result.AuthorizeCode);
				_liveConnectClient = new LiveConnectClient(session);

				var operationResult = await _liveConnectClient.GetAsync("me/skydrive/files");
				var items = operationResult.Result["data"] as List<object>;
				var folderId = items == null
					? null
					: items.Select(item => item as IDictionary<string, object>)
						.Where(file => file["name"].ToString() == FolderName)
						.Select(file => file["id"].ToString())
						.FirstOrDefault();

				if (String.IsNullOrEmpty(folderId))
				{
					var folderData = new Dictionary<string, object> { { "name", FolderName } };
					operationResult = await _liveConnectClient.PostAsync("me/skydrive", folderData);
					dynamic res = operationResult.Result;
					folderId = res.id;
				}

				var path = string.Format("{0}/files", folderId);
				operationResult = await _liveConnectClient.GetAsync(path);
				var files = operationResult.Result["data"] as List<object>;
				var fileId = files == null
					? null
					: files.Select(item => item as IDictionary<string, object>)
						.Where(file => file["name"].ToString() == FileName)
						.Select(file => file["id"].ToString())
						.FirstOrDefault();

				if (!String.IsNullOrEmpty(fileId))
				{
					var file = await _liveConnectClient.DownloadAsync(fileId);
					var reader = new StreamReader(file.Stream);
					_value = reader.ReadToEnd();
				}
			}
		}
	}
}
