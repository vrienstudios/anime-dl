using System;
using System.IO;

namespace ADLCore.Epub
{
    public class Image
    {
        public string Name;
        // Location is set when exporting to epub
        public string location;
        public Byte[] bytes;

        public static Image LoadImageFromFile(string name, string location)
            => new Image { Name = name, bytes = File.ReadAllBytes(location) };

        public static Image GenerateImageFromByte(Byte[] bytes, string name)
            => new Image { Name = name, location = $"../Pictures/{name}", bytes = bytes };

        public override string ToString()
            => $"<div class=\"svg_outer svg_inner\"><svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" height=\"99%\" width=\"100%\" version=\"1.1\" preserveAspectRatio=\"xMidYMid meet\" viewBox=\"0 0 1135 1600\"><image xlink:href=\"{location}.jpeg\" width=\"1135\" height=\"1600\"/></svg></div>";

    }
}
