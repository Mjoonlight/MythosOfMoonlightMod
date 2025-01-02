using MythosOfMoonlight.Common.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Effects;

namespace MythosOfMoonlight.Common.Systems
{
    internal class VFXManager : ModSystem
    {
        public static RenderTarget2D screen, bloom1, bloom2, render;

        public static List<Action> DrawCache = [];

        public static void CreateRender()
        {
            if (Main.dedServ)
                return;

            GraphicsDevice gd = Main.instance.GraphicsDevice;

            screen?.Dispose();
            bloom1?.Dispose();
            bloom2?.Dispose();
            render?.Dispose();

            int width = gd.PresentationParameters.BackBufferWidth;
            int height = gd.PresentationParameters.BackBufferHeight;

            SurfaceFormat backBufferFormat = gd.PresentationParameters.BackBufferFormat;

            screen = new RenderTarget2D(gd, width, height, false, backBufferFormat, DepthFormat.None);
            render = new RenderTarget2D(gd, width, height, false, backBufferFormat, DepthFormat.None);
            bloom1 = new RenderTarget2D(gd, width / 3, height / 3, false, backBufferFormat, DepthFormat.None);
            bloom2 = new RenderTarget2D(gd, width / 3, height / 3, false, backBufferFormat, DepthFormat.None);
        }

        private static bool HasBloom()
        {
            bool flag = false;

            if (!MythosOfMoonlightConfig.Instance.CanBloomBeUsed)
                return false;

            if (DrawCache.Count != 0)
                flag = true;

            return flag;
        }

        private static void DrawBloom(SpriteBatch sb)
        {
            foreach (Action a in DrawCache)
            {
                a?.Invoke();
            }

            DrawCache?.Clear();
        }

        private static void UseBloom(GraphicsDevice graphicsDevice)
        {
            if (HasBloom())
            {
                graphicsDevice.SetRenderTarget(Main.screenTargetSwap);
                graphicsDevice.Clear(Color.Black);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                Main.spriteBatch.End();

                graphicsDevice.SetRenderTarget(screen);
                graphicsDevice.Clear(Color.Black);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                DrawBloom(Main.spriteBatch);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

                MythosOfMoonlight.Bloom2.Parameters["uScreenResolution"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight) / 3f);
                MythosOfMoonlight.Bloom2.Parameters["uRange"].SetValue(1.05f); //1.2f
                MythosOfMoonlight.Bloom2.Parameters["uIntensity"].SetValue(1.3f); //1.15f

                for (int i = 0; i < 2; i++)
                {
                    MythosOfMoonlight.Bloom2.CurrentTechnique.Passes["GlurV"].Apply();
                    graphicsDevice.SetRenderTarget(bloom1);
                    graphicsDevice.Clear(Color.Black);
                    Main.spriteBatch.Draw(screen, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
                    MythosOfMoonlight.Bloom2.CurrentTechnique.Passes["GlurH"].Apply();
                    graphicsDevice.SetRenderTarget(bloom2);
                    graphicsDevice.Clear(Color.Black);
                    Main.spriteBatch.Draw(bloom1, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
                }

                Main.spriteBatch.End();

                graphicsDevice.SetRenderTarget(Main.screenTarget);
                graphicsDevice.Clear(Color.Black);

                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);

                Main.spriteBatch.Draw(Main.screenTargetSwap, Vector2.Zero, Color.White);
                Main.spriteBatch.Draw(bloom2, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

                Main.spriteBatch.End();
            }

            else return;
        }

        public override void Load()
        {
            if (!Main.dedServ)
            {
                Main.OnResolutionChanged += Main_OnResolutionChanged;
            }

            On_FilterManager.EndCapture += On_FilterManager_EndCapture;

            Main.RunOnMainThread(() => { CreateRender(); });

            DrawCache = [];
        }

        public override void Unload()
        {
            DrawCache?.Clear();
            DrawCache = null;
        }

        private void On_FilterManager_EndCapture(On_FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            GraphicsDevice graphicsDevice = Main.instance.GraphicsDevice;

            UseBloom(graphicsDevice);

            orig.Invoke(self, finalTexture, screenTarget1, screenTarget2, clearColor);
        }

        private void Main_OnResolutionChanged(Vector2 obj) => CreateRender();
    }
}
