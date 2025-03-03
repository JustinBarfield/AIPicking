using AIPicking.Views;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using static AIPicking.ViewModel;

namespace AIPicking.ViewModels
{
    public class PickItemViewModel : INotifyPropertyChanged
    {
        #region Properties
        static string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
        static string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");
        private PickingItem pickingItem;
        private string cartID;
        private string recognizedText;
        private string ticketNumber;
        private readonly IntentViewModel _intentViewModel;
        private string recognizedLang;
        private bool isRecording;

        public string RecognizedText
        {
            get => recognizedText;
            set
            {
                recognizedText = value;
                OnPropertyChanged();
            }
        }

        public string TicketNumber
        {
            get => ticketNumber;
            set
            {
                ticketNumber = value;
                OnPropertyChanged();
            }
        }

        public bool IsRecording
        {
            get => isRecording;
            set
            {
                isRecording = value;
                OnPropertyChanged();
            }
        }
        public PickingItem PickingItem
        {
            get => pickingItem;
            set
            {
                pickingItem = value;
                OnPropertyChanged();
            }
        }

        public string CartID
        {
            get => cartID;
            set
            {
                cartID = value;
                OnPropertyChanged();
            }
        }

        public string Quantity
        {
            get => PickingItem.Quantity;
            set
            {
                PickingItem.Quantity = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => PickingItem.Title;
            set
            {
                PickingItem.Title = value;
                OnPropertyChanged();
            }
        }

        public string Location
        {
            get => PickingItem.Location;
            set
            {
                PickingItem.Location = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => PickingItem.Description;
            set
            {
                PickingItem.Description = value;
                OnPropertyChanged();
            }
        }

        public string ItemsLeft
        {
            get => PickingItem.ItemsLeft;
            set
            {
                PickingItem.ItemsLeft = value;
                OnPropertyChanged();
            }
        }

        public string SerialNumber
        {
            get => PickingItem.SerialNumber;
            set
            {
                PickingItem.SerialNumber = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; }
        public ICommand SkipItemCommand { get; }
        public ICommand HomeCommand { get; }
        #endregion
        public PickItemViewModel()
        {
            _intentViewModel = new IntentViewModel();
            PickingItem = new PickingItem
            {
                Quantity = "10",
                Title = "Sample Item",
                Location = "Aisle 3, Shelf 2",
                Description = "This is a sample item for picking.",
                ItemsLeft = "50",
                SerialNumber = "123"
            };
            ConfirmCommand = new RelayCommand(OnConfirm);
            SkipItemCommand = new RelayCommand(OnSkipItem);
            HomeCommand = new RelayCommand(OnHome);

            // Start the asynchronous initialization
            // InitializeAsync();
            SynthesizeAllInfo();
        }

        private async void InitializeAsync()
        {
            await SynthesizeAllInfo();
        }
        #region Buttons
        private async Task OnConfirm()
        {
            // Implement the logic for the Confirm button
        }

        private async Task OnSkipItem()
        {
            // Implement the logic for the Skip Item button
        }

        private async Task OnHome()
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

        #endregion

        #region TTS
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

        private async Task SynthesizeAllInfo()
        {
            
                var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
                speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";

                using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
                {
                    string text = $"Item: {Title}, Quantity: {Quantity}, Location: {Location}, Description: {Description}, Items Left: {ItemsLeft}, Serial Number: {SerialNumber}";
                    var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                    OutputSpeechSynthesisResult(speechSynthesisResult, text);
                }

                // Ask if the user is at the shelf
                await AskIfAtShelf();
            
        }

        public async Task SynthesizeSpeech(string text)
        {
            
                var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
                speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";

                using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
                {
                    var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                    OutputSpeechSynthesisResult(speechSynthesisResult, text);

                    await RecognizeSpeechFromMic();
                }
           
        }

        private async Task AskIfAtShelf()
        {

            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechSynthesisVoiceName = "en-US-AvaMultilingualNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                string text = "Are you at the shelf?";
                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }

            // Recognize speech from the mic and analyze the intent
            await RecognizeSpeechFromMic();

        }

        #endregion

        #region STT
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
                _intentViewModel.RegisteredLang = recognizedLang;

                // Analyze the recognized speech
                await AnalyzeRecognizedSpeech(RecognizedText);
            }

            IsRecording = false;
        }

        public ICommand AnalyzeCommand => _intentViewModel.AnalyzeCommand;

        private async Task AnalyzeRecognizedSpeech(string recognizedText)
        {
            // Set the input text and language
            _intentViewModel.InputText = recognizedText;
            _intentViewModel.RegisteredLang = recognizedLang;

            // Execute the AnalyzeCommand
            if (_intentViewModel.AnalyzeCommand.CanExecute(null))
            {
                _intentViewModel.AnalyzeCommand.Execute(null);
            }

            // Wait for the analysis to complete (assuming AnalyzeCommand is asynchronous)
            await Task.Delay(1000); // Adjust the delay as needed

            // Retrieve the intent and confidence score
            var intent = _intentViewModel.Intent;
            var confidenceScore = _intentViewModel.ConfidenceScore;

           
        }
        #endregion
       

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
