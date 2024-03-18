
namespace BoxCodeApp;

public partial class GraphicsView : ContentView
{	
	public GraphicsView()
	{
		InitializeComponent();
		Viewer.Drawable = Application.Current is App app ? new GraphicsViewDrawable(app) : null;
	}
	public void Refresh()
	{
		Viewer.Invalidate();
	}
}

internal class GraphicsViewDrawable : IDrawable
{
	private App Application { get; }
	public GraphicsViewDrawable(App application)
    {
        Application = application;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
		canvas.FillColor = Colors.Black;
		canvas.FillRectangle(0, 0, dirtyRect.Width, dirtyRect.Height);
		(var w,var h) = (dirtyRect.Width, dirtyRect.Height);
		var image = Application.LoadedImage.ToMauiImage();
		(var iw, var ih) = (image.Width, image.Height);
		(var xratio,var yratio) = (iw/w,ih/h);
		var ratio = xratio > yratio ? xratio : yratio;
		var iratio = 1 / ratio;
		(var iaw, var iah) = (iw * iratio, ih * iratio);
		(var ix, var iy) = (w / 2 - iaw / 2, h / 2 - iah / 2);
		canvas.DrawImage(image, ix, iy, iaw, iah);

	}
}