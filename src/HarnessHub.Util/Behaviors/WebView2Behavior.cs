using System.Text.Json;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace HarnessHub.Util.Behaviors;

/// <summary>
/// WebView2 컨트롤과 ViewModel 간 통신을 중계하는 Attached Behavior.
/// View가 ViewModel을 직접 참조하지 않고도 마크다운 로드/저장을 수행한다.
/// </summary>
public static class WebView2Behavior
{
    /// <summary>
    /// WebView2 초기화 완료 여부 (per-instance).
    /// </summary>
    private static readonly DependencyProperty IsInitializedProperty =
        DependencyProperty.RegisterAttached(
            "IsInitialized",
            typeof(bool),
            typeof(WebView2Behavior),
            new PropertyMetadata(false));

    /// <summary>
    /// WebMessageReceived 핸들러 참조 (구독 해제용).
    /// </summary>
    private static readonly DependencyProperty WebMessageHandlerProperty =
        DependencyProperty.RegisterAttached(
            "WebMessageHandler",
            typeof(EventHandler<CoreWebView2WebMessageReceivedEventArgs>),
            typeof(WebView2Behavior),
            new PropertyMetadata(null));

    private static bool GetIsInitialized(DependencyObject obj) => (bool)obj.GetValue(IsInitializedProperty);
    private static void SetIsInitialized(DependencyObject obj, bool value) => obj.SetValue(IsInitializedProperty, value);

    private static EventHandler<CoreWebView2WebMessageReceivedEventArgs>? GetWebMessageHandler(DependencyObject obj)
        => (EventHandler<CoreWebView2WebMessageReceivedEventArgs>?)obj.GetValue(WebMessageHandlerProperty);
    private static void SetWebMessageHandler(DependencyObject obj, EventHandler<CoreWebView2WebMessageReceivedEventArgs>? value)
        => obj.SetValue(WebMessageHandlerProperty, value);

    /// <summary>
    /// WebView2에 로드할 로컬 HTML 폴더 경로.
    /// </summary>
    public static readonly DependencyProperty WebViewFolderProperty =
        DependencyProperty.RegisterAttached(
            "WebViewFolder",
            typeof(string),
            typeof(WebView2Behavior),
            new PropertyMetadata(null, OnWebViewFolderChanged));

    /// <summary>
    /// ViewModel에서 에디터로 전달할 마크다운 콘텐츠.
    /// </summary>
    public static readonly DependencyProperty MarkdownToLoadProperty =
        DependencyProperty.RegisterAttached(
            "MarkdownToLoad",
            typeof(string),
            typeof(WebView2Behavior),
            new PropertyMetadata(null, OnMarkdownToLoadChanged));

    /// <summary>
    /// 에디터에서 편집된 마크다운 콘텐츠를 ViewModel로 전달.
    /// </summary>
    public static readonly DependencyProperty EditedMarkdownProperty =
        DependencyProperty.RegisterAttached(
            "EditedMarkdown",
            typeof(string),
            typeof(WebView2Behavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// 에디터 줄 수를 ViewModel로 전달.
    /// </summary>
    public static readonly DependencyProperty LineCountProperty =
        DependencyProperty.RegisterAttached(
            "LineCount",
            typeof(int),
            typeof(WebView2Behavior),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// 저장 요청 트리거. true로 설정하면 에디터에 저장 메시지를 보낸다.
    /// </summary>
    public static readonly DependencyProperty RequestSaveProperty =
        DependencyProperty.RegisterAttached(
            "RequestSave",
            typeof(bool),
            typeof(WebView2Behavior),
            new PropertyMetadata(false, OnRequestSaveChanged));

    /// <summary>
    /// 에디터 테마 (dark / light).
    /// </summary>
    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.RegisterAttached(
            "Theme",
            typeof(string),
            typeof(WebView2Behavior),
            new PropertyMetadata("default", OnThemeChanged));

    public static string GetWebViewFolder(DependencyObject obj) => (string)obj.GetValue(WebViewFolderProperty);
    public static void SetWebViewFolder(DependencyObject obj, string value) => obj.SetValue(WebViewFolderProperty, value);

    public static string GetMarkdownToLoad(DependencyObject obj) => (string)obj.GetValue(MarkdownToLoadProperty);
    public static void SetMarkdownToLoad(DependencyObject obj, string value) => obj.SetValue(MarkdownToLoadProperty, value);

    public static string GetEditedMarkdown(DependencyObject obj) => (string)obj.GetValue(EditedMarkdownProperty);
    public static void SetEditedMarkdown(DependencyObject obj, string value) => obj.SetValue(EditedMarkdownProperty, value);

    public static int GetLineCount(DependencyObject obj) => (int)obj.GetValue(LineCountProperty);
    public static void SetLineCount(DependencyObject obj, int value) => obj.SetValue(LineCountProperty, value);

    public static bool GetRequestSave(DependencyObject obj) => (bool)obj.GetValue(RequestSaveProperty);
    public static void SetRequestSave(DependencyObject obj, bool value) => obj.SetValue(RequestSaveProperty, value);

    public static string GetTheme(DependencyObject obj) => (string)obj.GetValue(ThemeProperty);
    public static void SetTheme(DependencyObject obj, string value) => obj.SetValue(ThemeProperty, value);

    private static async void OnWebViewFolderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not WebView2 webView || e.NewValue is not string folder)
            return;

        await webView.EnsureCoreWebView2Async();

        webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "editor.local",
            folder,
            CoreWebView2HostResourceAccessKind.Allow);

        // 기존 핸들러 해제 (재진입 방지)
        var existingHandler = GetWebMessageHandler(webView);
        if (existingHandler is not null)
            webView.CoreWebView2.WebMessageReceived -= existingHandler;

        EventHandler<CoreWebView2WebMessageReceivedEventArgs> handler =
            (s, args) => OnWebMessageReceived(webView, args);
        SetWebMessageHandler(webView, handler);
        webView.CoreWebView2.WebMessageReceived += handler;

        webView.Unloaded += OnWebViewUnloaded;

        webView.CoreWebView2.Navigate("http://editor.local/index.html");
        SetIsInitialized(webView, true);
    }

    private static void OnWebViewUnloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not WebView2 webView)
            return;

        webView.Unloaded -= OnWebViewUnloaded;

        if (webView.CoreWebView2 is not null)
        {
            var handler = GetWebMessageHandler(webView);
            if (handler is not null)
            {
                webView.CoreWebView2.WebMessageReceived -= handler;
                SetWebMessageHandler(webView, null);
            }
        }

        SetIsInitialized(webView, false);
    }

    private static void OnWebMessageReceived(WebView2 webView, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var json = e.WebMessageAsJson;
            using var doc = JsonDocument.Parse(json);
            var action = doc.RootElement.GetProperty("action").GetString();

            switch (action)
            {
                case "contentChanged":
                    if (doc.RootElement.TryGetProperty("lineCount", out var lineCountProp))
                    {
                        SetLineCount(webView, lineCountProp.GetInt32());
                    }
                    break;

                case "returnContent":
                    if (doc.RootElement.TryGetProperty("content", out var contentProp))
                    {
                        var content = contentProp.GetString();
                        if (content is not null)
                        {
                            SetEditedMarkdown(webView, content);
                        }
                    }
                    SetRequestSave(webView, false);
                    break;

                case "ready":
                    var pendingMarkdown = GetMarkdownToLoad(webView);
                    if (!string.IsNullOrEmpty(pendingMarkdown))
                    {
                        SendLoadMessage(webView, pendingMarkdown);
                    }
                    var theme = GetTheme(webView);
                    if (theme != "default")
                    {
                        SendThemeMessage(webView, theme);
                    }
                    break;
            }
        }
        catch
        {
            // 잘못된 메시지는 무시
        }
    }

    private static void OnMarkdownToLoadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not WebView2 webView || e.NewValue is not string markdown)
            return;

        if (GetIsInitialized(webView) && webView.CoreWebView2 is not null)
        {
            SendLoadMessage(webView, markdown);
        }
    }

    private static void OnRequestSaveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not WebView2 webView || e.NewValue is not true)
            return;

        if (GetIsInitialized(webView) && webView.CoreWebView2 is not null)
        {
            var msg = JsonSerializer.Serialize(new { action = "save" });
            webView.CoreWebView2.PostWebMessageAsJson(msg);
        }
    }

    private static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not WebView2 webView || e.NewValue is not string theme)
            return;

        if (GetIsInitialized(webView) && webView.CoreWebView2 is not null)
        {
            SendThemeMessage(webView, theme);
        }
    }

    private static void SendLoadMessage(WebView2 webView, string markdown)
    {
        var msg = JsonSerializer.Serialize(new { action = "load", content = markdown });
        webView.CoreWebView2.PostWebMessageAsJson(msg);
    }

    private static void SendThemeMessage(WebView2 webView, string theme)
    {
        webView.CoreWebView2.Profile.PreferredColorScheme = theme == "dark"
            ? CoreWebView2PreferredColorScheme.Dark
            : CoreWebView2PreferredColorScheme.Light;

        var msg = JsonSerializer.Serialize(new { action = "setTheme", theme });
        webView.CoreWebView2.PostWebMessageAsJson(msg);
    }
}
