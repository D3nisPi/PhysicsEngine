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
        private List<Model> _models = new List<Model>();

        private float _zoomSensitivity = 1f;
        private float _mouseSensitivity = 0.015f;

        Shader colorShader;
        Shader texShader;
        Camera camera;

        private bool firstMove = true; // Flag for first frame
        private Vector2 lastPosition;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            VSync = VSyncMode.On;
        }

        protected override void OnLoad()
        {
            GL.ClearColor(1f, 1f, 1f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
            GL.PolygonMode(MaterialFace.Back, PolygonMode.Line);
            //CursorState = CursorState.Grabbed;

            camera = new Camera(new Vector3(10, 10, 10), Vector3.Zero, Size.X / (float)Size.Y, 100, 0.3f);


            string vertPath = $@"Shaders\Data\colorShader.vert";
            string fragPath = $@"Shaders\Data\colorShader.frag";
            colorShader = new Shader(vertPath, fragPath);

            vertPath = $@"Shaders\Data\texShader.vert";
            fragPath = $@"Shaders\Data\texShader.frag";
            texShader = new Shader(vertPath, fragPath);


            string path = @$"Models\Data\Aether\Aether.obj";

            Model model = Model.ParseOBJ(path, colorShader, texShader, new Vector4(0, 1, 0, 1));
            //model.RotationPerSecond = new RotationAngles(MathHelper.PiOver3, MathHelper.PiOver2, MathHelper.Pi);
            _models.Add(model);
            model.Move(0, -10, 0);

            base.OnLoad();
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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


            var mouse = MouseState;

            if (firstMove)
            {
                lastPosition = new Vector2(mouse.X, mouse.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - lastPosition.X;
                var deltaY = mouse.Y - lastPosition.Y;
                lastPosition = new Vector2(mouse.X, mouse.Y);

                camera.Phi += deltaX * _mouseSensitivity;
                camera.Theta -= deltaY * _mouseSensitivity;
            }
        }
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            camera.R -= e.OffsetY * _zoomSensitivity;
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            camera.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}