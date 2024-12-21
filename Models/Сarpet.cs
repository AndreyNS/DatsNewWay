namespace DatsNewWay.Models
{
    public class StaticData
    {
        public const int XSize = 180;
        public const int YSize = 180;
        public const int ZSize = 60;
    }

    public class SnakeBase
    {
        public string id { get; set; }
        public List<int> direction { get; set; }
    }

    public class SnakeRequest
    {
        public List<SnakeBase> snakes { get; set; }
    }

    public class Snake : SnakeBase
    {
        public List<int> oldDirection { get; set; }
        public List<List<int>> geometry { get; set; }
        public int deathCount { get; set; }
        public string status { get; set; }
        public string reviveRemainMs { get; set; }

    }

    public class Enemy
    {
        public List<List<int>> geometry { get; set; }
        public string status { get; set; }
        public int kills { get; set; }
    }

    public class Food
    {
        public List<int> c { get; set; }
        public int points { get; set; }
    }

    public class SpecialFood
    {
        public List<List<int>> golden { get; set; }
        public List<List<int>> suspicious { get; set; }
    }

    public class GameState
    {
        public List<int> mapSize { get; set; }
        public string name { get; set; }
        public int points { get; set; }
        public List<List<int>> fences { get; set; }
        public List<Snake> snakes { get; set; }
        public List<Enemy> enemies { get; set; }
        public List<Food> food { get; set; }
        public SpecialFood specialFood { get; set; }
        public int turn { get; set; }
        public int reviveTimeoutSec { get; set; }
        public int tickRemainMs { get; set; }
        public List<int> errors { get; set; }

    }


    // Готово

    public class Round
    {
        public string name { get; set; }
        public string startAt { get; set; }
        public string endAt { get; set; }
        public int duration { get; set; }
        public string status { get; set; }
        public int repeat { get; set; }
    }

    public class GameRoundsResponse
    {
        public string gameName { get; set; }
        public List<Round> rounds { get; set; }
    }

}
