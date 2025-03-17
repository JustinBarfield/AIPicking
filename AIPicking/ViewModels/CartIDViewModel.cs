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
        private readonly TextToSpeechViewModel textToSpeechViewModel;

        public CartIDViewModel()
        {
            _intentViewModel = new IntentViewModel();
            speechToTextViewModel = new SpeechToTextViewModel();
            textToSpeechViewModel = new TextToSpeechViewModel();

            SynthesizeSpeechCommand = new RelayCommand(async () => await textToSpeechViewModel.SynthesizeSpeech("Say the CartID"));
            AnalyzeCommand = new RelayCommand(async () => await _intentViewModel.AnalyzeConversationAsync(CartID, "en"));
            RecognizeSpeechFromMicCommand = new RelayCommand(async () =>
            {
                IsRecording = true;
                await speechToTextViewModel.RecognizeSpeechFromMic();
                CartID = speechToTextViewModel.RecognizedText;
                IsRecording = false;
            });

            ReturnToHomeCommand = new RelayCommand(async () => await ReturnToHome(null, null));
            EnterCommand = new RelayCommand(async () => await OpenPickItemView());
            SpeakCartID();
        }

        #region Properties
        private readonly IntentViewModel _intentViewModel;
        private readonly SpeechToTextViewModel speechToTextViewModel;

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
        public ICommand AnalyzeCommand { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task SpeakCartID()
        {
            await textToSpeechViewModel.SynthesizeSpeech("Say the CartID");
            IsRecording = true;
            await speechToTextViewModel.RecognizeSpeechFromMic();
            CartID = speechToTextViewModel.RecognizedText;
            IsRecording = false;
        }
        #endregion

        #region Pages
        public async Task ReturnToHome(object sender, RoutedEventArgs e)
        {
            var viewModel = new ViewModel();
            var view = new HomePageUC { DataContext = viewModel };

            var currentWindow = System.Windows.Application.Current.MainWindow;
            currentWindow.Content = view;
            currentWindow.Title = "Main Window";
            currentWindow.Width = 400;
            currentWindow.Height = 300;
        }

        public async Task OpenPickItemView()
        {
            var pickItemViewModel = new PickItemViewModel(CartID);
            var pickItemView = new PickItemUC { DataContext = pickItemViewModel };

            var currentWindow = System.Windows.Application.Current.MainWindow;
            currentWindow.Content = pickItemView;
            currentWindow.Title = "Pick Item";
            currentWindow.Width = 400;
            currentWindow.Height = 300;
        }
        #endregion

        public async Task HandleYesResponse()
        {
            // Implement the logic for handling "yes" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("Great, let's move on to the next item in PickItemViewModel");
        }

        public async Task HandleNoResponse()
        {
            // Implement the logic for handling "no" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("Please try again in PickItemViewModel");
        }

        public async Task HandleArrivedResponse()
        {
            // Implement the logic for handling "arrived" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("You said you've arrived in PickItemViewModel");
        }

        public async Task HandlePickedItemResponse()
        {
            // Implement the logic for handling "picked item" response in PickItemViewModel
            await textToSpeechViewModel.SynthesizeSpeech("You said you've picked the item in PickItemViewModel");
        }
    }
}