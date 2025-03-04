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
            speechToTextViewModel = new SpeechToTextViewModel();

            RecognizeSpeechFromMicCommand = new RelayCommand(async () =>
            {
                 speechToTextViewModel.RecognizeSpeechFromMic();
                Console.WriteLine($"Recognized Text: {speechToTextViewModel.RecognizedText}");
                CartID = speechToTextViewModel.RecognizedText;
                Console.WriteLine($"CartID set to: {CartID}");
            });

            ReturnToHomeCommand = new RelayCommand(async () => await ReturnToHome(null, null));
            EnterCommand = new RelayCommand(async () => await OpenPickItemView());
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