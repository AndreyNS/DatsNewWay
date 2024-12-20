using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Drawing;
using SkiaSharp;


namespace DatsNewWay.Services
{
    //public class HttpHandler
    //{
    //    private readonly string token = "66fbe8d1d550266fbe8d1d5508";
    //    private readonly dynamic emptyBody = new
    //    {
    //        transports = new object[] { }
    //    };

    //    //private const string baseAddress = "https://games-test.datsteam.dev/play/magcarp/player/"; // Тестовый
    //    private const string baseAddress = "https://games.datsteam.dev/play/magcarp/player/"; // Основной

    //    private Carpet Carpet { get; set; }

    //    private HttpClient _httpClient;
    //    private MappingVis _mappingVis;

    //    private int _count = 0;

    //    public HttpHandler(SKControl skControl)
    //    {
    //        _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
    //        _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", token);
    //        _mappingVis = new MappingVis(skControl);
    //    }

    //    public void Start()
    //    {
    //        Task.Run(async () =>
    //        {
    //            await ProccessAsync();
    //        });
    //    }

    //    private async Task ProccessAsync()
    //    {
    //        while (true)
    //        {
    //            try
    //            {
    //                await GetStocInfo();
    //                await MoveTransport();
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine(ex.Message);
    //            }
    //        }
    //    }

    //    private async Task GetStocInfo()
    //    {
    //        var response = await _httpClient.PostAsync("move", GetHttpContent(emptyBody));
    //        if (response == null)
    //        {
    //            return;
    //        }
    //        Console.WriteLine("Тело получено");

    //        string content = await response.Content.ReadAsStringAsync();
    //        var carpet = JsonConvert.DeserializeObject<Carpet>(content);
    //        Carpet = carpet;
    //        StaticData.transports = carpet.transports;


    //        DrawMap();
            
    //    }

    //    private async Task MoveTransport()
    //    {
    //        var moveList = await MovingLogic.WhyMove(Carpet);

    //        dynamic body = new
    //        {
    //            transports = moveList
    //        };

    //        var response = await _httpClient.PostAsync("move", GetHttpContent(body));
    //        if (response == null)
    //        {
    //            return;
    //        }
    //    }

    //    private dynamic GetHttpContent(dynamic body)
    //    {
    //        string JsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(body);
    //        dynamic httpContnet = new StringContent(JsonBody, Encoding.UTF8, "application/json");

    //        return httpContnet;
    //    }

    //}
}
