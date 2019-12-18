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
{   //각종 StandAlone계열 클래스들을 담당. 게임 씬과 무관한 기본 기능들을 수행하는 컴포넌트들을 처리합니다.
    public static class StandAlone
    {


        public static int FrameTimer = 0;//지나간 프레임수를 측정합니다.
        public static int ElapsedMillisec = 0; // 한 프레임에서 다음 프레임으로 넘어가는 밀리초를 측정합니다.

        public static void InternalUpdate() // 이것은 함부로 불러서는 안됩니다.
        {
            FrameTimer++;
            Fader.Update();
            MusicBox.Update();
        }

        public static void InternalDraw() // 이것은 함부로 불러서는 안됩니다.
        {
            Game1.Painter.OpenCanvas(() =>
            {
                Fader.DrawAll();
            });
        }

        public static void DrawString(string s, Point p, Color c)
        {
            GfxStr t = new GfxStr(s, p);
            t.Draw(c);
        }
        public static void DrawString(string s, Point p, Color c, Color BackGroundColor)
        {
            GfxStr t = new GfxStr(s, p, 3);
            Filter.Absolute(t, BackGroundColor);
            t.Draw(c);
        }


        public static Rectangle FullScreen //현재 게임 풀스크린에 대한 정보를 얻어옵니다.
        {
            get { return new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height); }
        }

        public static void DrawFullScreen(string SpriteName)
        {
            Gfx2D g = new Gfx2D(SpriteName, FullScreen);
            g.Draw();
        }


        public static void DrawFullScreen(string SpriteName, Color c)
        {
            Gfx2D g = new Gfx2D(SpriteName, FullScreen);
            g.Draw(c);
        }


        private static Random random = new Random();
        public static int Random(int x, int y)
        {
            return random.Next(Math.Min(x, y), Math.Max(x, y));
        }

        public static double Random()
        {
            return random.NextDouble();
        }
        public static T RandomPick<T>(List<T> Ts) //리스트 아이템 중 한개를 랜덤하게 픽합니다.
        {
            double r = StandAlone.Random();
            double m = 1.0 / Ts.Count;
            for (int i = 0; i < Ts.Count; i++)
            {
                if (r >= m * i && r < m * (i + 1))
                {
                    return Ts[i];
                }
            }
            return Ts[0];
        }

    }
}
