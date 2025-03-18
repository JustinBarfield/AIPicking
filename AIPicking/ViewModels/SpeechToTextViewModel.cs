using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AIPicking.ViewModels
{
    class SpeechToTextViewModel : INotifyPropertyChanged
    {
        public SpeechToTextViewModel()
        {
            _intentViewModel = new IntentViewModel();
        }

        static string OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
        {
            switch (speechSynthesisResult.Reason)
            {
                case ResultReason.SynthesizingAudioCompleted:
                    Console.WriteLine($"Speech synthesized for text: [{text}]");
                    return text;
                case ResultReason.Canceled:
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                    Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    if (cancellation.Reason == CancellationReason.Error)
                    {
                        Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                        Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                        Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                        return "Error";
                    }
                    return "Canceled";
                default:
                    return "error";
            }
        }

        private string recognizedText;
        private string recognizedLang;
        private bool isRecording;
        private static string languageKey = "59O1KpOwwOQwFTliUh931fyiATPPXJES1T5CJNg6dGAga7odm5G2JQQJ99BBACYeBjFXJ3w3AAAaACOGyRH3";
        private static string languageEndpoint = "https://seniordesignlanguage.cognitiveservices.azure.com/";
        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
        private static readonly Uri endpoint = new Uri(languageEndpoint);
        private readonly IntentViewModel _intentViewModel;
        public ICommand AnalyzeCommand => _intentViewModel.AnalyzeCommand;

        public bool IsRecording
        {
            get { return isRecording; }
            set
            {
                isRecording = value;
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

        public string RecognizedLang
        {
            get { return recognizedLang; }
            set
            {
                recognizedLang = value;
                OnPropertyChanged();
            }
        }

        private string language = "en-US"; // Default language
        public string Language
        {
            get { return language; }
            set
            {
                language = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static string speechKey = "8xzDB1l9OOZGb5CLKHjS82qhnAPeVV31yKZqDAyTmde0A98lbYcRJQQJ99BBACYeBjFXJ3w3AAAYACOGlizV";
        private static string speechRegion = "eastus";

        public async Task RecognizeSpeechFromMic()
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechRecognitionLanguage = Language; // Set the recognition language

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

            if (!string.IsNullOrEmpty(RecognizedText))
            {
                var client = new TextAnalyticsClient(endpoint, credentials);
                Azure.AI.TextAnalytics.DetectedLanguage detectedLanguage = client.DetectLanguage(RecognizedText);
                RecognizedLang = detectedLanguage.Iso6391Name;

                string thankYouMessage = RecognizedLang == "es" ? "Gracias" : "Thank you";

                var thankYouConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
                thankYouConfig.SpeechSynthesisVoiceName = RecognizedLang == "es" ? "es-ES-AlvaroNeural" : "en-US-GuyNeural";

                using (var speechSynthesizer = new SpeechSynthesizer(thankYouConfig))
                {
                    var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(thankYouMessage);
                    OutputSpeechSynthesisResult(speechSynthesisResult, thankYouMessage);
                }
            }
            else
            {
                Console.WriteLine("No speech recognized.");
            }
        }

        
    }
}
