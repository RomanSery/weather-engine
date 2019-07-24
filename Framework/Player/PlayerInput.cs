using System.Collections.Generic;
using SlimDX;
using SlimDX.DirectInput;
using WeatherGame.Framework.Input;
using WeatherGame.Framework.physics;
using WeatherGame.Framework.physics.Character;

namespace WeatherGame.Framework.Player
{
    public class PlayerInput
    {
        public enum Actions
        {
            MoveForwards,
            MoveBackwards,
            MoveDown,
            MoveUp,
            StrafeRight,
            StrafeLeft,
            Crouch,
            Jump
        };

        private Dictionary<Actions, Key> actionKeys;
        private bool lockCameraView = false;


        private bool isPhysicsControlActive = false;
        private float standingCameraOffset = 20.0f; // Offset from the position of the character to the 'eyes' while the character is standing.        
        private float crouchingCameraOffset = 10.0f; // Offset from the position of the character to the 'eyes' while the character is crouching.        
        private CharacterController characterController;

        public bool LockCameraView
        {
            get { return lockCameraView; }
            set { lockCameraView = value; }
        }
        public bool IsPhysicsControlActive
        {
            get { return isPhysicsControlActive; }
            set { isPhysicsControlActive = value; }
        }

        
        // Gives the character control over the Camera and movement input.        
        public void ActivatePhysics()
        {
            if (!IsPhysicsControlActive)
            {
                IsPhysicsControlActive = true;
                PhysicsEngine.Space.Add(characterController);
                //characterController.Body.Position = (Camera.Position - new Vector3(0, StandingCameraOffset, 0));
                characterController.Body.Position = Camera.Position;
                Camera.CurrentBehavior = Behavior.Physics;
            }
        }

        
        // Returns input control to the Camera.        
        public void DeactivatePhysics()
        {
            if (IsPhysicsControlActive)
            {
                IsPhysicsControlActive = false;
                PhysicsEngine.Space.Remove(characterController);
                Camera.CurrentBehavior = Behavior.Spectator;
            }
        }


        public void Initialize()
        {
            characterController = new CharacterController();
            characterController.HorizontalMotionConstraint.Speed = 40;
            characterController.HorizontalMotionConstraint.CrouchingSpeed = 20;
            characterController.JumpSpeed = 30;
            

            actionKeys = new Dictionary<Actions, Key>();
            actionKeys.Add(Actions.MoveForwards, Key.W);            
            actionKeys.Add(Actions.MoveBackwards, Key.S);            
            actionKeys.Add(Actions.MoveDown, Key.Q);            
            actionKeys.Add(Actions.MoveUp, Key.E);            
            actionKeys.Add(Actions.StrafeRight, Key.D);            
            actionKeys.Add(Actions.StrafeLeft, Key.A);
            actionKeys.Add(Actions.Crouch, Key.LeftShift);
            actionKeys.Add(Actions.Jump, Key.Q);            
        }

        public void Update()
        {
            if (IsPhysicsControlActive)
                HandlePhysicsUserInput();
            else
                HandleUserInput();

            KeyboardInput.Update();
           // if(!LockCameraView)
                MouseInput.Update();
            Camera.Update(LockCameraView);
        }       


        private void HandleUserInput()
        {
            Vector3 direction = new Vector3(0, 0, 0);
            if (KeyboardInput.IsPressed(actionKeys[Actions.MoveForwards]))                            
                direction.Z = -1.0f;
            if (KeyboardInput.IsPressed(actionKeys[Actions.MoveBackwards]))            
                direction.Z = 1.0f;
            if (KeyboardInput.IsPressed(actionKeys[Actions.MoveUp]))
                direction.Y = 1.0f;
            if (KeyboardInput.IsPressed(actionKeys[Actions.MoveDown]))
                direction.Y = -1.0f;
            if (KeyboardInput.IsPressed(actionKeys[Actions.StrafeRight]))
                direction.X = 1.0f;
            if (KeyboardInput.IsPressed(actionKeys[Actions.StrafeLeft]))
                direction.X = -1.0f;

            Camera.MovementDirection = direction;
        }


        private void HandlePhysicsUserInput()
        {            
            Camera.Position = characterController.Body.Position + (characterController.StanceManager.CurrentStance == Stance.Standing ? standingCameraOffset : crouchingCameraOffset) * characterController.Body.OrientationMatrix.Up;
            
            //Collect the movement impulses.
            Vector2 totalMovement = Vector2.Zero;
            Vector3 movementDir;

            if (KeyboardInput.IsPressed(actionKeys[Actions.MoveForwards]))
            {
                movementDir = Camera.Forward;
                totalMovement -= Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
            }
            if (KeyboardInput.IsPressed(actionKeys[Actions.MoveBackwards]))
            {
                movementDir = Camera.Forward;
                totalMovement += Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
            }

            if (KeyboardInput.IsPressed(actionKeys[Actions.StrafeLeft]))
            {
                movementDir = Camera.Left;
                totalMovement += Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
            }
            if (KeyboardInput.IsPressed(actionKeys[Actions.StrafeRight]))
            {
                movementDir = Camera.Right;
                totalMovement += Vector2.Normalize(new Vector2(movementDir.X, movementDir.Z));
            }            
            
            if (totalMovement == Vector2.Zero)
            {
                characterController.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
                Camera.MovementDirection = Vector3.Zero;
            }
            else
            {
                Vector2 md = Vector2.Normalize(totalMovement);
                characterController.HorizontalMotionConstraint.MovementDirection = md;
                Camera.MovementDirection = new Vector3(md.X, 0, md.Y);
            }

            characterController.StanceManager.DesiredStance = KeyboardInput.IsPressed(actionKeys[Actions.Crouch]) ? Stance.Crouching : Stance.Standing;

            //Jumping
            if (KeyboardInput.IsKeyDownOnce(actionKeys[Actions.Jump]))
            {
                characterController.Jump();
            }
            
        }

    }
}
