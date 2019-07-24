using System;
using System.Collections.Generic;
using SlimDX;
using SlimDX.Direct3D10;
using WeatherGame.Framework.Mesh3d;
using WeatherGame.Framework.Weather;
using WeatherGame.RenderLoop;
using WeatherGame.Framework.Objects;
using WeatherGame.Framework.World;

namespace WeatherGame.Framework.Rendering
{
    public static class BloomEffect
    {
        public static bool IsOn = true;

        public static BloomSetting settings = new BloomSetting();

        public static void SetSettings(BloomSetting s)
        {
            if (s == null) s = new BloomSetting();
            settings = s;
        }

        public static void Draw(int? w, int? h, bool wpf, bool fullScreen)
        {
            if (!SkyDome.UseBloom || !IsOn) return;
            return;
            /*
            Shader effect = WorldData.GetObject("Bloom.fx") as Shader;
            effect.GetVar("BloomThreshold").AsScalar().Set(settings.BloomThreshold);
            effect.GetVar("BloomIntensity").AsScalar().Set(settings.BloomIntensity);
            effect.GetVar("BaseIntensity").AsScalar().Set(settings.BaseIntensity);
            effect.GetVar("BloomSaturation").AsScalar().Set(settings.BloomSaturation);
            effect.GetVar("BaseSaturation").AsScalar().Set(settings.BaseSaturation);

            if (!fullScreen)
            {
                Game.Device.ClearRenderTargetView(DeferredRenderer.effectRT1.rtv, new SlimDX.Color4(1.0f, 0, 0, 0));
                Game.Device.ClearRenderTargetView(DeferredRenderer.effectRT2.rtv, new SlimDX.Color4(1.0f, 0, 0, 0));
                if (wpf) DeferredRenderer.FillShaderMaterials(effect);
                EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderBloomExtract");
                EffectPass pass = tech.GetPassByIndex(0);
                InputLayout layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.FullScreenQuad, pass.Description.Signature);
                Game.Device.OutputMerger.SetTargets(DeferredRenderer.effectRT1.rtv);
                if (w.HasValue && h.HasValue) Game.Device.Rasterizer.SetViewports(new Viewport(0, 0, w.Value, h.Value, 0.0f, 1.0f));
                Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
                Game.Device.InputAssembler.SetInputLayout(layout);
                effect.GetVar("colorMap").AsResource().SetResource(DeferredRenderer.baseRT.srv);
                effect.GetVar("positionMap").AsResource().SetResource(DeferredRenderer.positionRT.srv);
                effect.GetVar("glowMap").AsResource().SetResource(DeferredRenderer.finalRT.srv);
                pass.Apply();
                Game.Device.Draw(3, 0);
                DeferredRenderer.UnbindMRT(pass, effect.GetVar("colorMap").AsResource(), effect.GetVar("positionMap").AsResource(), effect.GetVar("glowMap").AsResource());


                tech = effect.EffectObj.GetTechniqueByName("RenderBlur");
                pass = tech.GetPassByIndex(0);
                layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.FullScreenQuad, pass.Description.Signature);
                Game.Device.OutputMerger.SetTargets(DeferredRenderer.effectRT2.rtv);
                if (w.HasValue && h.HasValue) Game.Device.Rasterizer.SetViewports(new Viewport(0, 0, w.Value, h.Value, 0.0f, 1.0f));
                Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
                Game.Device.InputAssembler.SetInputLayout(layout);
                effect.GetVar("colorMap").AsResource().SetResource(DeferredRenderer.baseRT.srv);
                effect.GetVar("positionMap").AsResource().SetResource(DeferredRenderer.positionRT.srv);
                effect.GetVar("glowMap").AsResource().SetResource(DeferredRenderer.effectRT1.srv);
                List<Vector4> SampleVals1 = EffectHelpers.SetBlurEffectParameters(effect, 1.0f / (float)DeferredRenderer.ScreenWidth, 0, 15, settings.BlurAmount);
                List<Vector4> SampleVals2 = EffectHelpers.SetBlurEffectParameters(effect, 0, 1.0f / (float)DeferredRenderer.ScreenHeight, 15, settings.BlurAmount);
                effect.GetVar("SampleVals1").AsVector().Set(SampleVals1.ToArray());
                effect.GetVar("SampleVals2").AsVector().Set(SampleVals2.ToArray());
                pass.Apply();
                Game.Device.Draw(3, 0);

                DeferredRenderer.UnbindMRT(pass, effect.GetVar("colorMap").AsResource(), effect.GetVar("positionMap").AsResource(), effect.GetVar("glowMap").AsResource());
            }
            else
            {
                EffectTechnique tech = effect.EffectObj.GetTechniqueByName("RenderFinal");
                EffectPass pass = tech.GetPassByIndex(0);
                InputLayout layout = ShaderHelper.ConstructInputLayout(MeshInputElements10.FullScreenQuad, pass.Description.Signature);
                Game.Device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
                Game.Device.InputAssembler.SetInputLayout(layout);
                effect.GetVar("colorMap").AsResource().SetResource(DeferredRenderer.finalRT.srv);
                effect.GetVar("positionMap").AsResource().SetResource(DeferredRenderer.positionRT.srv);
                effect.GetVar("glowMap").AsResource().SetResource(DeferredRenderer.effectRT2.srv);
                pass.Apply();
                Game.Device.Draw(3, 0);

                DeferredRenderer.UnbindMRT(pass, effect.GetVar("colorMap").AsResource(), effect.GetVar("positionMap").AsResource(), effect.GetVar("glowMap").AsResource());
            }
             */
        }
    }
  

    internal static class EffectHelpers
    {

        internal static List<Vector4> SetBlurEffectParameters(Shader effect, float dx, float dy, int sampleCount, float blurAmount)
        {
            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0, blurAmount);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1, blurAmount);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.          
            List<Vector4> SampleVals = new List<Vector4>(sampleCount);
            for (int i = 0; i < sampleCount; i++)
            {
                SampleVals.Add(new Vector4(sampleOffsets[i].X, sampleOffsets[i].Y, sampleWeights[i], 1));
            }
            return SampleVals;
        }
        internal static float ComputeGaussian(float n, float blurAmount)
        {
            float theta = blurAmount;
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }
    }
}
