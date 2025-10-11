using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Client.Views.Sessions.VideoSessionView;

public partial class VideoSurface : UserControl
{
    public VideoSurface()
    {
        InitializeComponent();
    }
    public WriteableBitmap? CurrentFrame { get; set; }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (CurrentFrame != null)
        {
            context.DrawImage(CurrentFrame, new Rect(0, 0, Bounds.Width, Bounds.Height));
        }
    }

    public void UpdateFrame(WriteableBitmap frame)
    {
        CurrentFrame = frame;
        InvalidateVisual();
    }
}