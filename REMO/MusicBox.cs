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

namespace REMO_Engine_V1._01
{

    public enum MusicBoxMode { Immediate, FadeOut }
    public static class MusicBox
    {

        private static List<SoundEffectInstance> SEList = new List<SoundEffectInstance>();
        private static Dictionary<string, SoundEffect> SongContainer = new Dictionary<string, SoundEffect>();
        private static List<SoundEffectInstance> SongPlayer = new List<SoundEffectInstance>();
        private static Dictionary<SoundEffectInstance, float> SongCoefficients = new Dictionary<SoundEffectInstance, float>();
        private static Dictionary<SoundEffectInstance, int> SongDisposer = new Dictionary<SoundEffectInstance, int>();

        public static string CurrentSong = "";
        private static float songVolume = 1.0f;
        public static float SongVolume
        {
            get
            {
                return songVolume;
            }
            set
            {
                if (value != songVolume && value <= 1.0f && value >= 0f)
                {
                    for (int i = 0; i < SongPlayer.Count; i++)
                    {
                        SongPlayer[i].Volume = value * SongCoefficients[SongPlayer[i]];
                    }
                    songVolume = value;
                }
            }

        }

        //두가지 state가 있다. 음악을 끌 때 즉시 꺼지는 모드(Immediate), 서서히 페이드아웃하며 꺼지는 모드(FadeOut) 
        private static MusicBoxMode mode;
        public static MusicBoxMode Mode
        {
            get
            {
                return mode;
            }

            set
            {
                if (value == MusicBoxMode.Immediate)
                    DisposeTimer = 0;
                if (value == MusicBoxMode.FadeOut)
                    DisposeTimer = 30;
                mode = value;

            }

        }


        public static float SEVolume = 1.0f;

        public static int DisposeTimer = 0;

        public static SoundEffectInstance PlaySE(string SEName)
        {
            SoundEffect soundEffect = Game1.content.Load<SoundEffect>(SEName);
            SoundEffectInstance soundEffectInstance = soundEffect.CreateInstance();
            soundEffectInstance.Volume = SEVolume;
            soundEffectInstance.Play();
            SEList.Add(soundEffectInstance);
            return soundEffectInstance;
        }
        public static SoundEffectInstance PlaySE(string SEName, float VolumeCoefficient)
        {
            SoundEffect soundEffect = Game1.content.Load<SoundEffect>(SEName);
            SoundEffectInstance soundEffectInstance = soundEffect.CreateInstance();
            soundEffectInstance.Volume = SEVolume * VolumeCoefficient;
            soundEffectInstance.Play();
            SEList.Add(soundEffectInstance);
            return soundEffectInstance;
        }

        public static void LoadSong(string SongName) // Song Container에 Song을 적재한다.
        {
            if (!SongContainer.ContainsKey(SongName))
            {
                SoundEffect soundEffect = Game1.content.Load<SoundEffect>(SongName);
                SongContainer.Add(SongName, soundEffect);
            }
        }

        public static void PlaySong(string SongName) // Container의 곡을 Player에서 재생한다.
        {
            if (!SongContainer.ContainsKey(SongName))
                LoadSong(SongName);
            if (SongName == CurrentSong)
                return;
            EmptySongPlayer();
            CurrentSong = SongName;
            SoundEffectInstance soundEffectInstance = SongContainer[SongName].CreateInstance();
            soundEffectInstance.Volume = SongVolume;
            soundEffectInstance.Play();
            soundEffectInstance.IsLooped = true;
            SongPlayer.Add(soundEffectInstance);
            SongCoefficients.Add(soundEffectInstance, 1f);
        }

        public static void PlaySong(string SongName, float VolumeCoefficient) // Container의 곡을 Player에서 재생한다.
        {
            if (!SongContainer.ContainsKey(SongName))
                LoadSong(SongName);
            EmptySongPlayer();
            SoundEffectInstance soundEffectInstance = SongContainer[SongName].CreateInstance();
            soundEffectInstance.Volume = SongVolume * VolumeCoefficient;
            soundEffectInstance.Play();
            soundEffectInstance.IsLooped = true;
            SongPlayer.Add(soundEffectInstance);
            SongCoefficients.Add(soundEffectInstance, VolumeCoefficient);
        }


        private static void EmptySongPlayer() // SongPlayer의 곡들을 Disposer로 옮긴다.
        {
            while (SongPlayer.Count > 0)
            {
                SongDisposer.Add(SongPlayer[0], DisposeTimer);
                SongPlayer.RemoveAt(0);
            }
        }

        public static void StopSong()
        {
            EmptySongPlayer();
            CurrentSong = "";
        }

        public static void Update()
        {
            //Garbage Collection
            for (int i = 0; i < SEList.Count; i++)
            {
                if (SEList[i].State == SoundState.Stopped)
                {
                    SEList.RemoveAt(i);
                    i--;
                }
            }

            //Disposer 처리
            foreach (SoundEffectInstance s in SongDisposer.Keys.ToList())
            {
                if (SongDisposer[s] > 0)
                {
                    SongDisposer[s]--;
                    s.Volume = s.Volume * (SongDisposer[s] / ((float)SongDisposer[s] + 1));
                }
                else
                {
                    s.Stop();
                    SongDisposer.Remove(s);
                    SongCoefficients.Remove(s);
                }
            }
        }

    }
}
