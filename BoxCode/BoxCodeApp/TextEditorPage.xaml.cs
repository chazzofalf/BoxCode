

using SkiaSharp;
using System.Text.RegularExpressions;
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

            var text = "";
            try
            {
                text = BoxCodeLib.LibraryUtil.ConvertFromBitmap(app.LoadedImage);
            }
            catch
            {

            }
                
            
            app.Dispatcher.Dispatch(() =>
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
            app.LoadedImage = BoxCodeLib.LibraryUtil.ConvertToBitmap(BoxCodeText.Text,singleLine:!SquaredWord.IsChecked);
            app.Dispatcher.Dispatch(() =>
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
        Task.Run(async () =>
        {
            await Task.Yield();
            var text = BoxCodeText.Text;
            var data = System.Text.Encoding.UTF8.GetBytes(text);
            var stream = new MemoryStream(data);
#pragma warning disable CA1416 // Validate platform compatibility
            var fsr = await global::CommunityToolkit.Maui.Storage.FileSaver.Default.SaveAsync("message.text", stream);
#pragma warning restore CA1416 // Validate platform compatibility
            if (fsr.IsSuccessful)
            {

            }
        });
    }

    private void LoadText_Clicked(object sender, EventArgs e)
    {
        var app = (App.Current as App);
        if (app != null)
        {
            Task.Run(async () =>
            {
                var options = new PickOptions();
                
                var flr = await FilePicker.Default.PickAsync();
                if (flr != null)
                {
                    var text = await File.ReadAllTextAsync(flr.FullPath);
                    app.Dispatcher.Dispatch(() =>
                    {
                        BoxCodeText.Text = text;
                    });
                }
            });
        }
    }

    private void SaveToImage_Clicked(object sender, EventArgs e)
    {
        Task.Run(PushText);
    }

    private void Cancel_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private void BoxCodeText_TextChanged(object sender, TextChangedEventArgs e)
    {
        
    }

    private void BoxCodeText_TextChanged_1(object sender, TextChangedEventArgs e)
    {
        if (Regex.Split(e.NewTextValue, "\r\n|\r|\n").Length > 1)
        {
            SquaredWord.IsChecked = false;
            SquaredWord.IsEnabled = false;
        }
        else
        {
            SquaredWord.IsEnabled = true;
        }
    }
}