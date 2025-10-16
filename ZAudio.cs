// ZAudio for HHSAdvSDL

using System;
using System.Dynamic;
using System.IO;
using System.Text;
using LibVLCSharp.Shared;

namespace HHSAdvAvalonia
{
    public class ZAudio
    {
        private string[] soundFiles = {
            "highschool", "charumera", "explosion", string.Empty,  "in_toilet", "acid",
        };
        public string DataFolder { get; private set; }
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        public ZAudio(string d)
        {
            DataFolder = d;
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            _mediaPlayer.Volume = 100;
            _mediaPlayer.EndReached += (sender, e) =>
            {
                // Handle end of playback if needed
            };
        }
        public void Play(int id)
        {
            if (id < 0 || id >= soundFiles.Length || string.IsNullOrEmpty(soundFiles[id])) return;
            StringBuilder mp3 = new StringBuilder(soundFiles[id]);
            mp3.Append(".mp3");
            string af = Path.Combine(DataFolder, mp3.ToString());
            if (File.Exists(af))
            {
                using (var media = new Media(_libVLC, af, FromType.FromPath))
                {
                    _mediaPlayer.Play(media);
                }
            }
        }
    }
}