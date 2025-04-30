using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static AIPicking.ViewModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;

namespace AIPicking.ViewModels
{
    class TextToSpeechViewModel
    {
        private static string speechKey = "8xzDB1l9OOZGb5CLKHjS82qhnAPeVV31yKZqDAyTmde0A98lbYcRJQQJ99BBACYeBjFXJ3w3AAAYACOGlizV";
        private static string speechRegion = "eastus";

        public ICommand SynthesizeSpeechCommand { get; }

        public TextToSpeechViewModel()
        {

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

        #region Tasks
        public async Task SynthesizeSpeech(string text,string RecognizedLang)
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechSynthesisVoiceName = "en-US-GuyNeural";
            if (RecognizedLang == "es")
            {
                speechConfig.SpeechSynthesisVoiceName = "es-ES-AlvaroNeural";
            }

                using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }
        }

        public async Task SynthesizeAllInfo(
            string CartID, string Title, string Quantity, string Location, string Description, string ItemsLeft, string SerialNumber,
            string CartIDLabel, string TitleLabel, string QuantityLabel, string LocationLabel, string DescriptionLabel, string ItemsLeftLabel, string SerialNumberLabel,
            string RecognizedLang, TranslatorViewModel translatorViewModel)
        {
            // Translate labels if necessary
            if (RecognizedLang == "es")
            {
                CartIDLabel = await TranslateLabel(CartIDLabel, translatorViewModel);
                TitleLabel = await TranslateLabel(TitleLabel, translatorViewModel);
                QuantityLabel = await TranslateLabel(QuantityLabel, translatorViewModel);
                LocationLabel = await TranslateLabel(LocationLabel, translatorViewModel);
                DescriptionLabel = await TranslateLabel(DescriptionLabel, translatorViewModel);
                ItemsLeftLabel = await TranslateLabel(ItemsLeftLabel, translatorViewModel);
                SerialNumberLabel = await TranslateLabel(SerialNumberLabel, translatorViewModel);
            }

            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechSynthesisVoiceName = RecognizedLang == "es" ? "es-ES-AlvaroNeural" : "en-US-GuyNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                string text = $"{TitleLabel}: {Title},{LocationLabel}: {Location},  {ItemsLeftLabel}: {ItemsLeft}";
                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }

            // Ask if the user is at the shelf
            await AskIfAtShelf(RecognizedLang);
        }

        private async Task<string> TranslateLabel(string label, TranslatorViewModel translatorViewModel)
        {
            await translatorViewModel.TranslateTextToSpanish(label);
            return translatorViewModel.TranslationResult;
        }

        private async Task AskIfAtShelf(string RecognizedLang)
        {
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechSynthesisVoiceName = RecognizedLang == "es" ?  "es-ES-AlvaroNeural" : "en-US-GuyNeural";

            using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
            {
                string text = RecognizedLang == "es" ? "¿Estás en el estante?" : "Are you at the shelf?";
                var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
                OutputSpeechSynthesisResult(speechSynthesisResult, text);
            }
        }
        #endregion
    }
}
