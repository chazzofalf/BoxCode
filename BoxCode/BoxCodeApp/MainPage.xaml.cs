using SkiaSharp;

namespace BoxCodeApp;

public partial class MainPage : ContentPage
{
	// int count = 0;
    private byte[]? data;
    private Stream? GetDataStream()
    {
        if (data != null)
            return new MemoryStream(data);
        return null;
    }
	public MainPage()
	{
		InitializeComponent();
#if WINDOWS
        LoadedImageHolder.Content = new BoxCodeApp.ImageView();
#else
        LoadedImageHolder.Content = new BoxCodeApp.GraphicsView();
#endif
    }
    private void Refresh()
    {
        Busy.IsRunning = Busy.IsVisible = true;
        Task.Run(RetrieveImage);
    }
    private bool ValidateImage(SKBitmap bitmap)
    {
        return BoxCodeLib.LibraryUtil.IsValid(bitmap);
        
    }
    public async Task RetrieveImage()
    {
        await Task.Yield();
        var app = (App.Current as App);
        if (app != null)
        {
            
            
            app.Dispatcher.Dispatch(() =>
            {
#if WINDOWS
                (LoadedImageHolder.Content as ImageView)?.Refresh();
#endif
                // LoadedImage.Source = source;
                Busy.IsRunning = Busy.IsVisible = false;
            });
            
            
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Refresh();
    }
    

    private void EditText_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new TextEditorPage());
    }

    private void SaveImage_Clicked(object sender, EventArgs e)
    {
        var app = (App.Current as App);
        if (app != null)
        {
            Task.Run(async () =>
            {
#pragma warning disable CA1416 // Validate platform compatibility
                var fsr = await global::CommunityToolkit.Maui.Storage.FileSaver.Default.SaveAsync("message.png", app.LoadedImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).AsStream());
#pragma warning restore CA1416 // Validate platform compatibility
                if (fsr.IsSuccessful)
                {

                }
            });
            
            
        }
        
    }

    private void LoadImage_Clicked(object sender, EventArgs e)
    {
        
        var app = (App.Current as App);
        if (app != null)
        {
            Task.Run(async () =>
            {
                var options = new PickOptions();
                options.FileTypes = FilePickerFileType.Png;
                var flr = await FilePicker.Default.PickAsync(options);
                if (flr != null)
                {
                    try
                    {
                        var bitmap = SKBitmap.Decode(flr.FullPath);
                        if (ValidateImage(bitmap))
                        {
                            app.LoadedImage = bitmap;
                            app.Dispatcher.Dispatch(() =>
                            {
                                Refresh();
                            });
                        }
                        else
                        {
                            await CommunityToolkit.Maui.Alerts.Toast.Make("This was an invalid image.").Show();
                        }
                        
                    }
                    catch 
                    {
                        await CommunityToolkit.Maui.Alerts.Toast.Make("This was an invalid image.").Show();
                        
                        
                    }
                    
                }
            });
        }
    }
    private void Reset()
    {
        var app = (App.Current as App);
        if (app != null)
        {
            app.ResetImage();
            Refresh();
        }
    }
    private void ResetThreadSafe()
    {
        if (App.Current is App app)
        {
            app.Dispatcher.Dispatch(Reset);
        }
        
    }
    private void ClearImage_Clicked(object sender, EventArgs e)
    {
        Reset();
    }
}

