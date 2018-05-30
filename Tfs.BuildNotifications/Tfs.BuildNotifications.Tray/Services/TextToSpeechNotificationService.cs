using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces;
using Tfs.BuildNotifications.Tray.Services.Interfaces;

namespace Tfs.BuildNotifications.Tray.Services
{
    public class TextToSpeechNotificationService : NotificationService, INotificationService
    {
        public TextToSpeechNotificationService(IAppConfig appConfig) : base(appConfig)
        {
        }

        protected override void ShowNotification(string title, string message, string image, Action onActivation = null)
        {
            SpeakTts(title);
            SpeakTts(message);
        }

        private static void SpeakTts(string tts)
        {
            string fileName;

            fileName = Path.GetTempFileName();
            fileName = Path.ChangeExtension(fileName, "wav");

            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                synth.SelectVoice(GetVoiceName(synth));

                // Configure the audio output. 
                synth.SetOutputToWaveFile(
                    fileName,
                    new SpeechAudioFormatInfo(32000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));

                synth.Speak(tts);
            }

            int Device = -1;
            if (Device == -1)
            {
                var devices = WaveOut.DeviceCount;

                Parallel.For(0, devices, (deviceNumber, state) => 
                {
                    try
                    {
                        PlayOnDevice(fileName, deviceNumber);
                    }
                    catch (Exception)
                    {
                    }
                });
            }
            else
            {
                PlayOnDevice(fileName, Device);
            }

            File.Delete(fileName);
        }

        private static string GetVoiceName(SpeechSynthesizer synth)
        {
            var voices = synth.GetInstalledVoices();

            //read from appconfig is cooming soon. Use The last one at the moment
            return voices.Last().VoiceInfo.Name;
        }

        private static void PlayOnDevice(string fileName, int device)
        {
            var waveOut = new WaveOutEvent();
            waveOut.DeviceNumber = device;

            WaveStream reader;
            if (fileName.EndsWith("mp3"))
            {
                reader = new Mp3FileReader(fileName);
            }
            else
            {
                reader = new WaveFileReader(fileName);
            }

            waveOut.Init(reader);
            waveOut.Play();

            while (waveOut.PlaybackState != PlaybackState.Stopped)
            {
                Thread.Sleep(20);
            }

            waveOut.Stop();

            reader.Dispose();
            waveOut.Dispose();
        }
    }
}