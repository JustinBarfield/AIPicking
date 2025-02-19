using AIPicking.ViewModels;
using AIPicking.Views;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace AIPicking
{
    public class ViewModel : INotifyPropertyChanged
    {
        private string textBoxValue;
        private string recognizedText;
        private string recognizedLang;
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

        public string RecognizedLang
        {
            get { return recognizedLang; }
            set
            {
                recognizedLang = value;
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
        public ICommand OpenScanCartIDViewCommand { get; }

        public ViewModel()
        {
            SynthesizeSpeechCommand = new RelayCommand(async () => await SynthesizeSpeech());
            RecognizeSpeechFromMicCommand = new RelayCommand(async () => await RecognizeSpeechFromMic());
            OpenScanCartIDViewCommand = new RelayCommand(async () => await OpenScanCartIDView(null, null));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        static void LanguageDetectionExample(TextAnalyticsClient client, string text)
        {
            DetectedLanguage detectedLanguage = client.DetectLanguage(text);
            Console.WriteLine("Language:");
            Console.WriteLine($"\t{detectedLanguage.Name},\tISO-6391: {detectedLanguage.Iso6391Name}\n");

            bool isEnglishOrSpanish = detectedLanguage.Iso6391Name == "en" || detectedLanguage.Iso6391Name == "es";
            if (isEnglishOrSpanish)
            {
                if (detectedLanguage.Iso6391Name == "en")
                {
                    Console.WriteLine("The detected language is English.");
                   // RecognizedLang = "en";
                }
                else if (detectedLanguage.Iso6391Name == "es")
                {
                    Console.WriteLine("The detected language is Spanish.");
                   // RecognizedLang = "es";
                }
            }
            else
            {
                Console.WriteLine("The detected language is neither English nor Spanish.");
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
        static string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
        static string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");
        static string languageKey = "59O1KpOwwOQwFTliUh931fyiATPPXJES1T5CJNg6dGAga7odm5G2JQQJ99BBACYeBjFXJ3w3AAAaACOGyRH3";
        static string languageEndpoint = "https://seniordesignlanguage.cognitiveservices.azure.com/";

        private static readonly AzureKeyCredential credentials = new AzureKeyCredential(languageKey);
        private static readonly Uri endpoint = new Uri(languageEndpoint);



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

        public async Task OpenScanCartIDView(object sender, RoutedEventArgs e)
        {
            var cartIDViewModel = new CartIDViewModel();
            var cartIDView = new CartID { DataContext = cartIDViewModel };

            var scanCartIDWindow = new Window
            {
                Title = "Scan Cart ID",
                Content = cartIDView, // Set the content to the CartID user control
                Width = 400,
                Height = 300
            };

            cartIDViewModel.SynthesizeSpeech(); // do not await it or it will wait until its done to show the window
            scanCartIDWindow.ShowDialog();
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
