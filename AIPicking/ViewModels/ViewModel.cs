using AIPicking.ViewModels;
using AIPicking.Views;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AIPicking
{
    public class ViewModel : INotifyPropertyChanged
    {
        #region Properties
        private readonly TextToSpeechViewModel textToSpeechViewModel;
        private readonly SpeechToTextViewModel speechToTextViewModel;
        private readonly TranslatorViewModel translatorViewModel;

       
        private string textBoxValue;
        
        public string TextBoxValue
        {
            get { return textBoxValue; }
            set
            {
                textBoxValue = value;
                OnPropertyChanged();
            }
        }
        private string enterCartIDValue;
        public string EnterCartIDValue
        {
            get { return enterCartIDValue; }
            set
            {
                enterCartIDValue = value;
                OnPropertyChanged();
            }
        }

        private string ticketNumber;
        public string TicketNumber
        {
            get { return ticketNumber; }
            set
            {
                ticketNumber = value;
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
        private string recognizedText;
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
        public ICommand RecognizeSpeechFromMicCommand { get; }
        public ICommand OpenScanCartIDViewCommand { get; }
        public ICommand OpenPickItemViewCommand { get; }
        public ICommand SynthesizeSpeechCommand { get; }
        public ICommand TranslateCommand { get; }

        #endregion
        public ViewModel()
        {
            EnterCartIDValue = "Scan Cart ID";
            textToSpeechViewModel = new TextToSpeechViewModel();
            speechToTextViewModel = new SpeechToTextViewModel();
            translatorViewModel = new TranslatorViewModel();
            SynthesizeSpeechCommand = new RelayCommand(async () => await textToSpeechViewModel.SynthesizeSpeech(TextBoxValue, RecognizedLang));
            TranslateCommand = new RelayCommand(async () =>
            {
                await LanguageDecision();
                if (RecognizedLang == "es")
                    await translatorViewModel.TranslateTextToSpanish(EnterCartIDValue);

                EnterCartIDValue = translatorViewModel.TranslationResult;
            });
            RecognizeSpeechFromMicCommand = new RelayCommand(async () =>
            {
                IsRecording = true;
                await speechToTextViewModel.RecognizeSpeechFromMic();
                RecognizedText = speechToTextViewModel.RecognizedText;
                isRecording = false;
            });
            OpenScanCartIDViewCommand = new RelayCommand(async () => await OpenScanCartIDView(null, null));
            InitializeAsync();
        }
        private async Task InitializeAsync()
        {
            await LanguageDecision();
        }
        #region Tasks

        public async Task LanguageDecision()
        {
            await textToSpeechViewModel.SynthesizeSpeech("What language would you like to continue in?", RecognizedLang);
            IsRecording = true;
            await speechToTextViewModel.RecognizeSpeechFromMic();
            RecognizedLang = speechToTextViewModel.RecognizedLang; // Set the recognized language
            IsRecording = false;
            if (RecognizedLang == "en") return;
            else if (RecognizedLang == "es")
                await translatorViewModel.TranslateTextToSpanish(enterCartIDValue);
            EnterCartIDValue = translatorViewModel.TranslationResult;
        }

        public async Task OpenScanCartIDView(object sender, RoutedEventArgs e)
        {
            var cartIDViewModel = new CartIDViewModel(RecognizedLang);
            var cartIDView = new CartID { DataContext = cartIDViewModel };

            var currentWindow = System.Windows.Application.Current.MainWindow;
            currentWindow.Content = cartIDView;
            currentWindow.Title = "Scan Cart ID";
            currentWindow.Width = 800;
            currentWindow.Height = 600;
        }

        #endregion
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
}
