using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Live;
using VisualStudioSync.Models;

namespace VisualStudioSync.Live
{
	public class LiveController : IRefreshTokenHandler
	{
		private const string ClientId = "00000000481024B2";
		private const string FolderName = "VS Sync";
		private const string FileName = "vs_sync.settings";
		private const string SkyDrivePath = "me/skydrive";
		private const string FilesPath = "{0}/files";
		private LiveAuthForm _authForm;
		private LiveAuthClient _liveAuthClient;
		private LiveConnectClient _liveConnectClient;
		private RefreshTokenInfo _refreshTokenInfo;

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
				_liveAuthClient = value;
				_liveConnectClient = null;
			}
		}

		private LiveConnectSession AuthSession
		{
			get
			{
				return AuthClient.Session;
			}
		}

		public LiveController()
		{
			InitLive();
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

		public Blob GetBlob()
		{
			if (AuthSession == null)
			{
				SignIn();
			}
			return ReadFile();
		}

		public void SaveBlob(string value)
		{
			if (AuthSession == null)
			{
				SignIn();
			}
			var g = WriteFile(value).Result;
		}

		private void SignIn()
		{
			if (_authForm == null)
			{
				var startUrl = AuthClient.GetLoginUrl(new List<string> { "wl.signin", "wl.skydrive", "wl.skydrive_update", "wl.offline_access" });
				const string endUrl = "https://login.live.com/oauth20_desktop.srf";
				_authForm = new LiveAuthForm(
					startUrl,
					endUrl,
					OnAuthCompleted);
				_authForm.FormClosed += AuthForm_FormClosed;
				_authForm.ShowDialog();
			}
		}

		private void OnAuthCompleted(AuthResult result)
		{
			CleanupAuthForm();
			if (result.AuthorizeCode != null)
			{
				try
				{
					var session = AuthClient.ExchangeAuthCodeAsync(result.AuthorizeCode);
					_liveConnectClient = new LiveConnectClient(session.Result);
				}
				catch (LiveAuthException aex)
				{
					//this.LogOutput("Failed to retrieve access token. Error: " + aex.Message);
				}
				catch (LiveConnectException cex)
				{
					//this.LogOutput("Failed to retrieve the user's data. Error: " + cex.Message);
				}
			}
			else
			{
				//this.LogOutput(string.Format("Error received. Error: {0} Detail: {1}", result.ErrorCode, result.ErrorDescription));
			}
		}

		private Blob ReadFile()
		{
			var folderId = GetSyncFolder();
			if (string.IsNullOrEmpty(folderId))
			{
				throw new NullReferenceException();
			}

			var path = string.Format(FilesPath, folderId);
			var result = _liveConnectClient.GetAsync(path).Result;
			var files = result.Result["data"] as List<object>;
			var file = files == null
				? null
				: files
					.Select(item => item as IDictionary<string, object>)
					.FirstOrDefault(f => f["name"].ToString() == FileName);

			if (file == null)
			{
				return null;
			}

			var id = file["upload_location"].ToString();
			var fileAsync = _liveConnectClient.DownloadAsync(id).Result;
			string value;
			using (var reader = new StreamReader(fileAsync.Stream))
			{
				value = reader.ReadToEnd();
			}

			return new Blob(value, DateTime.Parse(file["updated_time"].ToString()));
		}

		private Task<LiveOperationResult> WriteFile(string value)
		{
			var folderId = GetSyncFolder();
			if (string.IsNullOrEmpty(folderId))
			{
				throw new NullReferenceException();
			}

			return _liveConnectClient.UploadAsync(folderId, FileName,
				new MemoryStream(System.Text.Encoding.UTF8.GetBytes(value)),
				OverwriteOption.Overwrite);
		}

		private string GetSyncFolder()
		{
			string folderId = null;
			var result = _liveConnectClient.GetAsync(string.Format(FilesPath, SkyDrivePath)).Result;
			if (result != null)
			{
				var items = result.Result["data"] as List<object>;
				folderId = items == null
					? null
					: items.Select(item => item as IDictionary<string, object>)
						.Where(file => file["name"].ToString() == FolderName)
						.Select(file => file["id"].ToString())
						.FirstOrDefault();

				if (String.IsNullOrEmpty(folderId))
				{
					var folderData = new Dictionary<string, object> { { "name", FolderName } };
					result = _liveConnectClient.PostAsync(SkyDrivePath, folderData).Result;
					dynamic res = result.Result;
					folderId = res.id;
				}
			}
			return folderId;
		}
	}
}
