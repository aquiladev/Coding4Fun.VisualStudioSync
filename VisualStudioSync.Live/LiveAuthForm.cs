using System;
using System.Windows.Forms;

namespace VisualStudioSync.Live
{
	public delegate void AuthCompletedCallback(AuthResult result);

	public partial class LiveAuthForm : Form
	{
		private readonly string _startUrl;
		private readonly string _endUrl;
		private readonly AuthCompletedCallback _callback;

		public LiveAuthForm(string startUrl, string endUrl, AuthCompletedCallback callback)
		{
			_startUrl = startUrl;
			_endUrl = endUrl;
			_callback = callback;
			InitializeComponent();
		}

		private void LiveAuthForm_Load(object sender, EventArgs e)
		{
			webBrowser.Navigated += WebBrowser_Navigated;
			webBrowser.Navigate(_startUrl);
		}

		private void WebBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
		{
			if (webBrowser.Url.AbsoluteUri.StartsWith(_endUrl))
			{
				if (_callback != null)
				{
					_callback(new AuthResult(this.webBrowser.Url));
				}
			}
		}
	}
}
