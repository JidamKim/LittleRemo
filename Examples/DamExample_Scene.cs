
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

namespace REMOEngine.Examples
{
    //Template for new Scene. 나중에 커스텀 항목 템플릿을 통해 Scene을 만들 수 있게 계획하겠음.
    public static class DamExam_MainScene
    {



        //Space for Scene properties

        private static Gfx2D PhoneScreen = new Gfx2D("PhoneScreen00", new Point(0,0), 0.5f);
        private static Rectangle PhoneScreenBound = new Rectangle(190, 72, 260, 456);
        private static Rectangle PhoneTouchBound = new Rectangle(216, 144, 79, 45);
        public static Rectangle WholeScreen=new Rectangle(0,0,640,640);


        private static Point ThumnailOrigin = new Point(191, 303);
        private static int ThumnailWidth = 90;

        private static Point[] Thumnails = new Point[] { new Point(0, 0), new Point(1, 0), new Point(2, 0), new Point(0, 1) };

        private static int OldPhoneScreenNum = 0;

     

        private static int GetIndex(string s)
        {
            return Int32.Parse(s.Substring(s.Length - 2, 2));
        }
        private static int GetScreenIndex()
        {
            return GetIndex(PhoneScreen.Sprite);
        }

        private static Point GetThumnailIndex(Point p)
        {
            return new Point((p.X - ThumnailOrigin.X) / ThumnailWidth, (p.Y - ThumnailOrigin.Y) / ThumnailWidth);
        }

        private static void PhoneShow(int number)
        {
            OldPhoneScreenNum = GetScreenIndex();
            PhoneScreen.Sprite = "PhoneScreen0" + number;
        }

        public static void ShowFail(int number)
        {
            DamExam_FailScene.FailNumber = number;
            Projector.PauseAll();
            Projector.Load(DamExam_FailScene.scn);
        }
        
      

        //Write your own Update&Draw Action in here        
        public static Scene scn = new Scene(
            ()=>
            {
                PhoneShow(0);
                scn.Camera.Zoom = (float)StandAlone.FullScreen.Width / WholeScreen.Width;
            },
            () =>
            {
                switch (GetScreenIndex())
                {
                    case 0:
                        if (User.JustLeftClicked(PhoneScreenBound))
                            PhoneShow(1);
                        break;
                    case 1:
                        if (User.JustLeftClicked(PhoneTouchBound))
                        {
                            PhoneShow(2);
                        }
                        break;
                    case 2:
                        if (User.JustLeftClicked())
                        {
                            Point ThmIndex = GetThumnailIndex(Cursor.Pos);
                            if (Thumnails.Contains(ThmIndex))
                                PhoneShow(3 + ThmIndex.X + ThmIndex.Y * 3);
                        }
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                        if (User.JustLeftClicked())
                            PhoneShow(7);
                        break;
                    case 7:
                        if (User.JustLeftClicked())
                            RouletteScene.LoadScene(0,()=> { return User.JustLeftClicked(); }, ()=> { PhoneShow(8); });
                        break;
                    case 8:
                        if (User.JustLeftClicked())
                            RouletteScene.LoadScene(1, () => { return User.JustLeftClicked(); }, () => { Projector.SwapTo(DamExam_DeliveryScene.scn); });
                        break;                
                }


                if (User.JustPressed(Keys.LeftControl,Keys.Z))
                {
                    PhoneShow(OldPhoneScreenNum);
                }

            },
            () =>
            {
                Game1.Painter.ClearBackground(Color.Black);
                PhoneScreen.Draw();
                new Gfx2D("Back", WholeScreen).Draw();           
            }
            );

    }

    public static class DamExam_FailScene
    {
        public static int FailNumber;



        //Write your own Update&Draw Action in here        
        public static Scene scn = new Scene(
            ()=>
            {
                scn.Camera.Zoom = (float)StandAlone.FullScreen.Width / DamExam_MainScene.WholeScreen.Width;
            },
            () =>
            {
                if(User.JustLeftClicked()||Keyboard.GetState().GetPressedKeys().Length>=1)
                {
                    Projector.Clear();
                    Projector.Load(DamExam_MainScene.scn);
                }
            },
            () =>
            {

                new Gfx2D("Fail" + FailNumber, DamExam_MainScene.WholeScreen).Draw();
                
            }
            );

    }
    public static class DamExam_DeliveryScene
    {
        private static int DeliveryTimer = -1;
        private static int DelInterval = 300;
        private static int DelNum=1;
        private static Gfx2D DeliverScreen = new Gfx2D(new Rectangle(191,88,260,440));
        private static Aligned<Gfx2D> Cirles = new Aligned<Gfx2D>(new Point(247, 329), new Point(50,0));

        private static void AddCircle()
        {
            Cirles.Add(new Gfx2D("Circle", new Rectangle(0, 0, 40, 40)));
            Cirles.Align();
        }

        public static Scene scn = new Scene(
            ()=>
            {
                DeliveryTimer = -1;
                DelNum = 1;
                scn.Camera.Zoom = (float)StandAlone.FullScreen.Width / DamExam_MainScene.WholeScreen.Width;
            }
            ,
            () =>
            {
                DeliverScreen.Sprite = "DAMEXAM.Delivery" + DelNum;            
                if (DeliveryTimer == -1)
                    DeliveryTimer = DelInterval;
                if(DeliveryTimer>0)
                    DeliveryTimer--;
                if(DeliveryTimer==0)
                {
                    if (DelNum == 1)
                    {
                        RouletteScene.LoadScene(2, () => { return true; }, () =>
                        {
                            DelNum = 2;
                            DeliveryTimer = -1;
                        });
                    }
                    if (DelNum == 2)
                    {
                        RouletteScene.LoadScene(3, () => { return true; }, () =>
                        {
                            DelNum = 3;
                            DeliveryTimer = -1;
                        });
                    }
                    if(DelNum==3)
                    {
                        Projector.PauseAll();
                        Projector.Load(DamExam_EndScene.scn);
                    }

                }

                if(DeliveryTimer%(DelInterval/8)==0)
                {
                    AddCircle();
                    if(Cirles.Count==4)
                    {
                        Cirles.Clear();
                    }
                }


            },
            () =>
            {
                DeliverScreen.Draw();
                new Gfx2D("Back", DamExam_MainScene.WholeScreen).Draw();
                Cirles.Draw();

                /*
                if (!Projector.Loaded(DamExam_EndScene.scn))
                    {
                    if (DelNum == 2)
                        DamExam_MainScene.DrawAlert();
                    if (DelNum == 4)
                        DamExam_MainScene.DrawAlert();

                }*/

            }
    );
    }

    public static class DamExam_EndScene
    {
        private static int EndTimer = 0;
        private static int EndInterval = 80;
        private static int EndNum
        {
            get { return Math.Min((EndTimer / EndInterval) + 1, 3); }
        }
        private static int OldEndNum;
        public static Scene scn = new Scene(
         () =>
         {
             EndTimer = 0;
             scn.Camera.Zoom = (float)StandAlone.FullScreen.Width /  DamExam_MainScene.WholeScreen.Width;
         }
         ,
         () =>
         {
             EndTimer++;
             if (OldEndNum == 1 && EndNum == 2)
             {
                  /*

                 if (StandAlone.Random() < DamExam_MainScene.Probs[4])
                     DamExam_MainScene.ShowFail(5);*/
             }
             if (OldEndNum == 2 && EndNum == 3)
             {
                 /*
                 if (StandAlone.Random() < DamExam_MainScene.Probs[5])
                     DamExam_MainScene.ShowFail(6);*/
             }

             if(EndNum==3&&(User.JustLeftClicked()||Keyboard.GetState().GetPressedKeys().Length>=1))
             {
                 Projector.Clear();
                 Projector.Load(DamExam_MainScene.scn);
             }

             OldEndNum = EndNum;
         },
         () =>
         {
             new Gfx2D("End" + EndNum, DamExam_MainScene.WholeScreen).Draw();
             //StandAlone.DrawFullScreen("End"+EndNum);
             if (EndNum == 1)
                 Filter.Absolute(DamExam_MainScene.WholeScreen, Color.Red * 0.5f * Fader.Flicker(100));
             if (EndNum == 2)
                 Filter.Absolute(DamExam_MainScene.WholeScreen, Color.Red * 0.5f * Fader.Flicker(100));
             if (Projector.Loaded(DamExam_FailScene.scn))
                 new Gfx2D("End" + (EndNum - 1), DamExam_MainScene.WholeScreen).Draw();

         }
        );


    }

    public static class RouletteScene
    {
        /*Space for Scene properties*/
        private static Gfx2D r = new Gfx2D("R1", new Rectangle(0, 0, 640, 740));

      
        public static bool CheckSuccess(double prob)
        {
            double r2 = r.Rotate % (2 * Math.PI);
            if (r2 <= 2 * Math.PI && r2 >= 2 * Math.PI * (1 - prob))
            {
                return false;
            }
            else
                return true;
        }

        public static int RTimer = -1; //룰렛을 돌리는 타이머
        public static int STimer = -1;
        public static int STimer_Max = 80;

        private static int FailNum; //실패시 불러올 씬의 넘버를 지정합니다.
        private static Func<bool> RCondition;//룰렛이 회전하게 되는 컨디션을 지정합니다.
        private static Action SuccessAction;//룰렛이 성공할 시 일어날 액션을 지정합니다.

        public static double[] Probs_Final = new double[] { 0.16, 0.16, 0.15, 0.15, 0.15, 0.21 }; // 룰렛의 확률을 지정하는 집합입니다.
        public static double[] Probs = Probs_Final;


        public static void LoadScene(int fNum, Func<bool> Condition, Action successAction)
        {
            FailNum = fNum;
            RCondition = Condition;
            SuccessAction = successAction;
            Projector.PauseAll();
            Projector.Load(scn);
        }


        //Write your own Update&Draw Action in here        
        public static Scene scn = new Scene(
            ()=>
            {
                scn.Camera.Zoom = (float)StandAlone.FullScreen.Width / DamExam_MainScene.WholeScreen.Width;
                RTimer = -1;//룰렛을 초기화합니다.
                STimer = -1;
                r.Sprite = "R" + (FailNum + 1);//룰렛을 넘버링에 맞게 교환합니다.
                r.ROrigin = new Point(639, 752);
            },
            () =>
            {
                if (RTimer == -1 && RCondition())
                    RTimer = 100 + StandAlone.Random(50, 150);
                if (RTimer == 0&&STimer==-1)
                {

                    if (!CheckSuccess(Probs[FailNum])) // 룰렛돌리기에 실패했으면 죽습니다
                    {
                        Projector.Unload(scn);
                        Projector.ResumeAll();
                        DamExam_MainScene.ShowFail(FailNum + 1);//관련된 페일상태를 띄웁니다.
                       
                    }
                    else
                    {
                        STimer = STimer_Max;
                    
                    }
                }
                if (RTimer > 0)
                {
                    RTimer--;
                }
                if (STimer > 0)
                    STimer--;
                if(STimer==0)
                {
                    SuccessAction();
                    Projector.Unload(scn);                    
                    Projector.ResumeAll();
                }
            },
            () =>
            {
                if (RTimer != -1)
                    r.Rotate += (float)Math.Sqrt(RTimer) / 60;
                if (STimer == -1)
                {
                    r.Draw(Color.White);
                    Gfx2D rF = new Gfx2D("RF" + (FailNum + 1), DamExam_MainScene.WholeScreen);
                    rF.Draw();
                    if (RTimer == -1)
                        rF.DrawAddon("RArrow", Color.White * Fader.Flicker(100));
                    else
                        rF.DrawAddon("RArrow", Color.White);
                }
                else
                {
                    Gfx2D Success = new Gfx2D("Success", new Point(200, 20), 1f);
                    Success.Center = new Point(320, 340);
                    Success.Draw(Color.White * Fader.Flicker(30));
                }
            }
            );

    }


}

