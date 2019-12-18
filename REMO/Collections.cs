using System;
using System.Collections.Generic;
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
    public class Aligned<T> where T : IMovable, IDrawable, IBoundable  // 일렬로 정렬된 객체들을 포함하는 콜렉션입니다.
    {
        public List<T> Components = new List<T>();
        public Point Origin = Point.Zero;//집합을 정렬할 기준점입니다. 
        public Point RV = Point.Zero;//기준점에 대한 상대벡터입니다.
        public Point Interval;//각 컴포넌트간 간격을 설정하는 벡터입니다.


        public T this[int i]
        {
            get => Components[i];
            set => value = Components[i];
        }

        public int Count
        {
            get => Components.Count;
        }

        public void RemoveAt(int i) => Components.RemoveAt(i);
        public int IndexOf(T t) => Components.IndexOf(t);
        public void Clear() => Components.Clear();

        public bool MouseIsOnComponents //마우스가 컴포넌트들 위에 있는지를 체크합니다.
        {
            get => OneContainsCursor() != null;
        }
        public int ClickedIndex//방금 클릭된 컴포넌트의 인덱스를 반환합니다. 컴포넌트가 클릭되지 않았을 경우 -1, 혹은 컴포넌트의 인덱스(0부터 시작)를 반환합니다.
        {
            get => IndexOf(OneJustClicked());
        }



        public Aligned(Point rv, Point interval)//상대벡터와 간격벡터를 설정합니다. 이후 Origin을 조정하여 특정 컴포넌트에 매달 수 있습니다.
        {
            RV = rv;
            Interval = interval;
        }

        public Aligned(Point rv, Point interval, List<T> TList)
        {
            RV = rv;
            Interval = interval;
            Components = TList;
        }

        public void Add(T t)
        {
            Components.Add(t);
        }


        public void Align() //컴포넌트들이 즉시 정렬됩니다.
        {
            Point temp = Origin + RV;
            for (int i = 0; i < Count; i++)
            {
                this[i].Pos = temp;
                temp += Interval;
            }
        }

        public void LazyAlign(int AlignSpeed) // 컴포넌트들이 정해진 스피드에 맞춰 느긋하게 정렬됩니다.
        {
            Point temp = Origin + RV;
            for (int i = 0; i < Count; i++)
            {
                this[i].MoveTo(temp.X, temp.Y, AlignSpeed);
                temp += Interval;
            }

        }

        public void AttachTo(Point Target, Point rv) // Target의 위치에 상대적으로 RV에 있는 위치로 고정됩니다.
        {
            Origin = Target;
            RV = rv;
            Align();
        }

        public void AttachTo(Point Target)//Target에 고정됩니다.
        {
            Origin = Target;
            Align();
        }

        public T Pick(Func<T, bool> FilterCondition) // 특정 Condition을 만족하는 객체를 뽑아냅니다. O(n)이므로 유의하여 사용하는 것이 좋습니다.
        {
            for (int i = 0; i < Count; i++)
            {
                if (FilterCondition(this[i]))
                    return this[i];
            }
            return default(T);
        }

        public T OneContainsCursor() // 커서가 놓여있는 객체를 뽑아냅니다. O(n)이므로 유의하여 사용하는 것이 좋습니다. 반환값이 null일 수 있으므로 . 대신 ?. operator를 씁시다.
        {
            return Pick((t) =>
            {
                return t.Bound.Contains(Cursor.Pos);
            });
        }
        public T OneJustClicked() // 막 클릭된 객체를 뽑아냅니다. O(n)이므로 유의하여 사용하는 것이 좋습니다. 반환값이 null일 수 있으므로 . 대신 ?. operator를 씁시다.
        {
            return Pick((t) =>
            {
                return t.Bound.Contains(Cursor.Pos) && User.JustLeftClicked();
            });
        }



        public void Draw()
        {
            for (int i = 0; i < Count; i++)
                this[i].Draw();
        }

    }

}
