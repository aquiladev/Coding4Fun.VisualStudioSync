using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Live;

namespace VisualStudioSync.Live
{
	public class LiveController : IRefreshTokenHandler
	{
		private const string ClientId = "00000000481024B2";
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
			if (_authForm != null)
			{
				_authForm.Dispose();
				_authForm = null;
			}
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
			DoCommand(DoIt);
			return "qw";
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

		private async void DoIt(AuthResult result)
		{
			CleanupAuthForm();
			if (result.AuthorizeCode != null)
			{
				var session = await AuthClient.ExchangeAuthCodeAsync(result.AuthorizeCode);
				_liveConnectClient = new LiveConnectClient(session);

				var operationResult = await _liveConnectClient.GetAsync("me/skydrive/files");
				dynamic re = operationResult.Result;

				//var items = re.Result["data"] as List<object>;
				//foreach (object item in items)
				//{
				//	var file = item as IDictionary<string, object>;
				//	if (file["name"].ToString() == "somefile.txt")
				//	{
				//		var id = file["id"].ToString();
				//	}
				//}
			}
		}
	}
}
