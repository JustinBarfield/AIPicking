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

            AnalyzeCommand = new RelayCommand(async () => await _intentViewModel.AnalyzeConversationAsync(CartID, RecognizedLang));
           
            ReturnToHomeCommand = new RelayCommand(async () => await ReturnToHome(null, null));
            EnterCommand = new RelayCommand(async () => await OpenPickItemView());

           
            InitializeAsync();
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
        private string recognizedLang;
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
        public async Task LanguageDecision()
        {
            await textToSpeechViewModel.SynthesizeSpeech("What language would you like to continue in?");
            IsRecording = true;
            await speechToTextViewModel.RecognizeSpeechFromMic();
            
            RecognizedLang = speechToTextViewModel.RecognizedLang; // Set the recognized language
            IsRecording = false;
        }
        public async Task SpeakCartID()
        {
            RecognizedLang = speechToTextViewModel.RecognizedLang; // Set the recognized language
            await textToSpeechViewModel.SynthesizeSpeech("Say the CartID");
            IsRecording = true;
            await speechToTextViewModel.RecognizeSpeechFromMic();
            CartID = speechToTextViewModel.RecognizedText;
            
            IsRecording = false;
        }
        #endregion

        #region Tasks
        private async Task InitializeAsync()
        {
            await LanguageDecision();
            await SpeakCartID();
        }
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
            var pickItemViewModel = new PickItemViewModel(CartID, recognizedLang);
            var pickItemView = new PickItemUC { DataContext = pickItemViewModel };

            var currentWindow = System.Windows.Application.Current.MainWindow;
            currentWindow.Content = pickItemView;
            currentWindow.Title = "Pick Item";
            currentWindow.Width = 500;
            currentWindow.Height = 800;
        }
        #endregion
    }
}