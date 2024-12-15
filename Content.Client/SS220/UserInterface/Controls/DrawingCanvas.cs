using OpenToolkit.GraphicsLibraryFramework;
using Robust.Client.UserInterface;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Content.Client.SS220.UserInterface.Controls;

public sealed class DrawingCanvas : Control
{
    public DrawingCanvas()
    {
        var image = new Image<Rgba32>(200, 200)ж

        float x = 10, y = 10, w = 100, h = 150;

        PointF[] ptCorner = new PointF[4];
        ptCorner[0] = new PointF(x, y);         // Top Left
        ptCorner[1] = new PointF(x + w, y);     // Top Right
        ptCorner[2] = new PointF(x + w, y + h); // Right Bottom
        ptCorner[3] = new PointF(x, y + h);     // Left Bottom

        
    }
}
