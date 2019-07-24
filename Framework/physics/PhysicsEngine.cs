using System;
using System.Collections.Generic;
using System.Diagnostics;
using BEPUphysics;
using BEPUphysics.Collidables;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Constraints;
using BEPUphysics.Entities;
using BEPUphysics.MathExtensions;
using BEPUphysics.PositionUpdating;
using BEPUphysics.Settings;
using SlimDX;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.Player;
using WeatherGame.Framework.World;
using WeatherGame.RenderLoop;

namespace WeatherGame.Framework.physics
{
    public static class PhysicsEngine
    {
        
        public static Space Space { get; set; }        
        public static double PhysicsTime { get; private set; }

        private static int accumulatedPhysicsFrames;
        private static double accumulatedPhysicsTime;
        private static double previousTimeMeasurement;

        private static bool _paused = true;

        private static Dictionary<string, InstancedMeshShape> staticMeshShapeCache;

        private static Dictionary<string, MobileMeshShape> mobileMeshShapeCache;
        private static Dictionary<string, ShapeDistributionInformation> mobileMeshInfoCache;
        

        static PhysicsEngine()
        {
            Space = new Space();
            Space.ForceUpdater.Gravity = new Vector3(0, -22.81f, 0);            
            //Space.ForceUpdater.Gravity = new Vector3(0, 0.0f, 0);
            Space.Solver.IterationLimit = 40;
                        
            MotionSettings.DefaultPositionUpdateMode = PositionUpdateMode.Continuous;
            MotionSettings.UseExtraExpansionForContinuousBoundingBoxes = true;
            SolverSettings.DefaultMinimumIterations = 20;                        

            MotionSettings.ConserveAngularMomentum = true;
            MotionSettings.UseRk4AngularIntegration = true;
            

            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    Space.ThreadManager.AddThread();
                }
            }

            staticMeshShapeCache = new Dictionary<string, InstancedMeshShape>();
            mobileMeshShapeCache = new Dictionary<string, MobileMeshShape>();
            mobileMeshInfoCache = new Dictionary<string, ShapeDistributionInformation>();
        }

        public static void Initialize()
        {
            staticMeshShapeCache = new Dictionary<string, InstancedMeshShape>();
            mobileMeshShapeCache = new Dictionary<string, MobileMeshShape>();
            mobileMeshInfoCache = new Dictionary<string, ShapeDistributionInformation>();

            foreach (Entity e in Space.Entities)
            {
                Space.Remove(e);
            }

            if (MainPlayer.CurrentWorldSpace != null)
            {
                List<Cell> cells = MainPlayer.CurrentWorldSpace.AllCells;
                foreach (Cell c in cells)
                {
                    Dictionary<String, GameObjectReference> meshes = MainPlayer.CurrentWorldSpace.GetAllModelObjRefs(c);
                    foreach (GameObjectReference m in meshes.Values)
                    {
                        AddMeshToSpace(m, c);
                    }
                }
            }

            PausePhysics();
        }


        public static void AddMeshToSpace(GameObjectReference m, Cell c)
        {
            Model model = m.BaseGameObject as Model;
            if (model.StaticMesh)
            {                
                InstancedMeshShape sms = null;                
                if (staticMeshShapeCache.ContainsKey(model.Mesh3d.meshFileName))
                    sms = staticMeshShapeCache[model.Mesh3d.meshFileName];
                else
                {
                    Vector3[] verts = model.Mesh3d.GetMeshVertices();
                    int[] indecies = model.Mesh3d.GetMeshIndices();
                    sms = new InstancedMeshShape(verts, indecies);
                    staticMeshShapeCache.Add(model.Mesh3d.meshFileName, sms);
                }                
                

                AffineTransform entityMatrix = new AffineTransform(m.Scale, Quaternion.RotationMatrix(m.Rotation), WorldSpace.GetRealWorldPos(m.Position, c));               
                InstancedMesh mm = new InstancedMesh(sms, entityMatrix);                
                mm.Tag = m;
                mm.ImproveBoundaryBehavior = true;                
                mm.Sidedness = BEPUphysics.CollisionShapes.ConvexShapes.TriangleSidedness.Clockwise;                
                PhysicsEngine.Space.Add(mm);
                m.PhysicsObj = mm;
                m.EntityTransform = entityMatrix.Matrix;                
            }
            else
            {
                MobileMeshShape mms = null;
                ShapeDistributionInformation shapeInfo;
                if (mobileMeshShapeCache.ContainsKey(model.Mesh3d.meshFileName))
                {
                    mms = mobileMeshShapeCache[model.Mesh3d.meshFileName];
                    shapeInfo = mobileMeshInfoCache[model.Mesh3d.meshFileName];
                }
                else
                {
                    Vector3[] verts = model.Mesh3d.GetMeshVertices();
                    int[] indecies = model.Mesh3d.GetMeshIndices();
                    mms = new MobileMeshShape(verts, indecies, AffineTransform.Identity, MobileMeshSolidity.Solid, out shapeInfo);
                    mobileMeshShapeCache.Add(model.Mesh3d.meshFileName, mms);
                    mobileMeshInfoCache.Add(model.Mesh3d.meshFileName, shapeInfo);
                }
                mms.Sidedness = BEPUphysics.CollisionShapes.ConvexShapes.TriangleSidedness.Clockwise;


                Matrix mWorld = m.Rotation * Matrix.Translation(WorldSpace.GetRealWorldPos(m.Position, c));
                Entity e = new Entity(mms, 10);
                e.CollisionInformation.LocalPosition = shapeInfo.Center;
                e.WorldTransform = mWorld;
                e.Tag = m;
                e.PositionUpdateMode = PositionUpdateMode.Continuous;
                PhysicsEngine.Space.Add(e);
                m.PhysicsObj = e;                
                m.EntityTransform = mWorld;
            }            
        }


        public static void Update()
        {
            if (Paused) return;

            long startTime = Stopwatch.GetTimestamp();
            float dt = Game.Time.ElapsedGameTime;

            //Update the simulation.
            //Pass in dt to the function to use internal timestepping, if desired.
            //Using internal time stepping usually works best when the interpolation is also used.
            //Check out the asynchronous updating documentation for an example (though you don't have to use a separate thread to use interpolation).
            Space.Update();
            
            long endTime = Stopwatch.GetTimestamp();
            accumulatedPhysicsTime += (endTime - startTime) / (double)Stopwatch.Frequency;
            accumulatedPhysicsFrames++;
            previousTimeMeasurement += dt;
            if (previousTimeMeasurement > .3f)
            {
                previousTimeMeasurement -= .3f;
                PhysicsTime = accumulatedPhysicsTime / accumulatedPhysicsFrames;
                accumulatedPhysicsTime = 0;
                accumulatedPhysicsFrames = 0;
            }
            
            
            foreach (Entity e in Space.Entities)
            {
                if (e.Tag != null && e.Tag is GameObjectReference)
                {
                    GameObjectReference gm = (GameObjectReference)e.Tag;
                    Model model = gm.BaseGameObject as Model;
                    gm.EntityTransform = e.WorldTransform;

                    e.Mass = model.Mass;            
                    //e.ApplyImpulse(e.CollisionInformation.LocalPosition, new Vector3(1, 1, 0));                                
                }
            }            
        }



        public static bool Paused
        {
            get
            {
                return _paused;
            }
            set
            {
                _paused = value;
                if (_paused) PausePhysics();
            }
        }

        private static void PausePhysics()
        {
            foreach (Entity e in Space.Entities)
            {
                if (e.Tag != null && e.Tag is GameObjectReference)
                {
                    GameObjectReference gm = (GameObjectReference)e.Tag;
                    Matrix entityMatrix = gm.Rotation * Matrix.Translation(WorldSpace.GetRealWorldPos(gm.Position, gm.CellContainer));
                    e.WorldTransform = entityMatrix;
                    e.AngularVelocity = Vector3.Zero;
                    e.AngularMomentum = Vector3.Zero;
                    e.AngularDamping = 0;
                    e.LinearMomentum = Vector3.Zero;
                    e.LinearVelocity = Vector3.Zero;
                    gm.EntityTransform = entityMatrix;
                }
            }
        }
        public static void UpdateMeshMatrix(GameObjectReference gm)
        {            
            if (gm.PhysicsObj == null) return;

            Model model = gm.BaseGameObject as Model;
            if (model.StaticMesh)
            {
                Cell c = WorldData.GetObject(gm.CellContainerId) as Cell;
                AffineTransform entityMatrix = new AffineTransform(gm.Scale, Quaternion.RotationMatrix(gm.Rotation), WorldSpace.GetRealWorldPos(gm.Position, c));                               

                InstancedMesh sm = gm.PhysicsObj as InstancedMesh;
                sm.WorldTransform = entityMatrix;
                gm.EntityTransform = entityMatrix.Matrix;
                sm.UpdateBoundingBox();                                     
            }
            else
            {
                if (!Paused) return;

                Cell c = WorldData.GetObject(gm.CellContainerId) as Cell;
                Matrix entityMatrix = gm.Rotation * Matrix.Translation(WorldSpace.GetRealWorldPos(gm.Position, c));

                Entity mm = gm.PhysicsObj as Entity;
                mm.WorldTransform = entityMatrix;
                gm.EntityTransform = entityMatrix;                               
            }            
        }
        

        public static void Dispose()
        {                 
            Space.Dispose();            
        }
    }
}
