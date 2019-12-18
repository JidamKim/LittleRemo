
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
namespace REMO_Engine_V1._01
{
    public static class Repository
    {
        //다른 클래스로 편입되기 전의 임시 그래픽스 혹은 각종 인수를 선언합니다.

        public static Gfx2D sqr = new Gfx2D(new Rectangle(200,200,50,50));

    }
    
    public class Game1 : Game
    {
        #region Modified_Game1_SourceCode
        public static GraphicsDeviceManager graphics;
        public static LocalizedContentManager content;
        public static class Painter
        {
            public static SpriteBatch spriteBatch;
            //private static bool isBegined = false;
            public static void Init() => spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            private static void BeginCanvas() =>
                spriteBatch.Begin(SpriteSortMode.Immediate,
                  BlendState.AlphaBlend,
                  null,
                  null,
                  null,
                  null,
                  null);
            private static void BeginCanvas(Camera2D CanvasCam) =>
                                spriteBatch.Begin(SpriteSortMode.Immediate,
                  BlendState.AlphaBlend,
                  null,
                  null,
                  null,
                  null,
                   CanvasCam.get_transformation(Game1.graphics.GraphicsDevice));
            private static void CloseCanvas() => spriteBatch.End();

            public static void Draw(Gfx2D gfx) => Draw(gfx, Color.White);
            public static void Draw(Gfx2D gfx, Color c) => spriteBatch.Draw(gfx.Texture, new Rectangle(gfx.Pos.X + (gfx.ROrigin.X * gfx.Bound.Width) / gfx.Texture.Width, gfx.Pos.Y + (gfx.ROrigin.Y * gfx.Bound.Height) / gfx.Texture.Height, gfx.Bound.Width, gfx.Bound.Height), null, c, gfx.Rotate, Method2D.PtV(gfx.ROrigin), SpriteEffects.None, 0);
            public static void Draw(Gfx2D gfx, params Color[] cs)
            {
                for (int i = 0; i < cs.Length; i++)
                    Draw(gfx, cs[i]);
            }
            public static void Draw(GfxStr gfx) => spriteBatch.DrawString(gfx.Texture, gfx.Text, new Vector2(gfx.Pos.X + gfx.Edge, gfx.Pos.Y + gfx.Edge), Color.Black);
            public static void Draw(GfxStr gfx, Color c) => spriteBatch.DrawString(gfx.Texture, gfx.Text, new Vector2(gfx.Pos.X + gfx.Edge, gfx.Pos.Y + gfx.Edge), c);
            public static void Draw(GfxStr gfx, params Color[] cs)
            {
                for (int i = 0; i < cs.Length; i++)
                    Draw(gfx, cs[i]);
            }
            public static void ClearBackground(Color c) => graphics.GraphicsDevice.Clear(c);

            public static void OpenCanvas(Action a) // 캔버스 열고 닫기를 신경쓰지 않도록 만든 액션 실행 함수입니다.
            {
                BeginCanvas();
                a();
                CloseCanvas();
            }

            public static void OpenCanvas(Camera2D c, Action a)
            {
                BeginCanvas(c);
                a();
                CloseCanvas();

            }
        }

        public class LocalizedContentManager : ContentManager
        {
            public LocalizedContentManager(IServiceProvider serviceProvider, string rootDirectory, CultureInfo currentCulture, string languageCodeOverride) : base(serviceProvider, rootDirectory)
            {
                this.CurrentCulture = currentCulture;
                this.LanguageCodeOverride = languageCodeOverride;
            }

            public LocalizedContentManager(IServiceProvider serviceProvider, string rootDirectory) : this(serviceProvider, rootDirectory, Thread.CurrentThread.CurrentCulture, null)
            {
            }

            public static string ProcessContentString(string s)
            {
                if (s.Contains("."))
                    return s;
                else
                {
                    for (int i = 0; i < GAMEOPTION.NameSpaces.Length; i++)
                    {
                        if (Game1.content.assetExists(GAMEOPTION.NameSpaces[i] + "." + s))
                        {
                            return GAMEOPTION.NameSpaces[i] + "." + s;
                        }
                    }
                    return s;
                }
            }

            public override T Load<T>(string assetName)
            {
                assetName = ProcessContentString(assetName);
                string localizedAssetName = assetName + "." + this.languageCode();
                if (this.assetExists(localizedAssetName))
                {
                    return base.Load<T>(localizedAssetName);
                }
                return base.Load<T>(assetName);

            }

            private string languageCode()
            {
                if (this.LanguageCodeOverride != null)
                {
                    return this.LanguageCodeOverride;
                }
                return this.CurrentCulture.TwoLetterISOLanguageName;
            }

            public bool assetExists(string assetName)
            {
                return File.Exists(Path.Combine(base.RootDirectory, assetName + ".xnb"));
            }

            public string LoadString(string path, params object[] substitutions)
            {
                string assetName;
                string key;
                this.parseStringPath(path, out assetName, out key);
                Dictionary<string, string> strings = this.Load<Dictionary<string, string>>(assetName);
                if (!strings.ContainsKey(key))
                {
                    strings = base.Load<Dictionary<string, string>>(assetName);
                }
                return string.Format(strings[key], substitutions);
            }

            private void parseStringPath(string path, out string assetName, out string key)
            {
                int i = path.IndexOf(':');
                if (i == -1)
                {
                    throw new ContentLoadException("Unable to parse string path: " + path);
                }
                assetName = path.Substring(0, i);
                key = path.Substring(i + 1, path.Length - i - 1);
            }

            public LocalizedContentManager CreateTemporary()
            {
                return new LocalizedContentManager(base.ServiceProvider, base.RootDirectory, this.CurrentCulture, this.LanguageCodeOverride);
            }

            public CultureInfo CurrentCulture;
            public string LanguageCodeOverride;
        }

        public static bool GameExit = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }


        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Painter.Init();
            Game1.content = new LocalizedContentManager(base.Content.ServiceProvider, base.Content.RootDirectory);
            CustomInit();
        }

        protected override void UnloadContent()
        {

        }

        #endregion
        #region Engine Update&Draw
        protected override void Update(GameTime gameTime)
        {
            if (GameExit)
                Exit();

            Projector.Update();
            StandAlone.InternalUpdate();
            CustomUpdate();
            User.Update();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            StandAlone.ElapsedMillisec = gameTime.ElapsedGameTime.Milliseconds;

            Projector.Draw();
            StandAlone.InternalDraw();
            CustomDraw();
            base.Draw(gameTime);
        }

        #endregion


        

        protected void CustomInit()
        {
        }

        protected void CustomUpdate()
        {
            /*
            if(User.Pressing(Keys.Up))
            {
                Repository.sqr.MoveByVector(new Point(0, -1), 5);
            }
            if (User.Pressing(Keys.Down))
            {
                Repository.sqr.MoveByVector(new Point(0, 1), 5);
            }
            if (User.Pressing(Keys.Left))
            {
                Repository.sqr.MoveByVector(new Point(-1, 0), 5);
            }
            if (User.Pressing(Keys.Right))
            {
                Repository.sqr.MoveByVector(new Point(1, 0), 5);
            }
            */           
            User.ArrowKeyPAct((p) =>
            {
                Repository.sqr.MoveByVector(p, 5);
            });

        }
        protected void CustomDraw()
        {
            Painter.OpenCanvas(
                () =>
                {
                    Painter.ClearBackground(Color.Black);
              
                    Fader.DrawAll();
                    Repository.sqr.Draw(Color.DarkMagenta);//
                    if(User.Pressing(Keys.LeftControl,Keys.Q))
                    {
                        StandAlone.DrawString(Fader.Flicker(100).ToString(), Cursor.Pos+new Point(0,20), Color.White,Color.Red);

                    }
                    Cursor.Draw(Color.White);
                });


        }
    }




    public static class GAMEOPTION
    {
        // 게임의 옵션을 지정합니다. 씬의 초기화와는 개념이 다릅니다.

        public static string[] NameSpaces = { "REMO" }; //작업중인 에셋들의 네임스페이스 이름들을 적어넣습니다. 나중에 에셋을 분류하기 용이합니다.







    }






}
