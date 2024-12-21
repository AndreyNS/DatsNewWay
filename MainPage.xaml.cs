using DatsNewWay.Models;
using DatsNewWay.Services;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Timers;

namespace DatsNewWay
{
    public partial class MainPage : ContentPage
    {
        private SKPoint translate = new SKPoint(0, 0);
        private float cubeSize = StaticData.XSize;
        private bool isRotating = false;
        private bool isPanning = false;
        private float rotationX = 0;
        private float rotationY = 0;
        private float rotationZ = 0;
        private float scale = 1f;

        private System.Timers.Timer timer;
        private SKPoint lastMousePosition;
        private SKPoint lastTouchPoint;
        private HttpHandler httpHandler;
        private GameState _gameState;


        private List<(float x, float y, float z, SKColor color)> elements = new List<(float, float, float, SKColor)>
        {
            (10, 10, 10, SKColors.Green),
            (15, 10, 10, SKColors.Green),
            (20, 10, 10, SKColors.Green),

            (5, 25, 5, SKColors.Yellow),
            (30, 15, 10, SKColors.Yellow),

            (20, 20, 20, SKColors.Red),
            (25, 25, 25, SKColors.Red),

            (10, 30, 15, SKColors.Gray),
            (15, 15, 30, SKColors.Gray),
            (70, 70, 70, SKColors.Gray),
        };

        SKPaint paintSnake, paintHead, paintHeadEnemy, paintEnemy, paintGold, paintOrange, paintFences;


        public MainPage()
        {
            InitializeComponent();

            httpHandler = new HttpHandler();
            timer = new System.Timers.Timer(5000);

            paintSnake = new SKPaint
            {
                Color = SKColors.Green,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f
            };

            paintHead = new SKPaint
            {
                Color = SKColors.DarkSeaGreen,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f
            };

            paintHeadEnemy = new SKPaint
            {
                Color = SKColors.OrangeRed,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f
            };

            paintEnemy = new SKPaint
            {
                Color = SKColors.Red,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f
            };

            paintGold = new SKPaint
            {
                Color = SKColors.Gold,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f
            };

            paintOrange = new SKPaint
            {
                Color = SKColors.Orange,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f
            };

            paintFences = new SKPaint
            {
                Color = SKColors.Gray,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                StrokeWidth = 0.5f
            };
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            httpHandler.Handler += HttpHandler_Handler;
            httpHandler.Start();

            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            var roundInfo = await httpHandler.GetRoundInfo();

            try
            {
                var activeRound = roundInfo.rounds.FirstOrDefault(f => f.status == "active");
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    nameround.Text = activeRound?.name;
                    duration.Text = activeRound?.duration.ToString();
                    endat.Text = activeRound?.endAt.ToString();
                });
            }
            catch (Exception ex)
            {

            }
        }

        private void HttpHandler_Handler(GameState gameState)
        {
            if (gameState != null && !gameState.snakes.Count().Equals(0))
            {
                _gameState = gameState;
            }

            SetSnakeData();
            MapCanvasView.InvalidateSurface();
        }

        private void SetSnakeData()
        {
            var snakes = _gameState.snakes;

            try
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    try
                    {
                        if (snakes[0] != null)
                        {
                            IdOneSnake.Text = snakes[0].id;
                            LenghtOneSnake.Text = snakes[0]?.geometry?.Count.ToString();
                            DeathOneSnake.Text = snakes[0]?.deathCount.ToString();
                            RemainOneSnake.Text = snakes[0]?.reviveRemainMs;
                            StatusOneSnake.Text = snakes[0]?.status;

                            if (snakes[0]?.geometry != null && !snakes[0].geometry.Count().Equals(0))
                            {

                                XOneSnake.Text = snakes[0]?.geometry?[0][0].ToString();
                                YOneSnake.Text = snakes[0]?.geometry?[0][1].ToString();
                                ZOneSnake.Text = snakes[0]?.geometry?[0][2].ToString();

                            }
                            var targetOne = httpHandler.GetTargetPoint(snakes[0].id);
                            TargetOneSnake.Text = $"{targetOne?.X} {targetOne?.Y} {targetOne?.Z}";
                            MoveOneSnake.Text = $"{snakes[0]?.direction?[0]} {snakes[0]?.direction?[1]} {snakes[0]?.direction?[2]}";
                        }
                    }
                    catch { }

                    try
                    {
                        IdTwoSnake.Text = snakes[1].id;
                        LenghtTwoSnake.Text = snakes[1]?.geometry?.Count.ToString();
                        DeathTwoSnake.Text = snakes[1]?.deathCount.ToString();
                        RemainTwoSnake.Text = snakes[1]?.reviveRemainMs;
                        StatusTwoSnake.Text = snakes[1]?.status;

                        if (snakes[1]?.geometry != null && !snakes[1].geometry.Count().Equals(0))
                        {
                            XTwoSnake.Text = snakes[1]?.geometry?[0][0].ToString();
                            YTwoSnake.Text = snakes[1]?.geometry?[0][1].ToString();
                            ZTwoSnake.Text = snakes[1]?.geometry?[0][2].ToString();
                        }
                       

                        var targetTwo = httpHandler.GetTargetPoint(snakes[1]?.id);
                        TargetTwoSnake.Text = $"{targetTwo?.X} {targetTwo?.Y} {targetTwo?.Z}";
                        MoveTwoSnake.Text = $"{snakes[1]?.direction?[0]} {snakes[1]?.direction?[1]} {snakes[1]?.direction?[2]}";
                    }
                    catch { }

                    try
                    {
                        IdThreeSnake.Text = snakes[2]?.id;
                        LenghtThreeSnake.Text = snakes[2]?.geometry?.Count.ToString();
                        DeathThreeSnake.Text = snakes[2]?.deathCount.ToString();
                        RemainThreeSnake.Text = snakes[2]?.reviveRemainMs;
                        StatusThreeSnake.Text = snakes[2]?.status;

                        if (snakes[2]?.geometry != null && !snakes[2].geometry.Count().Equals(0))
                        {
                            XThreeSnake.Text = snakes[2]?.geometry?[0][0].ToString();
                            YThreeSnake.Text = snakes[2]?.geometry?[0][1].ToString();
                            ZThreeSnake.Text = snakes[2]?.geometry?[0][2].ToString();
                        }


                        var targetThree = httpHandler.GetTargetPoint(snakes[2]?.id);
                        TargetThreeSnake.Text = $"{targetThree?.X} {targetThree?.Y} {targetThree?.Z}";
                        MoveThreeSnake.Text = $"{snakes[2]?.direction?[0]} {snakes[2]?.direction?[1]} {snakes[2]?.direction?[2]}";
                    }
                    catch { }
                    try
                    {
                        points.Text = _gameState.points.ToString();
                        tickrem.Text = _gameState.tickRemainMs.ToString();
                        error.Text = _gameState.errors.Count().ToString();
                    }
                    catch { }
                });

            }
            catch (Exception ex)
            {

            }
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            var width = e.Info.Width;
            var height = e.Info.Height;
            canvas.Translate(width / 2, height / 2);
            canvas.Scale(scale);

            //DrawCube(canvas);

            DrawElements(canvas);
        }

        private void DrawElements(SKCanvas canvas)
        {
            if (_gameState == null)
            {
                return;
            }

            if (_gameState.snakes != null)
            {
                foreach (var snake in _gameState.snakes)
                {
                    try
                    {
                        if (snake.geometry != null)
                        {
                            foreach (var geo in snake.geometry)
                            {
                                var point = new SKPoint3(geo[0] - cubeSize / 2, geo[1] - cubeSize / 2, geo[2] - cubeSize / 2);
                                var rotatedPoint = RotatePoint(point, rotationX, rotationY, rotationZ);

                                if (snake.geometry.IndexOf(geo).Equals(0))
                                {

                                    canvas.DrawCircle(rotatedPoint.X, rotatedPoint.Y, 0.5f, paintHead);
                                }

                                canvas.DrawCircle(rotatedPoint.X, rotatedPoint.Y, 0.5f, paintSnake);

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            if (_gameState.enemies != null)
            {

                foreach (var enemy in _gameState.enemies)
                {

                    try
                    {
                        if (enemy.geometry != null)
                        {
                            foreach (var geo in enemy.geometry)
                            {
                                var point = new SKPoint3(geo[0] - cubeSize / 2, geo[1] - cubeSize / 2, geo[2] - cubeSize / 2);
                                var rotatedPoint = RotatePoint(point, rotationX, rotationY, rotationZ);

                                if (enemy.geometry.IndexOf(geo).Equals(0))
                                {

                                    canvas.DrawCircle(rotatedPoint.X, rotatedPoint.Y, 0.5f, paintHeadEnemy);
                                }

                                canvas.DrawCircle(rotatedPoint.X, rotatedPoint.Y, 0.5f, paintEnemy);

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            if (_gameState.food != null)
            {
                foreach (var food in _gameState.food)
                {
                    try
                    {
                        var point = new SKPoint3(food.c[0] - cubeSize / 2, food.c[1] - cubeSize / 2, food.c[2] - cubeSize / 2);
                        var rotatedPoint = RotatePoint(point, rotationX, rotationY, rotationZ);
                        canvas.DrawCircle(rotatedPoint.X, rotatedPoint.Y, 0.5f, paintOrange);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            if (_gameState.specialFood != null && _gameState.specialFood.golden != null && !_gameState.specialFood.golden.Equals(0))
            {
                foreach (var food in _gameState.specialFood.golden)
                {
                    try
                    {
                        var point = new SKPoint3(food[0] - cubeSize / 2, food[1] - cubeSize / 2, food[2] - cubeSize / 2);
                        var rotatedPoint = RotatePoint(point, rotationX, rotationY, rotationZ);
                        canvas.DrawCircle(rotatedPoint.X, rotatedPoint.Y, 0.5f, paintGold);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            //if (_gameState.fences != null)
            //{
            //    foreach (var fence in _gameState.fences)
            //    {
            //        try
            //        {
            //            var point = new SKPoint3(fence[0] - cubeSize / 2, fence[1] - cubeSize / 2, fence[2] - cubeSize / 2);
            //            var rotatedPoint = RotatePoint(point, rotationX, rotationY, rotationZ);
            //            canvas.DrawCircle(rotatedPoint.X, rotatedPoint.Y, 0.5f, paintFences);
            //        }
            //        catch (Exception ex)
            //        {

            //        }

            //    }
            //}

        }

        private void DrawCube(SKCanvas canvas)
        {
            using (var paint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 1,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                float halfSize = cubeSize / 2;
                SKPoint3[] vertices = new SKPoint3[]
                {
                    new SKPoint3(-halfSize, -halfSize, -halfSize),
                    new SKPoint3(halfSize, -halfSize, -halfSize),
                    new SKPoint3(halfSize, halfSize, -halfSize),
                    new SKPoint3(-halfSize, halfSize, -halfSize),
                    new SKPoint3(-halfSize, -halfSize, halfSize),
                    new SKPoint3(halfSize, -halfSize, halfSize),
                    new SKPoint3(halfSize, halfSize, halfSize),
                    new SKPoint3(-halfSize, halfSize, halfSize)
                };

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = RotatePoint(vertices[i], rotationX, rotationY, rotationZ);

                }

                int[,] edges = new int[,]
                {
                    { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 },
                    { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 },
                    { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 }
                };


                for (int i = 0; i < edges.GetLength(0); i++)
                {
                    var start = vertices[edges[i, 0]];
                    var end = vertices[edges[i, 1]];
                    canvas.DrawLine(start.X, start.Y, end.X, end.Y, paint);
                }

            }
        }

        private SKPoint3 RotatePoint(SKPoint3 point, float angleX, float angleY, float angleZ)
        {
            float radX = DegreesToRadians(angleX);
            float radY = DegreesToRadians(angleY);
            float radZ = DegreesToRadians(angleZ);

            float cosX = (float)Math.Cos(radX);
            float sinX = (float)Math.Sin(radX);

            float cosY = (float)Math.Cos(radY);
            float sinY = (float)Math.Sin(radY);

            float cosZ = (float)Math.Cos(radZ);
            float sinZ = (float)Math.Sin(radZ);

            float y1 = point.Y * cosX - point.Z * sinX;
            float z1 = point.Y * sinX + point.Z * cosX;

            float x2 = point.X * cosY + z1 * sinY;
            float z2 = -point.X * sinY + z1 * cosY;

            float x3 = x2 * cosZ - y1 * sinZ;
            float y3 = x2 * sinZ + y1 * cosZ;

            return new SKPoint3(x3, y3, z2);
        }


        private void OnTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    isRotating = true;
                    lastTouchPoint = e.Location;
                    break;

                case SKTouchAction.Moved:
                    if (isRotating)
                    {
                        var deltaX = e.Location.X - lastTouchPoint.X;
                        var deltaY = e.Location.Y - lastTouchPoint.Y;

                        rotationY += deltaX / 2;
                        rotationX -= deltaY / 2;

                        if (Math.Abs(deltaX) > Math.Abs(deltaY))
                        {
                            rotationZ += deltaX / 2;
                        }

                        lastTouchPoint = e.Location;
                        MapCanvasView.InvalidateSurface();
                    }
                    break;

                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    isRotating = false;
                    break;

                case SKTouchAction.WheelChanged:
                    scale *= e.WheelDelta > 0 ? 1.1f : 0.9f;
                    scale = Math.Clamp(scale, 5f, 50f);
                    MapCanvasView.InvalidateSurface();
                    break;
            }

            e.Handled = true;
        }

        private float DegreesToRadians(float degrees)
        {
            return (float)(degrees * Math.PI / 180);
        }
    }

    public struct SKPoint3
    {
        public float X;
        public float Y;
        public float Z;

        public SKPoint3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}