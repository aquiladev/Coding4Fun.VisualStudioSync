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

		public string GetFile()
		{
			if (AuthSession == null)
			{
				SignIn();
			}
			return ReadFile();
		}

		public void SaveFile(string value)
		{
			if (AuthSession == null)
			{
				SignIn();
			}
			WriteFile(value);
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

		/// <summary>
		/// TODO: decide long async\await method
		/// </summary>
		private string ReadFile()
		{
			var folderId = GetSyncFolder();
			if (string.IsNullOrEmpty(folderId))
			{
				throw new NullReferenceException();
			}

			var path = string.Format("{0}/files", folderId);
			var result = _liveConnectClient.GetAsync(path).Result;
			var files = result.Result["data"] as List<object>;
			var fileId = files == null
				? null
				: files.Select(item => item as IDictionary<string, object>)
					.Where(file => file["name"].ToString() == FileName)
					.Select(file => file["id"].ToString())
					.FirstOrDefault();

			if (!String.IsNullOrEmpty(fileId))
			{
				var file = _liveConnectClient.DownloadAsync(fileId).Result;
				var reader = new StreamReader(file.Stream);
				return reader.ReadToEnd();
			}
			return null;
		}

		private void WriteFile(string value)
		{
			var folderId = GetSyncFolder();
			if (string.IsNullOrEmpty(folderId))
			{
				throw new NullReferenceException();
			}

			using (var stream = new MemoryStream())
			{
				using (var writer = new StreamWriter(stream))
				{
					writer.Write(value);
					writer.Flush();
					stream.Position = 0;
					_liveConnectClient.UploadAsync(folderId, FileName, stream, OverwriteOption.Overwrite);
				}
			}
		}

		private string GetSyncFolder()
		{
			string folderId = null;
			var result = _liveConnectClient.GetAsync("me/skydrive/files").Result;
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
					result = _liveConnectClient.PostAsync("me/skydrive", folderData).Result;
					dynamic res = result.Result;
					folderId = res.id;
				}
			}
			return folderId;
		}
	}
}
