using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using Models;

namespace OpenGL
{
    public class Window : GameWindow
    {
        private List<Model3D> _models = new List<Model3D>();

        private float _zoomSensitivity = 1;

        Shader shader;
        Camera camera;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {

        }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            string vertPath = $@"{Directory.GetCurrentDirectory()}\shader.vert";
            string fragPath = $@"{Directory.GetCurrentDirectory()}\shader.frag";
            shader = new Shader(vertPath, fragPath);

            camera = new Camera(new Vector3(5, 5, 5), Vector3.Zero, Size.X / (float)Size.Y, 100, 0.3f);


            string cubePath = @$"{Directory.GetCurrentDirectory()}\data\cube.obj";
            Model3D cube = Model3D.ParseOBJ(cubePath);
            cube.RotationPerSecond = new RotationAngles(MathHelper.PiOver2, MathHelper.Pi, MathHelper.TwoPi);
            _models.Add(cube);

            base.OnLoad();
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            shader.ActivateProgram();

            foreach(var model in _models)
            {
                model.Update((float)args.Time);
                model.Draw();
            }


            Matrix4 modelMatrix = Matrix4.Identity;
            Matrix4 viewMatrix = camera.GetViewMatrix();
            Matrix4 projectionMatrix = camera.GetProjectionMatrix();

            shader.SetMatrixUniform("model", ref modelMatrix);
            shader.SetMatrixUniform("view", ref viewMatrix);
            shader.SetMatrixUniform("projection", ref projectionMatrix);

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