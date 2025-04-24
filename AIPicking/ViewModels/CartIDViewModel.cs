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
using System.Windows.Media;

namespace AIPicking.ViewModels
{
    public class CartIDViewModel : INotifyPropertyChanged
    {
        #region Properties
        private readonly TextToSpeechViewModel textToSpeechViewModel;
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

        private bool isRecording;
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

        private string scanCartIDText = "Scan Cart ID";
        public string ScanCartIDText
        {
            get { return scanCartIDText; }
            set
            {
                scanCartIDText = value;
                OnPropertyChanged();
            }
        }

        private string cartIDLabelText = "Cart ID:";
        public string CartIDLabelText
        {
            get { return cartIDLabelText; }
            set
            {
                cartIDLabelText = value;
                OnPropertyChanged();
            }
        }

        private string enterButtonText = "Enter";
        public string EnterButtonText
        {
            get { return enterButtonText; }
            set
            {
                enterButtonText = value;
                OnPropertyChanged();
            }
        }

        private readonly TranslatorViewModel translatorViewModel;
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
        #endregion
        public CartIDViewModel(string recognizedLang)
        {
            RecognizedLang=recognizedLang;
            _intentViewModel = new IntentViewModel();
            speechToTextViewModel = new SpeechToTextViewModel();
            textToSpeechViewModel = new TextToSpeechViewModel();

            AnalyzeCommand = new RelayCommand(async () => await _intentViewModel.AnalyzeConversationAsync(CartID, recognizedLang));
            ReturnToHomeCommand = new RelayCommand(async () => await ReturnToHome(null, null));
            EnterCommand = new RelayCommand(async () => await OpenPickItemView(RecognizedLang));

            translatorViewModel = new TranslatorViewModel();

            InitializeAsync(recognizedLang);
        }
        #region Tasks
        private async Task InitializeAsync(string recognizedLang)
        {
            if (recognizedLang == "en") { await SpeakCartID(recognizedLang); }
            else if (recognizedLang == "es")
            {
                TranslateToSpanish();
                await SpeakCartID(recognizedLang);
            }

            
        }
        public async Task SpeakCartID(string RecognizedLang)
        {
            string message = "Say the CartID";
            if (RecognizedLang == "en")
            {
                await textToSpeechViewModel.SynthesizeSpeech(message,RecognizedLang);
            }
            else if (RecognizedLang == "es")
            {
                await translatorViewModel.TranslateTextToSpanish(message);
                message = translatorViewModel.TranslationResult;
                await textToSpeechViewModel.SynthesizeSpeech(message, RecognizedLang);
            }

            IsRecording = true;
            await speechToTextViewModel.RecognizeSpeechFromMic();
            CartID = speechToTextViewModel.RecognizedText;

            IsRecording = false;
            OpenPickItemView(RecognizedLang);
        }
        private void TranslateToSpanish()
        {
            ScanCartIDText = "Escanear ID del carrito";
            CartIDLabelText = "ID del carrito:";
            EnterButtonText = "Ingresar";
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

        public async Task OpenPickItemView(string RecognizedLang)
        {
            var pickItemViewModel = new PickItemViewModel(CartID, RecognizedLang);
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