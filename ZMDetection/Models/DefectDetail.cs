namespace ZMDetection.Models;

public sealed class DefectDetail
{
    public DefectDetail(string name, string code, int x, int y, int width, int height)
    {
        Name = name;
        Code = code;
        DefectX = x;
        DefectY = y;
        Width = width;
        Height = height;
    }

    public string Name { get; }
    public string Code { get; }
    public int DefectX { get; }
    public int DefectY { get; }
    public int Width { get; }
    public int Height { get; }
    public int Area => Width * Height;
}
