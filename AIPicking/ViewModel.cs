﻿using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AIPicking
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string textBoxValue;
        private string recognizedText;
        private bool isRecording;

        public string TextBoxValue
        {
            get { return textBoxValue; }
            set
            {
                textBoxValue = value;
                OnPropertyChanged();
            }
        }

        public string RecognizedText
        {
            get { return recognizedText; }
            set
            {
                recognizedText = value;
                OnPropertyChanged();
            }
        }

        public bool IsRecording
        {
            get { return isRecording; }
            set
            {
                isRecording = value;
                OnPropertyChanged();
            }
        }

        public ICommand SynthesizeSpeechCommand { get; }
        public ICommand RecognizeSpeechFromMicCommand { get; }

        public ViewModel()
        {
            SynthesizeSpeechCommand = new RelayCommand(async () => await SynthesizeSpeech());
            RecognizeSpeechFromMicCommand = new RelayCommand(async () => await RecognizeSpeechFromMic());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
        static string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
        static string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

        static void OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
        {
            switch (speechSynthesisResult.Reason)
            {
                case ResultReason.SynthesizingAudioCompleted:
                    Console.WriteLine($"Speech synthesized for text: [{text}]");
                    break;
                case ResultReason.Canceled:
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                    }
                    break;
                default:
                    break;
            }
        }

        public async Task SynthesizeSpeech()
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);

            // The neural multilingual voice can speak different languages based on the input text.
            speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                // Get text from the ViewModel and synthesize to the default speaker.
                string text = TextBoxValue;

                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }
        }

        public async Task RecognizeSpeechFromMic()
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);

            IsRecording = true;
            using (var audioConfig = AudioConfig.FromDefaultMicrophoneInput())
            using (var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig))
            {
                Console.WriteLine("Speak into your microphone.");
                var speechRecognitionResult = await speechRecognizer.RecognizeOnceAsync();
                RecognizedText = speechRecognitionResult.Text;
                Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
            }
            IsRecording = false;
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public async void Execute(object parameter)
        {
            await _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
