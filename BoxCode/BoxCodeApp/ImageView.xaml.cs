

using SkiaSharp;

namespace BoxCodeApp;

public partial class ImageView : ContentView
{
	public ImageView()
	{
		InitializeComponent();
        Refresh();
	}
    
    
    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        Refresh();
    }
    public void Refresh()
    {
        Task.Run(async () =>
        {
            await Task.Yield();
            while (Viewer == null || Viewer.Width <= 0 && Viewer.Height <= 0)
            {
                await Task.Delay(100);
            }
            
            Application.Current?.Dispatcher.Dispatch(() =>
            {
                _Refresh();
            });
                
            
        });
    }

    private void _Refresh(Image? child=null)
    {
        child = child ?? Viewer;
        if (Application.Current is App app)
        {
            var dirtyRect = new SKRect(0, 0, (float)child.Width, (float)child.Height);
            var bitmap = new SKBitmap((int)dirtyRect.Width, (int)dirtyRect.Height);
            var canvas = new SKCanvas(bitmap);
            var black = new SKPaint()
            {
                Color = SKColors.Black
            };
            canvas.DrawRect(new SKRect(0, 0, dirtyRect.Width, dirtyRect.Height), black);

            (var w, var h) = (dirtyRect.Width, dirtyRect.Height);
            var image = app.LoadedImage;
            (var iw, var ih) = (image.Width, image.Height);
            (var xratio, var yratio) = (iw / w, ih / h);
            var ratio = xratio > yratio ? xratio : yratio;
            var iratio = 1 / ratio;
            (var iaw, var iah) = (iw * iratio, ih * iratio);
            (var ix, var iy) = (w / 2 - iaw / 2, h / 2 - iah / 2);
            canvas.DrawBitmap(image, new SKRect(0, 0, image.Width, image.Height), new SKRect(ix, iy, ix+iaw, iy+iah));
            canvas.Flush();
            Viewer.Source = ImageSource.FromStream(() =>
            
                bitmap.Encode(SKEncodedImageFormat.Png, 100).AsStream()
            );
        }
        
    }
}