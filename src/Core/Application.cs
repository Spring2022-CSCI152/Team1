using System.Numerics;
using Silk.NET;
using Bulldog.Renderer;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Shader = Bulldog.Renderer.Shader;
using System.Drawing;
using Bulldog.Utils;
using Texture = Bulldog.Renderer.Texture;

namespace Bulldog.Core
{
    class Program
    {
        private static IWindow _window;
        private static GL _gl;
        private static IKeyboard primaryKeyboard;
        private const int Width = 800;
        private const int Height = 700;
        private static BufferObject<float> _vbo;
        private static BufferObject<uint> _ebo;
        private static VertexArrayObject<float, uint> _vao;
        private static Texture _texture;
        private static Shader _shader;
        // mesh
        private static ObjLoader _myObj;
        private static Mesh _myMesh;

        private const string VertShaderSourcePath = "../../../src/Core/shader.vert";
        private const string FragShaderSourcePath = "../../../src/Core/shader.frag";
        private const string TexturePath = "../../../src/Scene/uv-test.png";
        // private const string ObjPath = "../../../src/Scene/suzanne.obj";
        // private const string ObjPath = "../../../res/CLASSROOM.obj";
        // private const string ObjPath = "../../../res/classroom3.obj";
        // private const string ObjPath = "../../../res/SuzanneTri.obj";
        private const string ObjPath = "../../../res/CupOBJ/Cup.obj";
        // private const string ObjPath = "../../../res/index-testing.obj";

        //Setup the camera's location, directions, and movement speed
        private static Vector3 CameraPosition = new Vector3(0.0f, 0.0f, 3.0f);
        private static Vector3 CameraFront = new Vector3(0.0f, 0.0f, -1.0f);
        private static Vector3 CameraUp = Vector3.UnitY;
        private static Vector3 CameraDirection = Vector3.Zero;
        private static float CameraYaw = -90f;
        private static float CameraPitch = 0f;
        private static float CameraZoom = 45f;

        //Used to track change in mouse movement to allow for moving of the Camera
        private static Vector2 LastMousePosition;
        
        // For Camera Speed
        private static float CameraRotateSpeed = 20f;
        private static float CameraTranslateSpeed = 10f;

        private static void Main()
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "LearnOpenGL with Silk.NET";
            _window = Window.Create(options);

            _window.Load += OnLoad;
            _window.Update += OnUpdate;
            _window.Render += OnRender;
            _window.Closing += OnClose;

            _window.Run();
        }


        private static void OnLoad()
        {
            //Set-up input context.
            IInputContext input = _window.CreateInput();

            foreach (var keyboard in input.Keyboards)
            {
                keyboard.KeyDown += KeyDown;
            }
            
            //Getting the opengl api for drawing to the screen.
            _gl = GL.GetApi(_window);
            
            //Creating a shader.
            Console.WriteLine("Compiling shaders...");
            _shader = new Shader(_gl, VertShaderSourcePath, FragShaderSourcePath);
            Console.WriteLine("Shaders Done.");
            
            //Load texture
            _texture = new Texture(_gl, TexturePath);
            
            // load obj
            _myObj = new ObjLoader(ObjPath);
            _myMesh = new Mesh(_gl, ObjPath, TexturePath);
            
            // create buffers
            Console.WriteLine("Creating buffers...");
            var verts = _myObj.Vertices;
            var txcds = _myObj.TexCoords;
            var norms = _myObj.Normals;
            var bufferSize = (nuint) (verts.Length + txcds.Length + norms.Length);
            // create an empty buffer of proper size
            _vbo = new BufferObject<float>(_gl, BufferTargetARB.ArrayBuffer, bufferSize, null);
            // populate buffer with data
            _vbo.SetSubData(0, verts);
            _vbo.SetSubData(verts.Length, txcds);
            _vbo.SetSubData(verts.Length + txcds.Length, norms);
            // create index buffer
            _ebo = new BufferObject<uint>(_gl, _myObj.Indices, BufferTargetARB.ElementArrayBuffer);
            // create _vao to store buffers
            _vao = new VertexArrayObject<float, uint>(_gl, _vbo, _ebo);
            // tell _vao how data is organized inside of _vbo
            _vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 0, 0);
            _vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, 0, verts.Length);
            _vao.VertexAttributePointer(2, 3, VertexAttribPointerType.Float, 0, verts.Length + txcds.Length);
            
            Console.WriteLine("Buffers done.");
            
        }

        private static void OnRender(double obj)  //draw each frame
        {
            unsafe
            {
                _gl.Enable(EnableCap.DepthTest);
                _gl.Clear((uint) (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
                
                //Bind the geometry and shader.
                _vao.Bind();
                _shader.Use();
            
                //Setting a uniform.
                //Bind a texture and and set the uTexture0 to use texture0.
                _texture.Bind(TextureUnit.Texture0);
                _shader.SetUniform("uTexture0", 0);
                
                // var model = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(difference)) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(difference));
                var model = Matrix4x4.CreateRotationY(MathHelper.DegreesToRadians(CameraYaw)) * Matrix4x4.CreateRotationX(MathHelper.DegreesToRadians(CameraPitch));
                var view = Matrix4x4.CreateLookAt(CameraPosition, CameraPosition + CameraFront, CameraUp);
                var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraZoom), Width / Height, 0.1f, 200.0f);

                // _shader.SetUniform("uModel", model);
                // _shader.SetUniform("uView", view);
                // _shader.SetUniform("uProjection", projection);

                //Draw the geometry.
                //_gl.DrawElements(PrimitiveType.Triangles, (uint) TestQuad.Indices.Length, DrawElementsType.UnsignedInt, null);
                // _gl.DrawElements(PrimitiveType.Triangles, (uint) _myObj.Indices.Length, DrawElementsType.UnsignedInt, null);
                //Use elapsed time to convert to radians to allow our cube to rotate over time
                // var difference = (float) (_window.Time * 100);
                // var difference = 1;
                //We're drawing with just vertices and no indices, and it takes 36 vertices to have a six-sided textured cube
                //_gl.DrawArrays(PrimitiveType.Triangles, 0, 36);
                _myMesh.Draw(_shader, model, view, projection);
            }
        }

        private static void OnUpdate(double obj)
        {
            
        }

        private static void OnClose()
        {
            _vbo.Dispose();
            _ebo.Dispose();
            _vao.Dispose();
        }
        
        private static void KeyDown(IKeyboard arg1, Key key, int arg3)
        {
            //Check to close the window on escape.
            if (key == Key.Escape)
            {
                _window.Close();
            }

            switch (key)
            {
                case Key.W:
                    CameraPitch += CameraRotateSpeed;
                    break;
                case Key.S:
                    CameraPitch -= CameraRotateSpeed;
                    break;
                case Key.A:
                    CameraYaw += CameraRotateSpeed;
                    break;
                case Key.D:
                    CameraYaw -= CameraRotateSpeed;
                    break;
                case Key.F:
                    CameraPosition.Z += CameraTranslateSpeed;
                    break;
                case Key.R:
                    CameraPosition.Z -= CameraTranslateSpeed;
                    break;
            }
        }

        private static void WhileKeyDown()
        {
            
        }
    }
}