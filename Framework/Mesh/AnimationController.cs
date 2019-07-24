using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using WeatherGame.RenderLoop;
using WeatherGame.Framework.Objects;

namespace WeatherGame.Framework.Mesh3d
{
    public class AnimationController
    {
        private bool loopAnimation = false;
        private bool isPlaying = false;

        private float currTime = 0;
        private Mesh3DAnimation currAnimation = null;
        private float animSpeed = 1.0f;
        private Matrix[] animMat = null;


        private GameObjectReference objRef = null;        
        private List<Mesh3DAnimation> animations = null;

        public AnimationController(GameObjectReference objRef)
        {
            this.objRef = objRef;
            Model model = this.objRef.BaseGameObject as Model;
            animations = model.Mesh3d.animations;
        }

        public List<Mesh3DAnimation> Animations
        {
            get { return animations; }
        }
        public bool IsPlaying
        {
            get { return isPlaying; }            
        }
        public Matrix[] AnimationTransform
        {
            get { return animMat; }
        }

        public float Speed
        {
            get { return animSpeed; }
            set { animSpeed = value; }
        }

        public void playAnimation(string name, bool loop)
        {
            loopAnimation = loop;
            isPlaying = true;
            currTime = 0;
            foreach (Mesh3DAnimation a in animations)
            {
                if (a.animName == name)
                {
                    currAnimation = a;
                    break;
                }
            }

            if (currAnimation == null)
            {
                stopAnimation();
                return;
            }

            animMat = new Matrix[currAnimation.channels.Length];
        }

        public void stopAnimation()
        {
            isPlaying = false;
            loopAnimation = false;
            currTime = 0;
            currAnimation = null;
            animMat = null;
        }

        public void update()
        {
            if (isPlaying == false || currAnimation == null) return;
            
            currTime += (Game.Time.FramesPerSecond * animSpeed);
            if (currTime > currAnimation.duration) currTime = 0;


            for (int i = 0; i < currAnimation.channels.Length; i++)
            {
                AnimationChannel ac = currAnimation.channels[i];
                int frame = 0;
                Matrix presentTransform = Matrix.Identity;

                while (frame < ac.animKeys.Length - 1)
                {
                    if (currTime < ac.animKeys[frame + 1].time)
                        break;
                    frame++;
                }


                // interpolate between this frame's value and next frame's value
                int nextFrame = (frame + 1) % ac.animKeys.Length;
                AnimationKey key = ac.animKeys[frame];
                AnimationKey nextKey = ac.animKeys[nextFrame];
                double diffTime = nextKey.time - key.time;
                if (diffTime < 0.0)
                    diffTime += currAnimation.duration;
                if (diffTime > 0)
                {
                    float factor = (float)((currTime - key.time) / diffTime);
                    presentTransform = key.animMat + (nextKey.animMat - key.animMat) * factor;
                }
                else
                {
                    presentTransform = key.animMat;
                }

                animMat[i] = presentTransform;
            }

        }
    }
}
