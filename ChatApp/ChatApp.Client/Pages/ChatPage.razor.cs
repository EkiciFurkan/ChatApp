using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client.Pages
{
    public class ChatPageHome : ComponentBase
    {
        [Inject] private NavigationManager? Navigation { get; set; }

        private HubConnection? _hubConnection;
        private readonly List<string> _messages = [];
        
        protected bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        protected string? UserName { get; set; }
        protected string? Message { get; set; }
        protected IReadOnlyList<string> Messages => _messages.AsReadOnly();

        protected override async Task OnInitializedAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Navigation!.ToAbsoluteUri("/chatHub"))
                .Build();

            _hubConnection.On<string, string, DateTime>("ReceiveMessage", (userName, message, date) =>
            {
                var formattedMessage = $"{Environment.NewLine}{userName}{Environment.NewLine}{message} - {date:HH:mm:ss}";
                Message = string.Empty;
                _messages.Add(formattedMessage);
                InvokeAsync(StateHasChanged);
            });

            await _hubConnection.StartAsync();
        }

        protected async Task SendMessage()
        {
            if (_hubConnection == null)
            {
                Console.WriteLine("Hub connection is not initialized.");
                return;
            }
            try
            {
                await _hubConnection.SendAsync("SendMessage", UserName, Message, DateTime.Now);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while sending the message: {ex.Message}");
            }
        }
    }
}