namespace BoxCodeApp;
using SkiaSharp;
public partial class App : Application
{
	public SkiaSharp.SKBitmap LoadedImage { get; set; }
	public void ResetImage()
	{
        LoadedImage = SKBitmap.Decode(typeof(App).Assembly.GetManifestResourceStream("DefaultPicture"));
    }
	public App()
	{
		InitializeComponent();
		ResetImage();
		MainPage = new AppShell();
	}
	
}
