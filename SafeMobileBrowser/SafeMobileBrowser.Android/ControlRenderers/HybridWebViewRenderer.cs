﻿using Android.Content;
using Android.Views;
using Plugin.CurrentActivity;
using SafeMobileBrowser.Controls;
using SafeMobileBrowser.Droid.ControlRenderers;
using SafeMobileBrowser.Droid.MediaDownload;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using AWebkit = Android.Webkit;

[assembly: ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]

namespace SafeMobileBrowser.Droid.ControlRenderers
{
    public class HybridWebViewRenderer : WebViewRenderer
    {
        private HybridWebViewClient _webViewClient;
        private HybridWebViewChromeClient _webViewChromeClient;

        public HybridWebViewRenderer(Context context)
            : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<WebView> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                _webViewClient = GetHybridWebViewClient();
                Control.SetWebViewClient(_webViewClient);
                _webViewChromeClient = GetHybridWebViewChromeClient();
                Control.SetWebChromeClient(_webViewChromeClient);
                Control.LoadUrl($"{AssetBaseUrl}index.html");
                CrossCurrentActivity.Current.Activity.RegisterForContextMenu(Control);
            }
        }

        protected override void OnCreateContextMenu(IContextMenu menu)
        {
            base.OnCreateContextMenu(menu);

            var webViewHitTestResult = Control.GetHitTestResult().Type;

            if (webViewHitTestResult == AWebkit.HitTestResult.ImageType ||
                webViewHitTestResult == AWebkit.HitTestResult.SrcAnchorType)
            {
                menu.SetHeaderTitle("Download Image");
                menu.Add(0, 1, 0, "click to download")
                    .SetOnMenuItemClickListener(new ImageDownloadMenuItemListener(Control.GetHitTestResult().Extra));
            }
        }

        private HybridWebViewClient GetHybridWebViewClient()
        {
            return new HybridWebViewClient(this);
        }

        private HybridWebViewChromeClient GetHybridWebViewChromeClient()
        {
            return new HybridWebViewChromeClient(this);
        }

        protected override void Dispose(bool disposing)
        {
            _webViewClient?.Dispose();
            _webViewChromeClient?.Dispose();
            base.Dispose(disposing);
        }
    }
}
