using SkiaSharp;

namespace BoxCodeApp;

public partial class MainPage : ContentPage
{
	int count = 0;
    private byte[] data;
    private Stream GetDataStream()
    {
        return new MemoryStream(data);
    }
	public MainPage()
	{
		InitializeComponent();
        
	}
    private void Refresh()
    {
        Busy.IsRunning = Busy.IsVisible = true;
        Task.Run(RetrieveImage);
    }
    private void ValidateImage(SKBitmap bitmap)
    {
        var _ = BoxCodeLib.LibraryUtil.ConvertFromBitmap(bitmap);
    }
    public async Task RetrieveImage()
    {
        await Task.Yield();
        var app = (App.Current as App);
        if (app != null)
        {
            
            data = app.LoadedImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).ToArray();
            var source = ImageSource.FromStream(GetDataStream);
            App.Current.Dispatcher.Dispatch(() =>
            {
                LoadedImage.Source = source;
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
                var fsr = await global::CommunityToolkit.Maui.Storage.FileSaver.Default.SaveAsync("message.png", app.LoadedImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100).AsStream());
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
                        ValidateImage(bitmap);
                        app.LoadedImage = SKBitmap.Decode(flr.FullPath);
                        app.Dispatcher.Dispatch(() =>
                        {
                            Refresh();
                        });
                    }
                    catch (Exception ex)
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
        App.Current.Dispatcher.Dispatch(Reset);
    }
    private void ClearImage_Clicked(object sender, EventArgs e)
    {
        Reset();
    }
}

