using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Speech.AudioFormat;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using Tfs.BuildNotifications.Tray.Infrastructure.Config.Interfaces;
using Tfs.BuildNotifications.Tray.Services.Interfaces;

namespace Tfs.BuildNotifications.Tray.Services
{
	public class TextToSpeechNotificationService : NotificationService, INotificationService
    {
        public TextToSpeechNotificationService(IAppConfig appConfig) : base(appConfig) { }

        protected override void ShowNotification(string title, string message, string image, Action onActivation = null)
        {
            Speak(title);
            Speak(message);
        }

        private void Speak(string tts)
        {
            string fileName;

            fileName = Path.GetTempFileName();
            fileName = Path.ChangeExtension(fileName, "wav");

            using (var synth = new SpeechSynthesizer())
            {
                synth.SelectVoice(GetVoiceName(synth));

                // Configure the audio output. 
                synth.SetOutputToWaveFile(fileName,
                    new SpeechAudioFormatInfo(32000, AudioBitsPerSample.Sixteen, AudioChannel.Stereo));

                synth.Speak(tts);
            }

            var device = -1;

            if (device == -1)
            {
                var devices = WaveOut.DeviceCount;

                Parallel.For(0, devices, (deviceNumber, state) => 
                {
                    try
                    {
                        PlayOnDevice(fileName, deviceNumber);
                    }
                    catch (Exception) { }
                });
            }
            else
            {
                PlayOnDevice(fileName, device);
            }

            File.Delete(fileName);
        }

        private string GetVoiceName(SpeechSynthesizer synth)
        {
            var voices = synth.GetInstalledVoices();

            // Use the last one at the moment. ToDo: set in app.config?
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