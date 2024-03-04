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
    private void Button_Clicked(object sender, EventArgs e)
    {

    }

    private void EditText_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new TextEditorPage());
    }

    private void SaveImage_Clicked(object sender, EventArgs e)
    {

    }

    private void LoadImage_Clicked(object sender, EventArgs e)
    {

    }

    private void ClearImage_Clicked(object sender, EventArgs e)
    {
        var app = (App.Current as App);
        if (app != null)
        {
            app.ResetImage();
            Refresh();
        }
    }
}

