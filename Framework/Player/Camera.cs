using System;
using System.Drawing;
using SlimDX;
using WeatherGame.Framework.Input;
using WeatherGame.RenderLoop;

namespace WeatherGame.Framework.Player
{




    internal static class CameraConstants
    {
        public const float DEFAULT_MOUSE_SMOOTHING_SENSITIVITY = 0.5f;
        public const float DEFAULT_SPEED_FLIGHT_YAW = 100.0f;
        public const float DEFAULT_SPEED_MOUSE_WHEEL = 1.0f;
        public const float DEFAULT_SPEED_ORBIT_ROLL = 100.0f;
        public const float DEFAULT_SPEED_ROTATION = 0.3f;
        public const float DEFAULT_VELOCITY_X = 1.0f;
        public const float DEFAULT_VELOCITY_Y = 1.0f;
        public const float DEFAULT_VELOCITY_Z = 1.0f;
        public const int MOUSE_SMOOTHING_CACHE_SIZE = 10;

        public const float DEFAULT_FOVX = (float)Math.PI / 4.0f;
        public const float DEFAULT_ZNEAR = 0.0001f;
        public const float DEFAULT_ZFAR = 1000.0f;

        public const float DEFAULT_ORBIT_MIN_ZOOM = DEFAULT_ZNEAR + 1.0f;
        public const float DEFAULT_ORBIT_MAX_ZOOM = DEFAULT_ZFAR * 0.5f;

        public const float DEFAULT_ORBIT_OFFSET_LENGTH = DEFAULT_ORBIT_MIN_ZOOM +
            (DEFAULT_ORBIT_MAX_ZOOM - DEFAULT_ORBIT_MIN_ZOOM) * 0.25f;

    }

    public enum Behavior
    {
        Physics,
        Spectator,        
    };

    public static class CameraHelper
    {
        public static float ToDegrees(float angle)
        {
            return (float)(angle * (180.0f / Math.PI));
        }
        public static float ToRadians(float angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public static void PerformMouseFiltering(float x, float y, float mouseSmoothingSensitivity, ref Vector2[] mouseSmoothingCache, ref Vector2 smoothedMouseMovement)
        {
            // Shuffle all the entries in the cache.
            // Newer entries at the front. Older entries towards the back.
            for (int i = mouseSmoothingCache.Length - 1; i > 0; --i)
            {
                mouseSmoothingCache[i].X = mouseSmoothingCache[i - 1].X;
                mouseSmoothingCache[i].Y = mouseSmoothingCache[i - 1].Y;
            }

            // Store the current mouse movement entry at the front of cache.
            mouseSmoothingCache[0].X = x;
            mouseSmoothingCache[0].Y = y;

            float averageX = 0.0f;
            float averageY = 0.0f;
            float averageTotal = 0.0f;
            float currentWeight = 1.0f;

            // Filter the mouse movement with the rest of the cache entries.
            // Use a weighted average where newer entries have more effect than
            // older entries (towards the back of the cache).
            for (int i = 0; i < mouseSmoothingCache.Length; ++i)
            {
                averageX += mouseSmoothingCache[i].X * currentWeight;
                averageY += mouseSmoothingCache[i].Y * currentWeight;
                averageTotal += 1.0f * currentWeight;
                currentWeight *= mouseSmoothingSensitivity;
            }

            // Calculate the new smoothed mouse movement.
            smoothedMouseMovement.X = averageX / averageTotal;
            smoothedMouseMovement.Y = averageY / averageTotal;
        }


        public static void PerformMouseSmoothing(float x, float y, int mouseIndex, ref Vector2[] mouseMovement, ref Vector2 smoothedMouseMovement)
        {
            mouseMovement[mouseIndex].X = x;
            mouseMovement[mouseIndex].Y = y;

            smoothedMouseMovement.X = (mouseMovement[0].X + mouseMovement[1].X) * 0.5f;
            smoothedMouseMovement.Y = (mouseMovement[0].Y + mouseMovement[1].Y) * 0.5f;

            mouseIndex ^= 1;
            mouseMovement[mouseIndex].X = 0.0f;
            mouseMovement[mouseIndex].Y = 0.0f;
        }
    }




    public static class Camera
    {
        #region Fields
        private static bool movingAlongNegZ, movingAlongPosZ, movingAlongNegY, movingAlongPosY, movingAlongNegX, movingAlongPosX;

        private static int mouseIndex;
        private static float rotationSpeed, orbitRollSpeed, flightYawSpeed, mouseWheelSpeed;
        private static float mouseSmoothingSensitivity;
        private static Vector3 currentVelocity, velocity, movementDirection;
        private static Vector2[] mouseMovement;
        private static Vector2[] mouseSmoothingCache;
        private static Vector2 smoothedMouseMovement;

        private static Vector3 WORLD_X_AXIS = Vector3.UnitX;
        private static Vector3 WORLD_Y_AXIS = Vector3.UnitY;
        private static Vector3 WORLD_Z_AXIS = Vector3.UnitZ;

        private static Behavior behavior;
        private static bool preferTargetYAxisOrbiting;

        private static float fovx, aspectRatio, znear, zfar, accumPitchDegrees, orbitMinZoom, orbitMaxZoom, orbitOffsetLength, firstPersonYOffset;
        private static Vector3 eye, target, targetYAxis, xAxis, yAxis, zAxis, viewDir, viewLeft, viewRight, savedEye;

        private static Quaternion orientation;
        private static Matrix viewMatrix, projMatrix;
        public static Matrix reflectionViewMatrix;

        private static Quaternion savedOrientation;
        #endregion

        #region Public Methods

        #region Init
        static Camera()
        {
            behavior = Behavior.Spectator;
            preferTargetYAxisOrbiting = true;

            fovx = CameraConstants.DEFAULT_FOVX;
            znear = CameraConstants.DEFAULT_ZNEAR;
            zfar = CameraConstants.DEFAULT_ZFAR;

            accumPitchDegrees = 0.0f;
            orbitMinZoom = CameraConstants.DEFAULT_ORBIT_MIN_ZOOM;
            orbitMaxZoom = CameraConstants.DEFAULT_ORBIT_MAX_ZOOM;
            orbitOffsetLength = CameraConstants.DEFAULT_ORBIT_OFFSET_LENGTH;
            firstPersonYOffset = 0.0f;

            eye = Vector3.Zero;
            target = Vector3.Zero;
            targetYAxis = Vector3.UnitY;
            xAxis = Vector3.UnitX;
            yAxis = Vector3.UnitY;
            zAxis = Vector3.UnitZ;

            orientation = Quaternion.Identity;
            viewMatrix = Matrix.Identity;

            savedEye = eye;
            savedOrientation = orientation;

            movingAlongNegZ = movingAlongPosZ = movingAlongNegY = movingAlongPosY = movingAlongNegX = movingAlongPosX = false;

            rotationSpeed = CameraConstants.DEFAULT_SPEED_ROTATION;
            orbitRollSpeed = CameraConstants.DEFAULT_SPEED_ORBIT_ROLL;
            flightYawSpeed = CameraConstants.DEFAULT_SPEED_FLIGHT_YAW;
            mouseWheelSpeed = CameraConstants.DEFAULT_SPEED_MOUSE_WHEEL;
            mouseSmoothingSensitivity = CameraConstants.DEFAULT_MOUSE_SMOOTHING_SENSITIVITY;
            velocity = new Vector3(CameraConstants.DEFAULT_VELOCITY_X, CameraConstants.DEFAULT_VELOCITY_Y, CameraConstants.DEFAULT_VELOCITY_Z);
            mouseSmoothingCache = new Vector2[CameraConstants.MOUSE_SMOOTHING_CACHE_SIZE];
            movementDirection = new Vector3(0, 0, 0);

            mouseIndex = 0;
            mouseMovement = new Vector2[2];
            mouseMovement[0].X = 0.0f;
            mouseMovement[0].Y = 0.0f;
            mouseMovement[1].X = 0.0f;
            mouseMovement[1].Y = 0.0f;
        }
        public static void Initialize(GameWindow gw)
        {
            Rectangle clientBounds = gw.ClientRectangle;
            NativeMethods.SetCursorPos(clientBounds.Width / 2, clientBounds.Height / 2);

            float aspect = (float)clientBounds.Width / (float)clientBounds.Height;
            Perspective(CameraConstants.DEFAULT_FOVX, aspect, CameraConstants.DEFAULT_ZNEAR, CameraConstants.DEFAULT_ZFAR);
        }
        public static void Perspective(float fovx, float aspect, float znear, float zfar)
        {
            Camera.fovx = fovx;
            Camera.aspectRatio = aspect;
            Camera.znear = znear;
            Camera.zfar = zfar;

            projMatrix = Matrix.PerspectiveFovLH(fovx, aspect, znear, zfar);
        }
        #endregion

        #region LookAt
        public static void LookAt(Vector3 target) { LookAt(eye, target, yAxis); }
        public static void LookAt(Vector3 eye, Vector3 target, Vector3 up)
        {

            Camera.eye = eye;
            Camera.target = target;

            zAxis = target - eye;
            zAxis.Normalize();

            viewDir = zAxis;
            viewLeft = yAxis;
            viewRight = xAxis;

            Vector3.Cross(ref zAxis, ref up, out xAxis);
            xAxis.Normalize();

            Vector3.Cross(ref zAxis, ref xAxis, out yAxis);
            yAxis.Normalize();
            xAxis.Normalize();

            viewMatrix = Matrix.Identity;

            viewMatrix[0, 0] = xAxis.X;
            viewMatrix[1, 0] = xAxis.Y;
            viewMatrix[2, 0] = xAxis.Z;
            viewMatrix[3, 0] = -Vector3.Dot(xAxis, eye);

            viewMatrix[0, 1] = yAxis.X;
            viewMatrix[1, 1] = yAxis.Y;
            viewMatrix[2, 1] = yAxis.Z;
            viewMatrix[3, 1] = -Vector3.Dot(yAxis, eye);

            viewMatrix[0, 2] = zAxis.X;
            viewMatrix[1, 2] = zAxis.Y;
            viewMatrix[2, 2] = zAxis.Z;
            viewMatrix[3, 2] = -Vector3.Dot(zAxis, eye);

            // Extract the pitch angle from the view matrix.
            accumPitchDegrees = CameraHelper.ToDegrees((float)Math.Asin(viewMatrix[1, 2]));

            Quaternion.RotationMatrix(ref viewMatrix, out orientation);

        }
        public static void UndoRoll()
        {
            //LookAt(eye, eye + ViewDirection, WORLD_Y_AXIS);
        }
        #endregion

        #region Move
        public static void Move(float dx, float dy, float dz)
        {
            Vector3 forwards;

            if (behavior == Behavior.Physics)
            {
                // Calculate the forwards direction. Can't just use the
                // camera's view direction as doing so will cause the camera to
                // move more slowly as the camera's view approaches 90 degrees
                // straight up and down.

                forwards = Vector3.Normalize(Vector3.Cross(xAxis, WORLD_Y_AXIS));
            }
            else
            {
                forwards = viewDir;
            }

            eye += xAxis * dx;
            eye += WORLD_Y_AXIS * dy;
            eye += forwards * dz;

            Position = eye;
        }
        public static void Move(Vector3 direction, Vector3 distance)
        {
            eye.X += direction.X * distance.X;
            eye.Y += direction.Y * distance.Y;
            eye.Z += direction.Z * distance.Z;

            UpdateViewMatrix();
        }
        #endregion

        public static void Rotate(float headingDegrees, float pitchDegrees, float rollDegrees)
        {
            headingDegrees = -headingDegrees;
            pitchDegrees = -pitchDegrees;
            rollDegrees = -rollDegrees;

            switch (behavior)
            {
                case Behavior.Physics:
                case Behavior.Spectator:
                    RotateFirstPerson(headingDegrees, pitchDegrees);
                    break;
                default:
                    break;
            }

            UpdateViewMatrix();
        }
        public static void Zoom(float zoom, float minZoom, float maxZoom)
        {
            zoom = Math.Min(Math.Max(zoom, minZoom), maxZoom);
            Perspective(zoom, aspectRatio, znear, zfar);
        }
        public static void Update(bool LockCameraView)
        {
            if (MouseInput.CurrentState != null)
            {
                CameraHelper.PerformMouseFiltering((float)MouseInput.CurrentState.X, (float)MouseInput.CurrentState.Y, mouseSmoothingSensitivity, ref mouseSmoothingCache, ref smoothedMouseMovement);
                CameraHelper.PerformMouseSmoothing(smoothedMouseMovement.X, smoothedMouseMovement.Y, mouseIndex, ref mouseMovement, ref smoothedMouseMovement);
            }

            float elapsedGameTime = 1.0f;
            if (Game.Time != null) elapsedGameTime = Game.Time.ElapsedGameTime;

            switch (CurrentBehavior)
            {
                case Behavior.Physics:
                case Behavior.Spectator:
                    if(!LockCameraView)
                        RotateSmoothly(smoothedMouseMovement.X, smoothedMouseMovement.Y, 0.0f);
                    UpdatePosition(movementDirection, elapsedGameTime);
                    break;
                default:
                    break;
            }
        }       
        #endregion

        #region Private Methods
        private static void ChangeBehavior(Behavior newBehavior)
        {
            Behavior prevBehavior = behavior;
            if (prevBehavior == newBehavior) return;
            behavior = newBehavior;

            switch (newBehavior)
            {
                case Behavior.Physics:
                    switch (prevBehavior)
                    {
                        case Behavior.Spectator:
                            eye.Y = firstPersonYOffset;
                            UpdateViewMatrix();
                            break;

                        default:
                            break;
                    }

                    UndoRoll();
                    break;

                case Behavior.Spectator:


                    UndoRoll();
                    break;

                default:
                    break;
            }
        }
        private static void ChangeOrientation(Quaternion newOrientation)
        {
            Matrix m = Matrix.RotationQuaternion(newOrientation);

            // Store the pitch for this new orientation.
            // First person and spectator behaviors limit pitching to
            // 90 degrees straight up and down.

            float pitch = (float)Math.Asin(m.M23);

            accumPitchDegrees = CameraHelper.ToDegrees(pitch);

            // First person and spectator behaviors don't allow rolling.
            // Negate any rolling that might be encoded in the new orientation.

            orientation = newOrientation;

            if (behavior == Behavior.Physics || behavior == Behavior.Spectator)
                LookAt(eye, eye + viewDir, WORLD_Y_AXIS);

            UpdateViewMatrix();
        }
        private static void RotateFirstPerson(float headingDegrees, float pitchDegrees)
        {
            accumPitchDegrees += pitchDegrees;

            if (accumPitchDegrees > 90.0f)
            {
                pitchDegrees = 90.0f - (accumPitchDegrees - pitchDegrees);
                accumPitchDegrees = 90.0f;
            }

            if (accumPitchDegrees < -90.0f)
            {
                pitchDegrees = -90.0f - (accumPitchDegrees - pitchDegrees);
                accumPitchDegrees = -90.0f;
            }

            float heading = CameraHelper.ToRadians(headingDegrees);
            float pitch = CameraHelper.ToRadians(pitchDegrees);
            Quaternion rotation = Quaternion.Identity;

            // Rotate the camera about the world Y axis.
            if (heading != 0.0f)
            {
                Quaternion.RotationAxis(ref WORLD_Y_AXIS, heading, out rotation);
                //Quaternion.Concatenate(ref rotation, ref orientation, out orientation);
                orientation = Concatenate(rotation, orientation);          
            }

            // Rotate the camera about its local X axis.
            if (pitch != 0.0f)
            {
                Quaternion.RotationAxis(ref WORLD_X_AXIS, pitch, out rotation);
                //Quaternion.Concatenate(ref orientation, ref rotation, out orientation);                
                orientation = Concatenate(orientation, rotation);                          
            }
        }

        private static Quaternion Concatenate( Quaternion left, Quaternion right )
        {
                Quaternion quaternion;
                float rx = right.X;
                float ry = right.Y;
                float rz = right.Z;
                float rw = right.W;
                float lx = left.X;
                float ly = left.Y;
                float lz = left.Z;
                float lw = left.W;
                float yz = (ry * lz) - (rz * ly);
                float xz = (rz * lx) - (rx * lz);
                float xy = (rx * ly) - (ry * lx);
                float lengthSq = ((rx * lx) + (ry * ly)) + (rz * lz);

                quaternion.X = ((rx * lw) + (lx * rw)) + yz;
                quaternion.Y = ((ry * lw) + (ly * rw)) + xz;
                quaternion.Z = ((rz * lw) + (lz * rw)) + xy;
                quaternion.W = (rw * lw) - lengthSq;

                return quaternion;
        }


        private static void UpdateViewMatrix()
        {
            orientation.Normalize();
            Matrix.RotationQuaternion(ref orientation, out viewMatrix);

            xAxis = new Vector3(viewMatrix[0, 0], viewMatrix[1, 0], viewMatrix[2, 0]);
            yAxis = new Vector3(viewMatrix[0, 1], viewMatrix[1, 1], viewMatrix[2, 1]);
            zAxis = new Vector3(viewMatrix[0, 2], viewMatrix[1, 2], viewMatrix[2, 2]);
            viewDir = -zAxis;
            viewRight = xAxis;
            viewLeft = -xAxis;

            viewMatrix[3, 0] = -Vector3.Dot(xAxis, eye);
            viewMatrix[3, 1] = -Vector3.Dot(yAxis, eye);
            viewMatrix[3, 2] = -Vector3.Dot(zAxis, eye);

            Plane WaterPlane = new Plane(Vector3.UnitY, 0);
            reflectionViewMatrix = Matrix.Reflection(WaterPlane) * viewMatrix;
                      

        }
        private static void RotateSmoothly(float headingDegrees, float pitchDegrees, float rollDegrees)
        {
            headingDegrees *= rotationSpeed;
            pitchDegrees *= rotationSpeed;
            rollDegrees *= rotationSpeed;

            Rotate(headingDegrees, pitchDegrees, rollDegrees);
        }

        private static void UpdateVelocity(Vector3 direction, float elapsedTimeSec)
        {
            if (CurrentBehavior == Behavior.Physics) return;

            if (direction.X != 0.0f)
                currentVelocity.X = direction.X * velocity.X;
            else
                currentVelocity.X = 0.0f;

            if (direction.Y != 0.0f)
                currentVelocity.Y = direction.Y * velocity.Y;
            else
                currentVelocity.Y = 0.0f;


            if (direction.Z != 0.0f)
                currentVelocity.Z = direction.Z * velocity.Z;
            else
                currentVelocity.Z = 0.0f;

        }
        private static void UpdatePosition(Vector3 direction, float elapsedTimeSec)
        {
            if (CurrentBehavior == Behavior.Physics) return;

            if (currentVelocity.LengthSquared() != 0.0f)
            {
                // Only move the camera if the velocity vector is not of zero
                // length. Doing this guards against the camera slowly creeping
                // around due to floating point rounding errors.

                Vector3 displacement = (currentVelocity * elapsedTimeSec) +
                    (0.5f * new Vector3(1, 1, 1) * elapsedTimeSec * elapsedTimeSec);

                // Floating point rounding errors will slowly accumulate and
                // cause the camera to move along each axis. To prevent any
                // unintended movement the displacement vector is clamped to
                // zero for each direction that the camera isn't moving in.
                // Note that the UpdateVelocity() method will slowly decelerate
                // the camera's velocity back to a stationary state when the
                // camera is no longer moving along that direction. To account
                // for this the camera's current velocity is also checked.

                if (direction.X == 0.0f && (float)Math.Abs(currentVelocity.X) < 1e-6f)
                    displacement.X = 0.0f;

                if (direction.Y == 0.0f && (float)Math.Abs(currentVelocity.Y) < 1e-6f)
                    displacement.Y = 0.0f;

                if (direction.Z == 0.0f && (float)Math.Abs(currentVelocity.Z) < 1e-6f)
                    displacement.Z = 0.0f;

                Move(displacement.X, displacement.Y, displacement.Z);
            }

            // Continuously update the camera's velocity vector even if the
            // camera hasn't moved during this call. When the camera is no
            // longer being moved the camera is decelerating back to its
            // stationary state.

            UpdateVelocity(direction, elapsedTimeSec);
        }
        #endregion

        #region Properties
        public static Behavior CurrentBehavior
        {
            get { return behavior; }
            set { ChangeBehavior(value); }
        }
        public static float OrbitMaxZoom
        {
            get { return orbitMaxZoom; }
            set { orbitMaxZoom = value; }
        }
        public static float OrbitMinZoom
        {
            get { return orbitMinZoom; }
            set { orbitMinZoom = value; }
        }
        public static float OrbitOffsetDistance
        {
            get { return orbitOffsetLength; }
            set { orbitOffsetLength = value; }
        }
        public static Vector3 OrbitTarget
        {
            get { return target; }
            set { target = value; }
        }
        public static Quaternion Orientation
        {
            get { return orientation; }
            set { ChangeOrientation(value); }
        }
        public static Vector3 Position
        {
            get { return eye; }
            set
            {
                eye = value;
                UpdateViewMatrix();
            }
        }
        public static bool PreferTargetYAxisOrbiting
        {
            get { return preferTargetYAxisOrbiting; }
            set
            {
                preferTargetYAxisOrbiting = value;

                if (preferTargetYAxisOrbiting)
                    UndoRoll();
            }
        }
        public static Matrix ProjectionMatrix { get { return projMatrix; } }
        public static Vector3 ViewDirection { get { return viewDir; } }
        public static Matrix ViewMatrix { get { return viewMatrix; } }
        public static Matrix ViewProjectionMatrix { get { return viewMatrix * projMatrix; } }
        public static Vector3 XAxis { get { return xAxis; } }
        public static Vector3 YAxis { get { return yAxis; } }
        public static Vector3 ZAxis { get { return zAxis; } }
        public static Vector3 Forward
        {
            get
            {                               
                return viewDir;
            }             
        }
        public static Vector3 Left
        {
            get
            {
                return viewLeft;
            }
        }
        public static Vector3 Right
        {
            get
            {
                return viewRight;
            }
        }

        public static Vector3 CurrentVelocity
        {
            get { return currentVelocity; }
        }
        public static float FlightYawSpeed
        {
            get { return flightYawSpeed; }
            set { flightYawSpeed = value; }
        }
        public static float MouseSmoothingSensitivity
        {
            get { return mouseSmoothingSensitivity; }
            set { mouseSmoothingSensitivity = value; }
        }
        public static float MouseWheelSpeed
        {
            get { return mouseWheelSpeed; }
            set { mouseWheelSpeed = value; }
        }
        public static float OrbitRollSpeed
        {
            get { return orbitRollSpeed; }
            set { orbitRollSpeed = value; }
        }
        public static float RotationSpeed
        {
            get { return rotationSpeed; }
            set { rotationSpeed = value; }
        }
        public static Vector3 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        public static Vector3 MovementDirection
        {
            get { return movementDirection; }
            set
            {
                if (CurrentBehavior == Behavior.Physics)
                    value.Y = 0;

                movementDirection = value;
            }
        }
        public static Vector3 Target
        {
            get { return target; }            
        }
      
        public static float Fovx
        {
            get { return fovx; }
        }
        public static float AspectRatio
        {
            get { return aspectRatio; }
        }
        #endregion

    }
}
