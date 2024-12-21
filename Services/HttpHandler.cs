using DatsNewWay.Models;
using Newtonsoft.Json;
using System.Text;


namespace DatsNewWay.Services
{
    public class HttpHandler
    {

        public delegate void Manager(GameState gameState);
        public event Manager Handler;

        private readonly string token = "023e493c-b99e-45a0-bb3a-6a840957ad15";
        private readonly dynamic emptyBody = new
        {
            transports = new object[] { }
        };

        //private const string baseAddress = "https://games-test.datsteam.dev/"; // Тестовый
        private const string baseAddress = "https://games.datsteam.dev/ "; // Основной

        private HttpClient _httpClient;
        private Moving _moving;

        private int _count = 0;

        public HttpHandler()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", token);

            _moving = new();

            Handler += MoveTransport;
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                await ProccessAsync();
            });
        }

        public Point3D GetTargetPoint(string id)
        {
            if (_moving.targetPoint.TryGetValue(id, out var targetPoint))
            {
                return targetPoint;
            }

            return null;
        }
        private async Task ProccessAsync()
        {
            while (true)
            {
                try
                {
                    await GetStocInfo();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public async Task<GameRoundsResponse> GetRoundInfo()
        {
            var response = await _httpClient.GetAsync("rounds/snake3d");
            if (response == null)
            {
                throw new Exception("null");
            }
            Console.WriteLine("Тело получено");

            string content = await response.Content.ReadAsStringAsync();
            try
            {
                var gameRoundsResponse = JsonConvert.DeserializeObject<GameRoundsResponse>(content);
                return gameRoundsResponse;

            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private async Task GetStocInfo()
        {
            var response = await _httpClient.PostAsync("play/snake3d/player/move", GetHttpContent(emptyBody));
            if (response == null)
                return;

            Console.WriteLine("Тело получено");

            string content = await response.Content.ReadAsStringAsync();
            try
            {
                var gameState = JsonConvert.DeserializeObject<GameState>(content);

                if (gameState?.snakes == null)
                {
                    return;
                }

                Handler?.Invoke(gameState);

                await Task.Delay(gameState != null ? gameState.tickRemainMs : 500);
            }
            catch (Exception ex)
            {

            }
        }


        private async void MoveTransport(GameState gameState)
        {
            var moveList = await _moving.GetNewDirection(gameState);

            dynamic body = new
            {
                snakes = moveList
            };

            var response = await _httpClient.PostAsync("play/snake3d/player/move", GetHttpContent(body));
            if (response == null)
                return;
        }

        private dynamic GetHttpContent(dynamic body)
        {
            string JsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            dynamic httpContnet = new StringContent(JsonBody, Encoding.UTF8, "application/json");

            return httpContnet;
        }

    }
}
