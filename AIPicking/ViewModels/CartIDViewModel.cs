using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Windows;
using AIPicking.Views;

namespace AIPicking.ViewModels
{
    public class CartIDViewModel : INotifyPropertyChanged
    {
       
        public CartIDViewModel()
        {
            _intentViewModel = new IntentViewModel();
            SynthesizeSpeechCommand = new RelayCommand(async () => await SynthesizeSpeech());
            RecognizeSpeechFromMicCommand = new RelayCommand(async () => await RecognizeSpeechFromMic());
            ReturnToHomeCommand = new RelayCommand((async () => await ReturnToHome(null, null)));
            EnterCommand = new RelayCommand(async () => await OpenPickItemView());
        }
        
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
            }
        }

        public async Task SynthesizeSpeech()
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                string text = "Please say the cart I D";
                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);

                RecognizeSpeechFromMic();
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
                TicketNumber = RecognizedText;
                CartID = RecognizedText;
                Console.WriteLine($"RECOGNIZED: Text={speechRecognitionResult.Text}");
                _intentViewModel.InputText = RecognizedText;
                _intentViewModel.RegisteredLang = RecognizedLang;
            }

            IsRecording = false;
        }
        #region Properties
        private readonly IntentViewModel _intentViewModel;
        static string speechKey = "8xzDB1l9OOZGb5CLKHjS82qhnAPeVV31yKZqDAyTmde0A98lbYcRJQQJ99BBACYeBjFXJ3w3AAAYACOGlizV";
        static string speechRegion = "eastus";
       
        private string cartID;
        public string CartID
        {
            get { return cartID; }
            set
            {
                cartID = value;
                OnPropertyChanged();
            }
        }

        private string recognizedText;
        private string recognizedLang;
        private bool isRecording;
        private string ticketNumber;

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

        public string TicketNumber
        {
            get { return ticketNumber; }
            set
            {
                ticketNumber = value;
                OnPropertyChanged();
            }
        }

        public ICommand SynthesizeSpeechCommand { get; }
        public ICommand RecognizeSpeechFromMicCommand { get; }
        public ICommand ReturnToHomeCommand { get; }
        public ICommand EnterCommand { get; }
        public ICommand AnalyzeCommand => _intentViewModel.AnalyzeCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Pages
        public async Task ReturnToHome(object sender, RoutedEventArgs e)
        {
            var viewModel = new ViewModel();
            var view = new HomePageUC { DataContext = viewModel };

            // Assuming you have a reference to the current window
            var currentWindow = System.Windows.Application.Current.MainWindow;

            // Update the content of the current window
            currentWindow.Content = view;

            // Optionally, you can update the title or other properties of the current window
            currentWindow.Title = "Main Window";
            currentWindow.Width = 400;
            currentWindow.Height = 300;
        }

        public async Task OpenPickItemView()
        {
            var pickItemViewModel = new PickItemViewModel();
            var pickItemView = new PickItemUC { DataContext = pickItemViewModel };

            // Assuming you have a reference to the current window
            var currentWindow = System.Windows.Application.Current.MainWindow;

            // Update the content of the current window
            currentWindow.Content = pickItemView;

            // Optionally, you can update the title or other properties of the current window
            currentWindow.Title = "Pick Item";
            currentWindow.Width = 400;
            currentWindow.Height = 300;
        }
        #endregion
    }
}