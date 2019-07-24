using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.ComponentModel;
using WorldEditor.Rendering;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D10;
using D3D10 = SlimDX.Direct3D10;
using DXGI = SlimDX.DXGI;
using WeatherGame.RenderLoop;
using WeatherGame.Framework.Interfaces;
using WeatherGame.Framework.Input;
using WeatherGame.Framework.Rendering;
using WeatherGame.Framework.Player;
using WeatherGame;
using WeatherGame.Framework;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.World;

namespace WorldEditor.Rendering
{
    public static class Gizmo
    {
        public static Model[] model = new Model[3];

        public static Vector3 Position { get; set; }
        public static Matrix Rotation { get; set; }
        public static Vector3 Scale { get; set; }


        // BoundingBox matching X axis (BLUE-Arrow)
        public static BoundingBox bbXaxis
        {
            get
            {
                return new BoundingBox(
                    Position,
                    Position - (new Vector3(5.5f, 0.25f, 0.25f))
                );
            }
        }

        // BoundingBox matching Z axis (GREEN-Arrow)
        public static BoundingBox bbZaxis
        {
            get
            {
                return new BoundingBox(
                    Position,
                    Position + (new Vector3(-0.25f, 0.25f, 5.5f))
                );
            }
        }

        // BoundingBox matching Y axis (Up, RED-Arrow)
        public static BoundingBox bbYaxis
        {
            get
            {
                return new BoundingBox(
                    Position,
                    Position + (new Vector3(-0.25f, 5.5f, 0.25f))
                );
            }
        }

        static Gizmo()
        {
            // Set all the right data for the gizmo                        
            model[0] = WorldData.GetObject("gizmo") as Model;

            // model[1] = Game1.Instance.Content.Load<Model>("GizmoAxisRotate");
            //model[2] = Game1.Instance.Content.Load<Model>("GizmoAxisScale");

            Position = Vector3.Zero;
            Scale = new Vector3(1, 1, 1);
            Rotation = Matrix.Identity;
        }

        public static void Draw()
        {

            // Vector3.Backward = fix for bad model pivot.
            Matrix world = Matrix.Translation(Position);


            RenderingFunctions.RenderMesh2(model[0], Vector3.Zero, "SimpleTexturedQuad.fx", "RenderDiffOnly", world);
        }

        public static void DrawOrigin(Matrix mWorld)
        {
            Matrix world = Matrix.Scaling(2, 2, 2) * mWorld;
            RenderingFunctions.RenderMesh2(model[0], Vector3.Zero, "SimpleTexturedQuad.fx", "RenderDiffOnly", world);
        }
    }

    public enum GizmoMode
    {
        Translate,
        Rotate,
        Scale
    }

    public static class ObjectPicker
    {
        public static GameObjectReference SelectedObject;
        
        private static float dist = 0, sDist = 0;
        private static bool dragX = false, dragY = false, dragZ = false;
        
        private static float moveX, moveY, moveZ;        
        private static int finalPosX, finalPosY, finalPosZ;
        
        static int gridSpacing = 1;        
        private static bool wasDragging = false;
        
        public static GizmoMode gizmoMode;
        public static event EventHandler SelectedObjectChanged;
        

        static ObjectPicker()
        {
            //Gizmo = new Gizmo();
            //Gizmo.Scale = new Vector3(1,1,1);            
        }

        public static void Update()
        {
            gizmoMode = GizmoMode.Translate;
            if (SelectedObject == null) return;


            Gizmo.Position = SelectedObject.Position;
        }


        public static void SelectionRay(MouseButtonEventArgs e, Point p)
        {
            //if (wasDragging) return;
            Viewport[] vps = Game.Device.Rasterizer.GetViewports();
            if (vps == null || vps.Length == 0) return;

            moveX = 0; moveY = 0; moveZ = 0;

            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                Viewport vp = Game.Device.Rasterizer.GetViewports()[0];
                Vector2 mousePosition = new Vector2((float)p.X, (float)p.Y);
                int mouseX = (int)p.X;
                int mouseY = (int)p.Y;

                if (mouseX < 0) mouseX = 0;
                if (mouseY < 0) mouseY = 0;
                if (mouseX > vp.Width) mouseX = vp.Width;
                if (mouseY > vp.Height) mouseY = vp.Height;

                Vector3 near = new Vector3(mouseX, mouseY, 0f);
                Vector3 far = new Vector3(mouseX, mouseY, 1f);

                Vector3 position;
                Vector3 direction;

                Ray transformedRay;
                bool found = false;
                Dictionary<String, GameObjectReference> objDic = MainPlayer.CurrentWorldSpace.GetAllModelObjRefs(Global.GlobalSettings.CurrentCell);
                List<GameObjectReference> gm = new List<GameObjectReference>();
                foreach (GameObjectReference objRef in objDic.Values) gm.Add(objRef);
                for (int i = gm.Count - 1; i >= 0 && !found; i--)
                {
                    Model model = gm[i].BaseGameObject as Model;
                    
                    Vector3 v;
                    v.X = (((2.0f * mouseX) / vp.Width) - 1) / Camera.ProjectionMatrix[0, 0];
                    v.Y = -(((2.0f * mouseY) / vp.Height) - 1) / Camera.ProjectionMatrix[1, 1];
                    v.Z = 1.0f;

                    // Get the inverse view matrix
                    Matrix m = Matrix.Invert(Matrix.Scaling(gm[i].Scale.X, gm[i].Scale.Y, gm[i].Scale.Z) * gm[i].Rotation * WorldSpace.GetRealWorldMatrix(gm[i].Position, Global.GlobalSettings.CurrentCell) * Camera.ViewMatrix);                    

                    // Transform the screen space pick ray into 3D space
                    direction.X = v.X * m[0,0] + v.Y * m[1,0] + v.Z * m[2,0];
                    direction.Y = v.X * m[0,1] + v.Y * m[1,1] + v.Z * m[2,1];
                    direction.Z = v.X * m[0,2] + v.Y * m[1,2] + v.Z * m[2,2];
                    position.X = m[3,0];
                    position.Y = m[3,1];
                    position.Z = m[3,2];


                    transformedRay = new Ray(position, direction);
                    float distance;
                    if (Ray.Intersects(transformedRay, model.Mesh3d.bb, out distance))
                    {
                        if (DoesRayInterestMesh(gm[i].BaseGameObject, position, direction))
                        {
                            SelectedObject = gm[i];
                            found = true;
                            if (SelectedObjectChanged != null) SelectedObjectChanged(null, EventArgs.Empty);
                        }
                        else
                            SelectedObject = null;
                    }
                    else
                    {
                        SelectedObject = null;
                    }
                }
            }
            
            
        }


        #region Helpers
        private static bool DoesRayInterestMesh(BaseGameObject gm, Vector3 orig, Vector3 dir)
        {
            Model model = gm as Model;

            Vector3[] verts = model.Mesh3d.GetMeshVertices();
            int[] indices = model.Mesh3d.GetMeshIndices();
            int dwNumFaces = model.MeshObj.FaceCount;
            for (int i = 0; i < dwNumFaces; i++)
            {
                int i0 = indices[3 * i + 0];
                int i1 = indices[3 * i + 1];
                int i2 = indices[3 * i + 2];
                Vector3 v0 = verts[i0];
                Vector3 v1 = verts[i1];
                Vector3 v2 = verts[i2];

                //// Check if the pick ray passes through this point
                if (IntersectTriangle(orig, dir, v0, v1, v2))
                    return true;
            }
            return false;

        }

        //--------------------------------------------------------------------------------------
        // Given a ray origin (orig) and direction (dir), and three vertices of a triangle, this
        // function returns TRUE and the interpolated texture coordinates if the ray intersects 
        // the triangle
        //--------------------------------------------------------------------------------------
        private static bool IntersectTriangle(Vector3 orig, Vector3 dir,
                        Vector3 v0, Vector3 v1, Vector3 v2)
        {
            // Find vectors for two edges sharing vert0
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;

            // Begin calculating determinant - also used to calculate U parameter
            Vector3 pvec = Vector3.Cross(dir, edge2);

            // If determinant is near zero, ray lies in plane of triangle
            float det = Vector3.Dot(edge1, pvec);

            Vector3 tvec;
            if (det > 0)
            {
                tvec = orig - v0;
            }
            else
            {
                tvec = v0 - orig;
                det = -det;
            }

            if (det < 0.0001f)
                return false;

            // Calculate U parameter and test bounds
            float u = Vector3.Dot(tvec, pvec);
            if (u < 0.0f || u > det)
                return false;

            // Prepare to test V parameter
            Vector3 qvec = Vector3.Cross(tvec, edge1);

            // Calculate V parameter and test bounds
            float v = Vector3.Dot(dir, qvec);
            if (v < 0.0f || u + v > det)
                return false;

            return true;
        }
        #endregion

    }
}
