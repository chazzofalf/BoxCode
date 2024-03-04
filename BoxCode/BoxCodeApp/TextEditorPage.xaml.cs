

using static System.Net.Mime.MediaTypeNames;

namespace BoxCodeApp;

public partial class TextEditorPage : ContentPage
{
	public TextEditorPage()
	{
		InitializeComponent();
	}
    private void Refresh()
    {
        Busy.IsRunning = Busy.IsVisible = true;
        Task.Run(RetrieveText);
    }
    public async Task RetrieveText()
    {
        await Task.Yield();
        var app = (App.Current as App);
        if (app != null)
        {
            var text = BoxCodeLib.LibraryUtil.ConvertFromBitmap(app.LoadedImage);
            
            App.Current.Dispatcher.Dispatch(() =>
            {
                BoxCodeText.Text = text;
                Busy.IsRunning = Busy.IsVisible = false;
            });
        }
    }
    public async Task PushText()
    {
        await Task.Yield();
        var app = (App.Current as App);
        if (app != null)
        {
            app.LoadedImage = BoxCodeLib.LibraryUtil.ConvertToBitmap(BoxCodeText.Text,singleLine:false);
            App.Current.Dispatcher.Dispatch(() =>
            {
                Navigation.PopAsync();
            });
        }
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        Refresh();
    }
    private void SaveText_Clicked(object sender, EventArgs e)
    {

    }

    private void LoadText_Clicked(object sender, EventArgs e)
    {

    }

    private void SaveToImage_Clicked(object sender, EventArgs e)
    {
        Task.Run(PushText);
    }

    private void Cancel_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}