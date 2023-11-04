using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Xml.Linq;
using OpenGL.Windows;

namespace Program
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Profile = ContextProfile.Compatability,
                Size = new Vector2i(900, 900),
            };
            using (var testWindow = new Window(GameWindowSettings.Default, nativeWindowSettings))
            {
                testWindow.Run();
            }
        }
    }
}