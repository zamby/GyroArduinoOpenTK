using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GyroArduinoOpenTK
{
    public partial class Form1 : Form
    {

        GLControl GLC_display;

        bool loaded = false;

        float angleX = 0;
        float angleY = 0;
        float angleZ = 0;

        ISerialData serialData;

        public Form1(ISerialData serialDataIoC)
        {
            InitializeComponent();
            this.GLC_display = new OpenTK.GLControl();
            this.GLC_display.BackColor = System.Drawing.Color.Black;
            this.GLC_display.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GLC_display.Location = new System.Drawing.Point(0, 0);
            this.GLC_display.Name = "GLC_display";
            this.GLC_display.Size = new System.Drawing.Size(857, 542);
            this.GLC_display.TabIndex = 0;
            this.GLC_display.VSync = false;
            this.GLC_display.Load += new System.EventHandler(this.GLC_display_Load);
            this.GLC_display.Paint += new System.Windows.Forms.PaintEventHandler(this.GLC_display_Paint);
            this.GLC_display.Resize += new System.EventHandler(this.GLC_display_Resize);
            this.Controls.Add(this.GLC_display);

            this.serialData = serialDataIoC;

            serialData.EventDataReceive += (DataArray data) =>
            {
                angleX = -data.angleX / (float)Math.PI * 180.0f;
                angleY = data.angleY / (float)Math.PI * 180.0f;
                angleZ = -data.angleZ / (float)Math.PI * 180.0f;

                if (loaded)
                    this.GLC_display.Invalidate();

            };

            serialData.start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void GLC_display_Load(object sender, EventArgs e)
        {
            //CONTEXT
            GL.ClearColor(Color.Black);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);           
            GL.CullFace(CullFaceMode.Back);

            SetupViewport();
            loaded = true;
        }

        void Render()
        {
            if (!loaded)
                return;

            // OPENGL 

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Matrix4 lookat = Matrix4.LookAt(new Vector3(1.0f, 5.0f, 6.0f), Vector3.Zero, Vector3.UnitZ);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref lookat);
            GL.PushMatrix();
            GL.Begin(PrimitiveType.Lines);

            int rows = 10;
            int columns = 10;

            for (int i = -rows; i <= rows; i++)
            {
                if (i == 0)
                    GL.Color3(Color.Green);
                else
                    GL.Color3(Color.Gray);
                GL.Vertex3(-columns, i, 0);
                GL.Vertex3(columns, i, 0);
            }

            /* Vertical lines. */

            for (int i = -columns; i <= columns; i++)
            {
                if (i == 0)
                    GL.Color3(Color.Blue);
                else
                    GL.Color3(Color.Gray);
                GL.Vertex3(i, -rows, 0);
                GL.Vertex3(i, rows, 0);
            }

            GL.End();
            GL.PopMatrix();

            GL.PushMatrix();
            GL.Rotate(angleX, new Vector3(1.0f, 0.0f, 0.0f));
            GL.Rotate(angleY, new Vector3(0.0f, 1.0f, 0.0f));
            GL.Rotate(angleZ, new Vector3(0.0f, 0.0f, 1.0f));
            GL.Begin(PrimitiveType.Quads);
            {
                GL.Color3(Color.Orange);
                GL.Vertex3(-2f, 2f, 0f);
                GL.Color3(Color.Lime);
                GL.Vertex3(-2f, -2f, 0f);
                GL.Color3(Color.Magenta);
                GL.Vertex3(2f, -2f, 0f);
                GL.Color3(Color.Cyan);
                GL.Vertex3(2f, 2f, 0f);
            }

            GL.End();
            GL.PopMatrix();
            GLC_display.SwapBuffers();
        }

        private void SetupViewport()
        {

            float aspectRatio = (float)GLC_display.Width / (float)GLC_display.Height;

            GL.Viewport(0, 0, GLC_display.Width, GLC_display.Height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            OpenTK.Matrix4 perspective = OpenTK.Matrix4.CreatePerspectiveFieldOfView((float)(System.Math.PI / 4f), aspectRatio, 0.01f, 2000f);
            GL.MultMatrix(ref perspective);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }


        private void GLC_display_Resize(object sender, EventArgs e)
        {
            SetupViewport();
            GLC_display.Invalidate();
        }


        private void GLC_display_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            serialData.stop();
        }

    }

}
