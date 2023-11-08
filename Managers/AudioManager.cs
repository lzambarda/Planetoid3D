using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
//using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
//using Microsoft.Xna.Framework.Net;
//using Microsoft.Xna.Framework.Storage;

namespace Planetoid3D
{
    /// <summary>
    /// Audio Manager, last review on version 1.0.2
    /// </summary>
    public static class AudioManager
    {
        public static void Initialize(ContentManager Content)
        {
            //Get ready with the sound engine
            audioEngine = new AudioEngine("Content//Audio//PlanetoidAudio.xgs");
            waveBank = new WaveBank(audioEngine, "Content//Audio//Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, "Content//Audio//Sound Bank.xsb");

            //Load directory info, abort if none
           // audioCollection["asteroid_death"] = Content.Load<Song>("Audio/asteroid_death");
 
           /* DirectoryInfo dir = new DirectoryInfo(Content.RootDirectory + "\\Audio");
            if (!dir.Exists)
                throw new DirectoryNotFoundException();
            //Init the resulting list
            soundEffects = new Dictionary<string, SoundEffect>();
            songs = new Dictionary<string, Song>();

            //Load all files that matches the file filter
            FileInfo[] files = dir.GetFiles("*.*");
            foreach (FileInfo file in files)
            {
                string key = Path.GetFileNameWithoutExtension(file.Name);
                if (key.StartsWith("music"))
                {
                    songs[key] = Content.Load<Song>("Audio/" + key);
                }
                else
                {
                    soundEffects[key] = Content.Load<SoundEffect>("Audio/" + key);
                }
            }*/

            planetoid = soundBank.GetCue("planetoid_loop");
            battle = soundBank.GetCue("battle_theme");
            swish = soundBank.GetCue("camera_move");
            //swish = soundEffects["camera_move"].CreateInstance();

            listener = new AudioListener();
            listener.Up = Vector3.Up;
            emitter = new AudioEmitter();
            emitter.Up = Vector3.Up;
        }

        //Audio
        private static SoundBank soundBank;
        private static WaveBank waveBank;

        private static AudioEngine audioEngine;
        private static Dictionary<string, SoundEffect> soundEffects;
        private static Dictionary<string, Song> songs;
        private static Cue ingame;
        private static Cue battle;
        private static Cue planetoid;
        private static Cue swish;
        //private static SoundEffectInstance swish;

        private static AudioListener listener;
        private static AudioEmitter emitter;

        public static int music_volume;
        public static int sound_volume;

        private static bool battleFlag;
        public static int battleIsHappening;


        public static void StartMusic()
        {
            //Play the title music
            ingame = soundBank.GetCue("music_title");
            ingame.Play();
            // ingame = songs["music_title"];
            // MediaPlayer.Play(ingame);
        }

        public static void StopMusic()
        {
            ingame.Stop(AudioStopOptions.AsAuthored);
           // MediaPlayer.Stop();
        }

        public static void ChangeVolume(string category, int volume)
        {
           AudioCategory cat = audioEngine.GetCategory(category);
           cat.SetVolume(volume / 10f);
        }

        public static void Play(string name)
        {
           soundBank.PlayCue(name);
            //soundEffects[name].Play();
        }

        public static void Stop(string name)
        {
            soundBank.GetCue(name).Stop(AudioStopOptions.AsAuthored);
        }

        public static void Load(ref Cue audio)
        {
            audio = soundBank.GetCue("turbina_loop");
        }

        public static void Play3D(BaseObject source,string name)
        {
            emitter.Position = source.matrix.Translation;
            emitter.Velocity = source.speed;
            emitter.Forward = source.matrix.Forward;// Vector3.Normalize(GameEngine.gameCamera.position - source.matrix.Translation);
            soundBank.PlayCue(name, listener, emitter);
            /*SoundEffectInstance sei = soundEffects[name].CreateInstance();
            sei.Apply3D(listener, emitter);
            sei.Play();*/
        }

        public static void StartSwish()
        {
            if (sound_volume > 0)
            {
               if (swish.IsPlaying)
                {
                    swish.Stop(AudioStopOptions.Immediate);
                }
                swish = soundBank.GetCue("camera_move");
                swish.Play();
                /*if (swish.State == SoundState.Playing)
                {
                    swish.Stop(true);
                }
                swish = soundEffects["camera_move"].CreateInstance();
                swish.Play();*/
            }
        }
        public static bool IsSwishPlaying()
        {
            return (swish != null && swish.IsPlaying);
            //return (swish.State == SoundState.Playing);
        }
        public static void StopSwish()
        {
            swish.Stop(AudioStopOptions.AsAuthored);
            //swish.Stop(false);
        }

        /// <summary>
        /// Update the position of the camera listener in the audio engine
        /// </summary>
        public static void UpdateListener()
        {
            listener.Forward = Vector3.Normalize(GameEngine.gameCamera.target.matrix.Translation - GameEngine.gameCamera.position);
            listener.Position = GameEngine.gameCamera.position;

            //CHANGE BETWEEN BATTLE MUSIC AND NORMAL MUSIC
            if (music_volume > 0)
            {
                if (battleIsHappening>10)// || GameEngine.planets.Exists(p => p.PlayerBattle() == true))
                {
                    battleIsHappening -= 1;
                    if (battleFlag == false)
                    {
                        ingame.Stop(AudioStopOptions.AsAuthored);
                        battle = soundBank.GetCue("battle_theme");
                        battle.Play();
                       /* MediaPlayer.Stop();
                        MediaPlayer.Play(songs["music_battle"]);
                        battleFlag = true;*/
                    }
                }
                else if (battleFlag)
                {
                    battle.Stop(AudioStopOptions.AsAuthored);
                    ingame = soundBank.GetCue("music_ingame");
                    ingame.Play();
                    /*MediaPlayer.Stop();
                    MediaPlayer.Play(ingame);
                    battleFlag = false;*/
                }
            }
        }

        /// <summary>
        /// Start planetoid noise
        /// </summary>
        public static void StartPlanetoid()
        {
            planetoid = soundBank.GetCue("planetoid_loop");
            planetoid.Play();
           // MediaPlayer.Play(songs["music_planetoid"]);
        }

        /// <summary>
        /// Update the planetoid noise by balancing it with the gameplay music
        /// </summary>
        public static void UpdatePlanetoid(float distance)
        {
            distance = MathHelper.Clamp(distance, 0, 3000);
            distance = (float)Math.Pow(1 - (distance / 3000f), 2);
            audioEngine.GetCategory("Planetoid").SetVolume(distance * (music_volume / 10f));
            audioEngine.GetCategory("Music").SetVolume((1 - distance) * (music_volume / 10f));

            //End the easter egg shooting the player away from the planetoid
            if (planetoid.IsStopped && planetoid.Name == "planetoid_easter")
            {
                planetoid = soundBank.GetCue("planetoid_loop");
                planetoid.Play();
                GameEngine.gameCamera.targetZoom = 3000;
                PlanetoidGame.flash = 1;
            }
        }

        /// <summary>
        /// Stop planetoid noise
        /// </summary>
        public static void StopPlanetoid()
        {
           planetoid.Stop(AudioStopOptions.Immediate);
        }

        /// <summary>
        /// Start the audio easter egg!
        /// </summary>
        public static void StartEasterEgg()
        {
            planetoid.Stop(AudioStopOptions.AsAuthored);
            planetoid = soundBank.GetCue("planetoid_easter");
            planetoid.Play();
        }

        /// <summary>
        /// Switch between gameplay music and title music
        /// </summary>
        public static void ToggleMusic(AudioStopOptions stopOptions)
        {
            if (battle != null)
            {
                battle.Stop(AudioStopOptions.AsAuthored);
            }
            //Play the title music
            if (ingame.Name == "music_title")
            {
                ingame.Stop(stopOptions);
                ingame = soundBank.GetCue("music_ingame");
            }
            else
            {
                ingame.Stop(stopOptions);
                ingame = soundBank.GetCue("music_title");
            }
            ingame.Play();
        }
    }
}
