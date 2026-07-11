using System.Windows;
using System.Windows.Media;

namespace ZMDetection.Controls;

public sealed class DonutChart : FrameworkElement
{
    public static readonly DependencyProperty OkValueProperty =
        DependencyProperty.Register(
            nameof(OkValue),
            typeof(double),
            typeof(DonutChart),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty NgValueProperty =
        DependencyProperty.Register(
            nameof(NgValue),
            typeof(double),
            typeof(DonutChart),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OkBrushProperty =
        DependencyProperty.Register(
            nameof(OkBrush),
            typeof(Brush),
            typeof(DonutChart),
            new FrameworkPropertyMetadata(Brushes.SeaGreen, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty NgBrushProperty =
        DependencyProperty.Register(
            nameof(NgBrush),
            typeof(Brush),
            typeof(DonutChart),
            new FrameworkPropertyMetadata(Brushes.IndianRed, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty EmptyBrushProperty =
        DependencyProperty.Register(
            nameof(EmptyBrush),
            typeof(Brush),
            typeof(DonutChart),
            new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(
            nameof(StrokeThickness),
            typeof(double),
            typeof(DonutChart),
            new FrameworkPropertyMetadata(16d, FrameworkPropertyMetadataOptions.AffectsRender));

    public double OkValue
    {
        get => (double)GetValue(OkValueProperty);
        set => SetValue(OkValueProperty, value);
    }

    public double NgValue
    {
        get => (double)GetValue(NgValueProperty);
        set => SetValue(NgValueProperty, value);
    }

    public Brush OkBrush
    {
        get => (Brush)GetValue(OkBrushProperty);
        set => SetValue(OkBrushProperty, value);
    }

    public Brush NgBrush
    {
        get => (Brush)GetValue(NgBrushProperty);
        set => SetValue(NgBrushProperty, value);
    }

    public Brush EmptyBrush
    {
        get => (Brush)GetValue(EmptyBrushProperty);
        set => SetValue(EmptyBrushProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        double thickness = Math.Max(1, StrokeThickness);
        double radius = Math.Max(0, Math.Min(ActualWidth, ActualHeight) / 2 - thickness / 2);
        var center = new Point(ActualWidth / 2, ActualHeight / 2);

        if (radius <= 0)
        {
            return;
        }

        double ok = Math.Max(0, OkValue);
        double ng = Math.Max(0, NgValue);
        double total = ok + ng;

        if (total <= 0)
        {
            drawingContext.DrawEllipse(
                null,
                CreatePen(EmptyBrush, thickness),
                center,
                radius,
                radius);
            return;
        }

        drawingContext.DrawEllipse(
            null,
            CreatePen(NgBrush, thickness),
            center,
            radius,
            radius);

        double okAngle = 360d * ok / total;
        if (okAngle >= 359.999)
        {
            drawingContext.DrawEllipse(
                null,
                CreatePen(OkBrush, thickness),
                center,
                radius,
                radius);
            return;
        }

        if (okAngle > 0)
        {
            drawingContext.DrawGeometry(
                null,
                CreatePen(OkBrush, thickness),
                CreateArcGeometry(center, radius, -90, okAngle));
        }
    }

    private static Pen CreatePen(Brush brush, double thickness)
    {
        return new Pen(brush, thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };
    }

    private static Geometry CreateArcGeometry(
        Point center,
        double radius,
        double startAngle,
        double sweepAngle)
    {
        Point start = GetPoint(center, radius, startAngle);
        Point end = GetPoint(center, radius, startAngle + sweepAngle);
        var geometry = new StreamGeometry();

        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(start, false, false);
            context.ArcTo(
                end,
                new Size(radius, radius),
                0,
                sweepAngle > 180,
                SweepDirection.Clockwise,
                true,
                false);
        }

        geometry.Freeze();
        return geometry;
    }

    private static Point GetPoint(Point center, double radius, double angle)
    {
        double radians = angle * Math.PI / 180d;
        return new Point(
            center.X + radius * Math.Cos(radians),
            center.Y + radius * Math.Sin(radians));
    }
}
