using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenGL.Cameras;
using OpenGL.Shaders;
using OpenGL.Models;

namespace OpenGL.Windows
{
    public class Window : GameWindow
    {
        private List<Model3D> _models = new List<Model3D>();

        private float _zoomSensitivity = 1;

        Shader colorShader;
        Shader texShader;
        Camera camera;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {

        }

        protected override void OnLoad()
        {
            GL.ClearColor(1f, 1f, 1f, 1f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            camera = new Camera(new Vector3(5, 5, 5), Vector3.Zero, Size.X / (float)Size.Y, 100, 0.3f);


            string vertPath = $@"{Directory.GetCurrentDirectory()}\Shaders\Data\colorShader.vert";
            string fragPath = $@"{Directory.GetCurrentDirectory()}\Shaders\Data\colorShader.frag";
            colorShader = new Shader(vertPath, fragPath);

            vertPath = $@"{Directory.GetCurrentDirectory()}\Shaders\Data\texShader.vert";
            fragPath = $@"{Directory.GetCurrentDirectory()}\Shaders\Data\texShader.frag";
            texShader = new Shader(vertPath, fragPath);

            string imgPath = @"C:\Users\d3nis\OneDrive\Рабочий стол\container.jpg";
            string cubePath = @$"{Directory.GetCurrentDirectory()}\Models\Data\cube.obj";
            Model3D cube = Model3D.ParseOBJ(cubePath, imgPath, colorShader, texShader, new Vector4(0, 1, 0, 1));
            cube.RotationPerSecond = new RotationAngles(MathHelper.PiOver3, MathHelper.PiOver2, MathHelper.Pi);
            _models.Add(cube);

            base.OnLoad();
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            //shader.ActivateProgram();

            foreach (var model in _models)
            {
                model.Update((float)args.Time);
                model.Draw();
            }


            Matrix4 modelMatrix = Matrix4.Identity;
            Matrix4 viewMatrix = camera.GetViewMatrix();
            Matrix4 projectionMatrix = camera.GetProjectionMatrix();

            colorShader.SetMatrixUniform("model", ref modelMatrix);
            colorShader.SetMatrixUniform("view", ref viewMatrix);
            colorShader.SetMatrixUniform("projection", ref projectionMatrix);

            texShader.SetMatrixUniform("model", ref modelMatrix);
            texShader.SetMatrixUniform("view", ref viewMatrix);
            texShader.SetMatrixUniform("projection", ref projectionMatrix);

            SwapBuffers();
        }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            camera.R -= e.OffsetY * _zoomSensitivity;
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
        }
    }
}