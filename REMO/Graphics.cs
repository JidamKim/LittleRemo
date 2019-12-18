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
    public class ALReader : TLReader // ALReader는 커스텀 룰이 적용된 TLReader입니다. Animation Language Reader입니다. Animation에 필요한 Tag Language를 분석합니다.
    {
        public bool isLooped = false;
        public List<string> IndicationList = new List<string>();
        public ALReader()
        {
            /* TL을 읽는 두가지 룰을 만듭니다. Tag를 통해 Loop Setting을 구분하고, 지시문 리스트를 만듭니다.
             * 
             * */
            AddRule("Loop", (s) =>
            {
                isLooped = true;
                IndicationList = TLReader.Parse(s, "->").ToList();
            });
            AddRule("ReadOnce", (s) =>
            {
                isLooped = false;
                IndicationList = TLReader.Parse(s, "->").ToList();
            });
        }
    }




    public abstract class Gfx : IMovable, IDrawable, IBoundable// 기본적인 Bound 연산 및 무브먼트 함수를 지원 + 드로우 함수 지정
    {
        private Rectangle bound;
        public Rectangle Bound
        {
            get { return bound; }
            set { bound = value; }
        }
        public Point Pos //왼쪽 위를 포지션으로 지정한다.
        {
            get { return Bound.Location; }
            set { bound.Location = value; }
        }

        public Point Center
        {
            get { return bound.Center; }
            set { bound.Location = new Point(value.X - bound.Width / 2, value.Y - bound.Height / 2); }
        }

        public Point ROrigin; // 회전 중심점. 재할당을 하지 않을 경우 텍스처의 중심이 회전중심이 됩니다. 재할당을 할 때는 물체의 왼쪽 위 지점을 (0,0)이라고 생각해주십시오. 중요한 점은, 실제 텍스처 파일의 가로 세로값을 참조한다는 것입니다.
        public float Rotate;  // 회전각. radian을 따릅니다.


        public void MoveTo(int x, int y, double speed) // (x,y)를 향해 등속운동.
        {
            double N = Method2D.Distance(new Point(x, y), Pos);//두 물체 사이의 거리
            if (N < speed)//거리가 스피드보다 가까우면 도착.
            {
                Pos = new Point(x, y);
                return;
            }

            Vector2 v = new Vector2(x - Pos.X, y - Pos.Y);
            v.Normalize();
            v = new Vector2((float)speed * v.X, (float)speed * v.Y);
            Pos = new Point(Bound.X + (int)(v.X), Bound.Y + (int)(v.Y));
        }

        public void MoveByVector(Point v, double speed) // 벡터 v의 방향으로 speed의 속도로 등속운동한다.
        {
            double N = Method2D.Distance(new Point(0, 0), v);
            int Dis_X = (int)(v.X * speed / N);
            int Dis_Y = (int)(v.Y * speed / N);
            Pos = new Point(Bound.X + Dis_X, Bound.Y + Dis_Y);
        }


        protected Action DrawAction;
        public void Draw()
        {
            if (DrawAction == null)
                Draw(Color.White);
            else
                DrawAction();
        }

        public void RegisterDrawAct(Action a) //Gfx에 커스터마이즈된 드로우함수를 달아줄 수 있습니다. 이를 통해 쉽게 커스텀함수를 불러올 수 있습니다. 
        {
            DrawAction = a;
        }

        public abstract void Draw(params Color[] colors);

        public void DrawAddon(string AddonName, params Color[] cs) //Graphic의 바운드에 새 애드온을 그립니다.
        {
            Game1.Painter.Draw(new Gfx2D(AddonName, Bound), cs);
        }

        public void DrawAddon(string AddonName, Point Vector, Point newSize, params Color[] cs)//Graphic의 position을 기준으로 하는 새 사이즈를 가진 애드온을 그립니다.
        {
            Game1.Painter.Draw(new Gfx2D(AddonName, new Rectangle(Pos + Vector, newSize)), cs);
        }
        public void DrawAddon(string AddonName, Point newSize, params Color[] cs)//Graphic의 position에 새 사이즈를 가진 애드온을 그립니다.
        {
            Game1.Painter.Draw(new Gfx2D(AddonName, new Rectangle(Pos, newSize)), cs);
        }

        public bool Contains(Point p)
        {
            return Bound.Contains(p);
        }
        public bool ContainsCursor()
        {
            return Contains(Cursor.Pos);
        }


        //Animation 처리.

        private class ALReader : TLReader // ALReader는 커스텀 룰이 적용된 TLReader입니다. Animation Language Reader입니다. Animation에 필요한 Tag Language를 분석합니다.
        {
            public bool isLooped = false;
            public List<string> IndicationList = new List<string>();
            public ALReader()
            {
                /* TL을 읽는 두가지 룰을 만듭니다. Tag를 통해 Loop Setting을 구분하고, 지시문 리스트를 만듭니다.
                 * 
                 * */
                AddRule("Loop", (s) =>
                {
                    isLooped = true;
                    IndicationList = TLReader.Parse(s, "->").ToList();
                });
                AddRule("ReadOnce", (s) =>
                {
                    isLooped = false;
                    IndicationList = TLReader.Parse(s, "->").ToList();
                });
            }
        }
        private ALReader aLReader = new ALReader();
        private Point Slicer = new Point(1, 1);
        private string CurrentStatement;
        private int AnimationTimer = 0;

        public void Animate(string statement, int FrameRate) // FrameRate만큼의 Frame이 지나면 스프라이트를 교체하는 형식의 애니메이트를 합니다. Update 함수에서 호출되어야 정상적으로 작동합니다.
        {
            if (CurrentStatement != statement) //Statement가 변경되면
            {
                aLReader.ReadLine(statement);//새로운 지시문 리스트를 만듭니다.
                AnimationTimer = 0;
            }
            else//변경되지 않았을 경우 리스트 내부를 처리합니다.
            {
                if (AnimationTimer == 0)
                {
                    //스프라이트 교체 작업을 실시합니다.(아직 구현 안됨)

                    AnimationTimer = FrameRate;
                }
                else
                {
                    AnimationTimer--;
                }
            }
        }


    }

    public class GfxStr : Gfx
    {
        private string text;
        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                Bound = new Rectangle(Pos, Method2D.VtP(Texture.MeasureString(Text)));
            }
        }
        public int Edge = 0;
        public SpriteFont Texture;

        public GfxStr(string _text) : this(_text, Point.Zero)
        {

        }

        public GfxStr(string _text, Point pos)
        {
            Texture = Game1.content.Load<SpriteFont>("DefaultFont");
            Text = _text;
            Pos = pos;
            Bound = new Rectangle(pos, Method2D.VtP(Texture.MeasureString(Text)));
        }
        public GfxStr(string text, Point pos, int edge)
        {
            Texture = Game1.content.Load<SpriteFont>("DefaultFont");
            Text = text;
            Edge = edge;
            Bound = new Rectangle(pos, Method2D.VtP(Texture.MeasureString(Text)) + new Point(2 * Edge, 2 * Edge));
        }


        public override void Draw(params Color[] colors) => Game1.Painter.Draw(this, colors);
    }

    public class Gfx2D : Gfx
    {
        public Texture2D Texture;
        public string Sprite
        {
            get { return Texture.ToString().Split('.')[1]; }
            set
            {
                Texture = Game1.content.Load<Texture2D>(value);
            }
        }

        public Gfx2D() // 메소드를 사용할 수 없는 Null 객체를 생성한다.
        {

        }
        public Gfx2D(Rectangle boundRect) : this("REMO.WhiteSpace", boundRect) { }

        public Gfx2D(string SpriteName, Rectangle boundRect)
        {
            Texture = Game1.content.Load<Texture2D>(SpriteName);
            Bound = boundRect;
            ROrigin = new Point(Texture.Width / 2, Texture.Height / 2);
        }

        public Gfx2D(string SpriteName, Point p, double r)
        {
            Texture = Game1.content.Load<Texture2D>(SpriteName);
            Bound = new Rectangle(p, new Point((int)(Texture.Width * r), (int)(Texture.Height * r)));
            ROrigin = new Point(Texture.Width / 2, Texture.Height / 2);
        }


        public override void Draw(params Color[] colors) => Game1.Painter.Draw(this, colors);



        public bool RContains(Point p)//만약 Gfx의 rotate값이 0이 아닐 경우, RContains 함수가 정확합니다. 다만, 보시다시피, 비싼 계산을 요구합니다.
        {
            Vector2 v = Method2D.PtV(p);
            Vector3 RO = new Vector3(Pos.X + (ROrigin.X * Bound.Width) / Texture.Width, Pos.Y + (ROrigin.Y * Bound.Height) / Texture.Height, 0);
            v = Vector2.Transform(v, Matrix.CreateTranslation(-RO) * Matrix.CreateRotationZ(-Rotate) * Matrix.CreateTranslation(RO));
            return Bound.Contains(Method2D.VtP(v));
        }
        public bool RContainsCursor()
        {
            return RContains(Cursor.Pos);
        }
    }

    public class Camera2D
    {
        protected float zoom; // Camera Zoom
        public Matrix transform; // Matrix Transform
        public Vector2 pos; // Camera Position
        protected float rotation; // Camera Rotation

        public Camera2D()
        {
            zoom = 1.0f;
            rotation = 0.0f;
            pos = Vector2.Zero;
        }
        public float Zoom
        {
            get { return zoom; }
            set
            {
                zoom = value;
            } // Negative zoom will flip image
        }

        public float Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }

        // Auxiliary function to move the camera
        public void Move(Vector2 amount)
        {
            pos += amount;
        }
        // Get set position
        public Vector2 Pos
        {
            get { return pos; }
            set { pos = value; }
        }
        public Matrix get_transformation(GraphicsDevice graphicsDevice)
        {
            transform = Matrix.CreateTranslation(new Vector3(-pos.X, -pos.Y, 0)) *
                                         Matrix.CreateRotationZ(Rotation) *
                                         Matrix.CreateScale(new Vector3(Zoom, Zoom, 1));
            //Matrix.CreateTranslation(-new Vector3(Game1.graphics.GraphicsDevice.Viewport.Width* (Zoom - 1) * 0.5f, Game1.graphics.GraphicsDevice.Viewport.Height* (Zoom - 1) * 0.5f, 0)); //마지막에 이걸 적용하면 중심에서 확대합니다.
            return transform;
        }


    }

    public static class Projector
    {
        private static event Action UpdateSceneEvent;
        private static event Action DrawSceneEvent;
        private static event Action PausedSceneEvent;

        public static Camera2D MainCamera;

        public static void Update() => UpdateSceneEvent?.Invoke();

        public static void Draw() => DrawSceneEvent?.Invoke();

        public static void Clear()
        {
            UpdateSceneEvent = null;
            DrawSceneEvent = null;
            PausedSceneEvent = null;
            MusicBox.StopSong();
        }

        public static bool Loaded(Scene s)
        {
            return DrawSceneEvent.GetInvocationList().Contains(s.drawEvent);
        }

        public static void Load(params Scene[] scs)
        {
            foreach (Scene s in scs)
            {
                UpdateSceneEvent += s.updateEvent;
                DrawSceneEvent += s.drawEvent;
                s.InitEvent?.Invoke();
                if (s.bgm != "")
                {
                    MusicBox.PlaySong(s.bgm);
                }
            }
        }

        public static void Unload(params Scene[] scs)
        {
            foreach (Scene s in scs)
            {
                UpdateSceneEvent -= s.updateEvent;
                DrawSceneEvent -= s.drawEvent;
                MusicBox.StopSong();
            }
        }

        public static void Pause(params Scene[] scs)
        {
            foreach (Scene s in scs)
            {
                UpdateSceneEvent -= s.updateEvent;
                PausedSceneEvent += s.updateEvent;
            }
        }
        public static void Resume(params Scene[] scs)
        {
            foreach (Scene s in scs)
            {
                PausedSceneEvent -= s.updateEvent;
                UpdateSceneEvent += s.updateEvent;
            }

        }

        public static void PauseAll()
        {
            PausedSceneEvent = UpdateSceneEvent;
            UpdateSceneEvent = null;
        }

        public static void ResumeAll()
        {
            UpdateSceneEvent += PausedSceneEvent;
            PausedSceneEvent = null;
        }


        public static void SwapTo(params Scene[] scs)
        {
            Clear();
            Load(scs);
        }

        public static void DrawInScreen(Viewport Screen, Action Act)
        {
            Viewport Temp = Game1.graphics.GraphicsDevice.Viewport;
            Game1.graphics.GraphicsDevice.Viewport = Screen;
            Act();
            Game1.graphics.GraphicsDevice.Viewport = Temp;

        }


    }

    public class Scene
    {
        public Camera2D Camera = new Camera2D();
        public string bgm = "";
        public Action updateEvent;
        public Action drawEvent;
        public Action InitEvent;

        public Scene() { }

        public Scene(Action UEvent, Action DEvent) => SetEvent(UEvent, DEvent);
        public Scene(Action IEv, Action UEvent, Action DEvent)
        {
            SetEvent(UEvent, DEvent);
            InitEvent = IEv;
        }


        public Scene(Camera2D SceneCam, Action UEvent, Action DEvent)
        {
            Camera = SceneCam;
            SetEvent(UEvent, DEvent);
        }
        public Scene(Camera2D SceneCam, Action IEv, Action UEvent, Action DEvent)
        {
            Camera = SceneCam;
            InitEvent = IEv;
            SetEvent(UEvent, DEvent);
        }



        public void SetEvent(Action UEvent, Action DEvent)
        {
            ClearEvent();
            updateEvent = UEvent;
            drawEvent = () =>
            {
                Game1.Painter.OpenCanvas(Camera,
                    () =>
                    {
                        DEvent();
                    });
            };
        }

        public void ClearEvent()
        {
            updateEvent = null;
            drawEvent = null;
        }



    }


    public static class Method2D
    {
        public static Point VtP(Vector2 v) // Vector2 to Point
        {
            return new Point((int)v.X, (int)v.Y);
        }

        public static Vector2 PtV(Point p) // Point to Vector2
        {
            return new Vector2(p.X, p.Y);
        }

        public static double Distance(Point p1, Point p2)
        {
            Vector2 v = new Vector2(p1.X - p2.X, p1.Y - p2.Y);
            return v.Length();
        }

        //포인트 a,b를 r:1-r로 내분.
        public static Point DivPoint(Point a, Point b, double r)
        {
            double x = a.X * r + b.X * (1 - r);
            double y = a.Y * r + b.Y * (1 - r);
            return new Point((int)x, (int)y);
        }
        public static Point Multiply(Point a, double k)
        {
            return new Point((int)(a.X * k), (int)(a.Y * k));
        }

        //Origin을 중심으로 줌과 회전변환을 하는 변환매트릭스.
        public static Matrix Matrix2D(Point Origin, float Rotate, float Zoom)
        {
            Matrix translateToOrigin = Matrix.CreateTranslation(-Origin.X, -Origin.Y, 0);
            Matrix rotationMatrix = Matrix.CreateRotationZ(MathHelper.ToRadians(Rotate));
            Matrix zoomMatrix = Matrix.CreateScale(Zoom);
            Matrix translateBackToPosition = Matrix.CreateTranslation(Origin.X, Origin.Y, 0);

            Matrix compositeMatrix = translateToOrigin * rotationMatrix * zoomMatrix * translateBackToPosition;

            return compositeMatrix;
        }
    }



    public static class Fader // Fade 관련 애니메이션 처리
    {
        //Flicker : 깜빡깜빡거리는 액션 처리할때 쓰는 인자(Coefficient)
        public static float Flicker(int Timer, int FlickTime)
        {
            return (Math.Max(Timer % FlickTime, FlickTime - Timer % FlickTime) - FlickTime / 2) / (float)(FlickTime / 2);
        }
        public static float Flicker(int FlickTime)
        {
            return Flicker(StandAlone.FrameTimer, FlickTime);
        }

        public static Dictionary<Color, Dictionary<Gfx, int>> FadeAnimations = new Dictionary<Color, Dictionary<Gfx, int>>();
        public static Dictionary<Color, int> FadeAnimationTimers = new Dictionary<Color, int>();
        public static List<Color> ExcludedColorSets = new List<Color>();

        public static void Add(Gfx g, int Timer, Color c)
        {
            if (FadeAnimations.ContainsKey(c))
            {
                FadeAnimations[c].Add(g, Timer);
            }
            else
            {
                FadeAnimations.Add(c, new Dictionary<Gfx, int>());
                FadeAnimations[c].Add(g, Timer);
                FadeAnimationTimers.Add(c, Timer);
            }
        }

        public static void Exclude(Color c)
        {
            if (!ExcludedColorSets.Contains(c))
                ExcludedColorSets.Add(c);
        }

        public static void Clear(Color c)
        {
            if (FadeAnimations.ContainsKey(c))
            {
                FadeAnimations[c].Clear();
            }
        }

        public static void Update()
        {
            foreach (Color c in FadeAnimations.Keys.ToList())
            {
                foreach (Gfx g in FadeAnimations[c].Keys.ToList())
                {
                    if (FadeAnimations[c][g] == 0)
                    {
                        FadeAnimations[c].Remove(g);
                    }
                    else
                        FadeAnimations[c][g]--;
                }
            }
        }

        public static void Draw(Color c)
        {
            if (FadeAnimations.ContainsKey(c))
            {
                foreach (Gfx g in FadeAnimations[c].Keys)
                {
                    g.Draw(c * ((float)FadeAnimations[c][g] / FadeAnimationTimers[c]));
                }
            }
        }

        public static void DrawAll()
        {
            foreach (Color c in FadeAnimations.Keys)
            {
                if (!ExcludedColorSets.Contains(c))
                    Draw(c);
            }
        }

    }

    public static class Filter
    {
        private static Gfx2D Lighter;
        public static void Absolute(Rectangle Bound, Color c)
        {
            Lighter = new Gfx2D("WhiteSpace", Bound);
            Lighter.Draw(c);
        }
        public static void Absolute(Gfx g, Color c) => Absolute(g.Bound, c);


        public static void Vignette(Rectangle Bound, float opacity)
        {
            Lighter = new Gfx2D("Light", Bound);
            Lighter.Draw(Color.White * opacity);
        }
        public static void Vignette(Gfx g, float opacity) => Vignette(g.Bound, opacity);


    }

}
