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
using REMO_Engine_V1._01;

namespace REMO_Engine_V1._01
{
    public interface IMovable
    {
        Point Pos { get; set; }
        void MoveTo(int x, int y, double speed); // (x,y)를 향해 등속운동.
        void MoveByVector(Point v, double speed); // 벡터 v의 방향으로 speed의 속도로 등속운동한다.
    }


    public abstract class Movable //물체의 위치와 위치를 이동시키는 함수를 포함하는 추상클래스입니다.
    {
        public abstract Point Pos { get; set; }
        public void MoveTo(int x, int y, double speed)// (x,y)를 향해 등속운동.
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
            Pos = new Point(Pos.X + (int)(v.X), Pos.Y + (int)(v.Y));
        }
        public void MoveByVector(Point v, double speed)// 벡터 v의 방향으로 speed의 속도로 등속운동한다.
        {
            double N = Method2D.Distance(new Point(0, 0), v);
            int Dis_X = (int)(v.X * speed / N);
            int Dis_Y = (int)(v.Y * speed / N);
            Pos = new Point(Pos.X + Dis_X, Pos.Y + Dis_Y);
        }
    }

    public interface IDrawable
    {
        void Draw();
        void RegisterDrawAct(Action a);
    }

    public interface IBoundable
    {
        Rectangle Bound { get; set; }
    }

    public interface IInitiable//이 클래스를 상속받은 객체는 단 한번 InitOnce 함수를 통해 액션을 호출할 기회를 얻습니다. 그 이후 InitOnce는 작동하지 않습니다. 주로 커스터마이즈된 이니셜라이즈 함수를 구현할 때 쓰게 됩니다.실제 구현은 REMOC을 참조하십시오.
    {
        void InitOnce(Action a);
    }
    public interface IClickable : IBoundable
    {
        void ClickAct();
        void RegisterClickAct(Action a);
        bool ContainsCursor();
        bool CursorClickedThis();
        bool Contains(Point p);
    }

    public abstract class REMOC : IInitiable, IDrawable, IClickable, IMovable //레모의 컨트롤 패턴을 모두 모아놓은 추상 클래스입니다. REMOC(REMO Control의 약자) 또한 각 인터페이스 구현의 예시가 되어줍니다.
    {

        //InitOnce 구현.
        private bool isInited = false;
        public void InitOnce(Action a)
        {
            if (!isInited)
            {
                a();
                isInited = true;
            }

        }
        //IDrawable 구현

        protected Action DrawAction; //나중에 이 항목을 오버라이드 함으로써 디폴트 드로우액션을 달아줄 수 있습니다. 실제 예시는 Gfx를 참조하십시오.
        public void Draw() =>
            DrawAction?.Invoke();
        public void RegisterDrawAct(Action a) //커스터마이즈된 드로우함수를 새로 달아줄 수 있습니다. 이를 통해 쉽게 커스텀함수를 불러올 수 있습니다. 
        {
            DrawAction = a;
        }


        //IClickable 구현

        private Rectangle bound;
        public Rectangle Bound
        {
            get { return bound; }
            set { bound = value; }
        }

        protected Action ClickAction;
        public void ClickAct()
        {
            if (User.JustLeftClicked())
                ClickAction?.Invoke();
        }
        public void RegisterClickAct(Action a) =>
            ClickAction = a;

        public bool ContainsCursor() => Contains(Cursor.Pos);
        public bool Contains(Point p) => Bound.Contains(p);
        public bool CursorClickedThis() => User.JustLeftClicked(Bound);


        //IMovable 구현


        public Point Pos // 물체 범위의 왼쪽 위를 포지션으로 지정한다.
        {
            get { return Bound.Location; }
            set { bound.Location = value; }
        }
        public void MoveTo(int x, int y, double speed)// (x,y)를 향해 등속운동.
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
            Pos = new Point(Pos.X + (int)(v.X), Pos.Y + (int)(v.Y));
        }
        public void MoveByVector(Point v, double speed)// 벡터 v의 방향으로 speed의 속도로 등속운동한다.
        {
            double N = Method2D.Distance(new Point(0, 0), v);
            int Dis_X = (int)(v.X * speed / N);
            int Dis_Y = (int)(v.Y * speed / N);
            Pos = new Point(Pos.X + Dis_X, Pos.Y + Dis_Y);
        }


    }
}
