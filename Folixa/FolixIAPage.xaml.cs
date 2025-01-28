using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GroqNet;
using GroqNet.ChatCompletions;

namespace Folixa
{
    public partial class FolixIAPage : ContentPage
    {
        public ObservableCollection<Message> Messages { get; set; }
        private Conexion _conexion;
        private GroqClient _groqClient;

        public FolixIAPage()
        {
            InitializeComponent();
            Messages = new ObservableCollection<Message>();
            MessagesCollectionView.ItemsSource = Messages;
            _conexion = new Conexion();
            _groqClient = new GroqClient("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", GroqModel.LLaMA3_70b, null, null);
        }

        private async void OnSendButtonClicked(object sender, EventArgs e)
        {
            // Añadir mensaje del usuario
            var userMessage = new Message
            {
                Text = MessageEntry.Text,
                Style = (Style)Resources["UserMessageStyle"]
            };
            Messages.Add(userMessage);

            // Obtener respuesta del bot
            var botResponse = await GetBotResponseAsync(MessageEntry.Text);
            var botMessage = new Message
            {
                Text = botResponse,
                Style = (Style)Resources["BotMessageStyle"]
            };
            Messages.Add(botMessage);

            MessageEntry.Text = string.Empty;
        }

        private async Task<string> GetBotResponseAsync(string userMessage)
        {
            try
            {
                // Obtener todos los datos de la base de datos
                var discotecas = await _conexion.ObtenerDiscotecasAsync();
                
                if (discotecas == null || discotecas.Count == 0)
                {
                    return "No hay datos de discotecas disponibles en este momento.";
                }

                string discotecasString = string.Join(", ", discotecas.Select(d => $"{d.Nombre}: {d.Valoracion}, {d.Ubicacion}, {d.Descripcion}, {d.Horario}"));

                // Preparar los datos para la API de Groq
                var requestData = new List<GroqMessage>
        {
            new GroqMessage { Role = "system", Content = "Eres un chatbot de recomendación de discotecas en Gijón. Responde a preguntas sobre las discotecas de Gijón." },
            new GroqMessage { Role = "user", Content = $"pregunta: {userMessage}, datos: {discotecasString}" }
        };

                // Llamada a la API de Groq
                var response = await _groqClient.GetChatCompletionsAsync(requestData);

                // Validar la respuesta de la API
                if (response != null && response.Choices != null && response.Choices.Count > 0)
                {
                    return response.Choices[0].Message.Content;
                }
                else
                {
                    return "La API de Groq no devolvió una respuesta válida. Verifica los datos enviados.";
                }
            }
            catch (Exception ex)
            {
                // Error inesperado
                return $"Ocurrió un error inesperado: {ex.Message}";
            }
        }
    }

    public class Message
    {
        public string Text { get; set; }
        public Style Style { get; set; }
    }

    public class GroqResponse
    {
        public string Answer { get; set; }
    }
}
