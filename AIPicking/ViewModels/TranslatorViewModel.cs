using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using static AIPicking.ViewModel;

namespace AIPicking.ViewModels
{
    public class TranslatorViewModel : INotifyPropertyChanged
    {
        private static readonly string key = "C4qX7N4RWfunc96zUkwaTsbDncjs2quphetSWNE7KAGkhObiXqvLJQQJ99BCACYeBjFXJ3w3AAAbACOGjFWj";
        private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com";
        private static readonly string location = "eastus";

        public TranslatorViewModel()
        {
            TranslateCommand = new RelayCommand(async () => await TranslateTextToSpanish("hello"));
        }

        public ICommand TranslateCommand { get; }

        private async Task TranslateTextToSpanish(string textToTranslate)
        {
            string route = "/translate?api-version=3.0&from=en&to=es";
           
            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }
        }

        private async Task TranslateTextToEnglish(string textToTranslate)
        {
            string route = "/translate?api-version=3.0&from=es&to=en";

            object[] body = new object[] { new { Text = textToTranslate } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(endpoint + route);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", location);

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
