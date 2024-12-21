using DatsNewWay.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DatsNewWay.Services
{
    public class Moving
    {
        private int xCenter = StaticData.XSize / 2;
        private int yCenter = StaticData.YSize / 2;
        private int zCenter = StaticData.ZSize / 2;

        private const int fieldError = 30;

        private List<SnakeBase> _snakeBases;
        private CancellationTokenSource _cts;

        private GameState _gameState;
        private List<(Point3D, int points)> _normalFood;

        private Point3D pointCenter;

        public ConcurrentDictionary<string, Point3D> targetPoint = new();

        public async Task<List<SnakeBase>> GetNewDirection(GameState gameState)
        {
            try
            {
                var fatFood = gameState.specialFood != null && gameState.specialFood.suspicious != null
                    ? gameState.specialFood.suspicious.Select(s => (new Point3D(s[0], s[1], s[2]), 0))
                    : new List<(Point3D, int points)>();
                var allFood = gameState.food != null
                    ? gameState.food.Select(s => (new Point3D(s.c[0], s.c[1], s.c[2]), s.points))
                    : new List<(Point3D, int points)>();
                _normalFood = allFood.Except(allFood.Intersect(fatFood)).ToList();

                pointCenter = new Point3D(xCenter, yCenter, zCenter);

                _gameState = gameState;
                _snakeBases = new();
                _cts = new();

                try
                {
                    if (gameState.snakes == null)
                    {
                        return null;
                    }

                    Thread thread = new Thread(() => {
                        CalculationMovement(gameState.snakes[0]);
                    });
                    Thread thread1 = new Thread(() => {
                        CalculationMovement(gameState.snakes[1]);
                    });

                    Thread thread2 = new Thread(() => {
                        CalculationMovement(gameState.snakes[2]);
                    });

                    thread.Start();
                    thread1.Start();
                    thread2.Start();

                    thread.Join();
                    thread1.Join();
                    thread2.Join();
                    //Parallel.ForEach(gameState.snakes, async (snake, _cts) =>
                    //{
                    //    CalculationMovement(snake);

                    //});

                    return _snakeBases;
                }
                catch (Exception ex)
                {
                    _cts?.Cancel();
                    return _snakeBases;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void CalculationMovement(Snake snake)
        {
            if (snake.geometry == null || snake.geometry.Count().Equals(0))
            {
                return;
            }
            var headGeo = snake.geometry.First();

            SnakeBase snakeBase = new();
            snakeBase.id = snake.id;
            snakeBase.direction = new List<int> { 0, 0, 0 };

            Point3D headPoint = new(headGeo[0], headGeo[1], headGeo[2]);

            bool isCenter = true;

            switch (headGeo)
            {
                case var head when Math.Abs(headPoint.X - xCenter) > fieldError:
                    {
                        isCenter = false;
                        break;
                    }
                case var head when Math.Abs(headPoint.Y - yCenter) > fieldError:
                    {
                        isCenter = false;
                        break;
                    }
                case var head when Math.Abs(headPoint.Z - zCenter) > fieldError:
                    {
                        isCenter = false;
                        break;
                    }
            }

            var aroundEnemy = _gameState.enemies != null
                ? _gameState.enemies
                .Where(w => w.status == "alive")
                .SelectMany(s => s.geometry.Select(g => new Point3D(g[0], g[1], g[2])))
                .Where(w => Math.Abs(w.X - headPoint.X) <= 5
                    && Math.Abs(w.Y - headPoint.Y) <= 5
                    && Math.Abs(w.Z - headPoint.Z) <= 5)
                : new List<Point3D>();


            var geometrySnake = snake.geometry.Select(s => new Point3D(s[0], s[1], s[2])).ToList();
            bool isOneElemSnake = geometrySnake.Count().Equals(1) ? true : false;
            var fences = _gameState.fences.Select(s => new Point3D(s[0], s[1], s[2]));

            var aroundFood = _normalFood.Where(
                w => Math.Abs(w.Item1.X - headPoint.X) <= fieldError
                && Math.Abs(w.Item1.Y - headPoint.Y) <= fieldError
                && Math.Abs(w.Item1.Z - headPoint.Z) <= fieldError).ToList();

            var errorFoods = fences.Intersect(aroundFood.Select(s => s.Item1));
            aroundFood.Except(errorFoods.Select(s => (s, 0)));

            //var nearbyFoods = aroundFood
            //    .OrderBy(m => (Math.Abs(m.Item1.X - headPoint.X) + Math.Abs(m.Item1.Y - headPoint.Y) + Math.Abs(m.Item1.Y - headPoint.Y)))
            //    .Take(aroundFood.Count() >= 3 ? 3 : aroundFood.Count());

            //var nearbyFood = nearbyFoods.MaxBy(m => m.points);

            (Point3D, int points) nearbyFood;

            if (aroundFood.Count().Equals(0))
            {
                nearbyFood = (pointCenter, 0);
            }
            else
            {
                nearbyFood = aroundFood.MinBy(m => (Math.Abs(m.Item1.X - headPoint.X) + Math.Abs(m.Item1.Y - headPoint.Y) + Math.Abs(m.Item1.Y - headPoint.Y)));
            }
           

            //var nearbyFood = (FindClosestPointToDirection(headPoint, pointCenter, aroundFood.Select(s => s.Item1).ToList()),0);
            if (_gameState != null && _gameState.specialFood != null && _gameState.specialFood.golden != null && !_gameState.specialFood.golden.Count().Equals(0))
            {
                var goldenFood = _gameState.specialFood.golden
                    .Select(s => (new Point3D(s[0], s[1], s[2]), 0))
                    .MinBy(m => (Math.Abs(m.Item1.X - headPoint.X) + Math.Abs(m.Item1.Y - headPoint.Y) + Math.Abs(m.Item1.Z - headPoint.Y)));

                if (aroundFood.Contains(goldenFood))
                {
                    nearbyFood = goldenFood;
                }
            }

            if (targetPoint.TryGetValue(snake.id, out var oldPoint))
            {
                var tempTarget = aroundFood.FirstOrDefault(f => f.Item1.X == oldPoint.X && f.Item1.Y == oldPoint.Y && f.Item1.Z == oldPoint.Z);
                if (tempTarget.Item1 != null)
                {
                    var indexTarget = aroundFood.IndexOf(tempTarget);
                    aroundFood.RemoveAt(indexTarget);

                    if (oldPoint != pointCenter)
                    {
                        nearbyFood = (oldPoint, 0);
                    }
             
                }
                else
                {
                    aroundFood.Remove(nearbyFood);
                    targetPoint.TryUpdate(snake.id, nearbyFood.Item1, oldPoint);
                }
            }
            else
            {
                targetPoint.TryAdd(snake.id, nearbyFood.Item1);
            }


            var nearbyEnemy = aroundEnemy.MinBy(m => (Math.Abs(m.X - headPoint.X) + Math.Abs(m.Y - headPoint.Y) + Math.Abs(m.Y - headPoint.Y)));
            List<Point3D> dangerPoint = new List<Point3D>();

            if (nearbyEnemy != null)
            {
                dangerPoint.Add(new Point3D(nearbyEnemy.X + 1, nearbyEnemy.Y, nearbyEnemy.Z));
                dangerPoint.Add(new Point3D(nearbyEnemy.X - 1, nearbyEnemy.Y, nearbyEnemy.Z));
                dangerPoint.Add(new Point3D(nearbyEnemy.X, nearbyEnemy.Y + 1, nearbyEnemy.Z));
                dangerPoint.Add(new Point3D(nearbyEnemy.X, nearbyEnemy.Y - 1, nearbyEnemy.Z));
                dangerPoint.Add(new Point3D(nearbyEnemy.X, nearbyEnemy.Y, nearbyEnemy.Z + 1));
                dangerPoint.Add(new Point3D(nearbyEnemy.X, nearbyEnemy.Y, nearbyEnemy.Z - 1));
                aroundEnemy.ToList().AddRange(dangerPoint);
            }

            //var otherSnakes = _gameState.snakes;
            //otherSnakes.Remove(snake);

            //try
            //{
            //    if (!otherSnakes.Count().Equals(0))
            //    {
            //        foreach (var otherSnake in otherSnakes)
            //        {
            //            var otherGeo = otherSnake.geometry.Select(s => new Point3D(s[0], s[1], s[2])).ToList();
            //            aroundEnemy.ToList().AddRange(otherGeo);
            //        }
            //    }
            //}
            //catch { }

            var shortedPath = CalculateShortestPath(headPoint, nearbyFood.Item1, maxSteps: 2, obstacles: aroundEnemy.ToHashSet(),
                collectibles: aroundFood.Select(s => s.Item1).ToHashSet(), restrictedPoints: isOneElemSnake ? null : geometrySnake);
            var intersectList = shortedPath.Intersect(fences);

            while (intersectList.Any())
            {
                shortedPath = CalculateShortestPath(headPoint, nearbyFood.Item1, obstacles: intersectList.Union(aroundEnemy).ToHashSet(),
                    maxSteps: 2, collectibles: aroundFood.Select(s => s.Item1).ToHashSet(), restrictedPoints: isOneElemSnake ? null : geometrySnake);
                intersectList = shortedPath.Intersect(fences);
            }

            var nextStep = shortedPath.Skip(1).First();
            snakeBase.direction[0] = nextStep.X - headPoint.X;
            snakeBase.direction[1] = nextStep.Y - headPoint.Y;
            snakeBase.direction[2] = nextStep.Z - headPoint.Z;

            lock (_snakeBases)
            {
                _snakeBases.Add(snakeBase);
            }

        }


        public  Point3D FindClosestPointToDirection(Point3D positionA, Point3D centerB, List<Point3D> points)
        {
            if (points == null || points.Count == 0)
            {
                return centerB;
            }

            var directionX = centerB.X - positionA.X;
            var directionY = centerB.Y - positionA.Y;
            var directionZ = centerB.Z - positionA.Z;

            var magnitude = Math.Sqrt(directionX * directionX + directionY * directionY + directionZ * directionZ);
            if (magnitude == 0)
            {
                throw new ArgumentException("Точка A и центр B совпадают, направление не определено.");
            }

            var normalizedDirX = directionX / magnitude;
            var normalizedDirY = directionY / magnitude;
            var normalizedDirZ = directionZ / magnitude;

            return points
                .OrderBy(point =>
                {
                    var toPointX = point.X - positionA.X;
                    var toPointY = point.Y - positionA.Y;
                    var toPointZ = point.Z - positionA.Z;

                    var dotProduct = toPointX * normalizedDirX + toPointY * normalizedDirY + toPointZ * normalizedDirZ;

                    var distanceToLine = Math.Sqrt(
                        toPointX * toPointX + toPointY * toPointY + toPointZ * toPointZ - dotProduct * dotProduct
                    );

                    return distanceToLine; 
                })
                .First();
        }

        private List<Point3D> CalculateShortestPath(
        Point3D pointA,
        Point3D pointB,
        HashSet<Point3D> obstacles = null,
        int? maxSteps = null,
        HashSet<Point3D> collectibles = null,
        List<Point3D> restrictedPoints = null)
        {
            List<Point3D> path = new List<Point3D>();

            int x = pointA.X;
            int y = pointA.Y;
            int z = pointA.Z;

            int targetX = pointB.X;
            int targetY = pointB.Y;
            int targetZ = pointB.Z;

            obstacles ??= new HashSet<Point3D>();
            collectibles ??= new HashSet<Point3D>();
            restrictedPoints ??= new List<Point3D>();
            int steps = 0;

            void AddPoint(int currentX, int currentY, int currentZ)
            {
                var currentPoint = new Point3D(currentX, currentY, currentZ);
                path.Add(currentPoint);
                steps++;

                if (collectibles.Contains(currentPoint))
                {
                    collectibles.Remove(currentPoint);
                }

                restrictedPoints.Insert(0, currentPoint);
                if (restrictedPoints.Count > 2)
                {
                    restrictedPoints.RemoveAt(restrictedPoints.Count - 1);
                }
            }

            while (x != targetX)
            {
                AddPoint(x, y, z);
                if (maxSteps.HasValue && steps >= maxSteps.Value)
                    return path;

                int nextX = x + (x < targetX ? 1 : -1);
                var nextPoint = new Point3D(nextX, y, z);

                if (obstacles.Contains(nextPoint) || restrictedPoints.Contains(nextPoint))
                {
                    int bypassY = y + 1;
                    if (!obstacles.Contains(new Point3D(x, bypassY, z)) && !restrictedPoints.Contains(new Point3D(x, bypassY, z)))
                    {
                        y = bypassY;
                    }
                    else
                    {
                        int bypassZ = z + 1;
                        if (!obstacles.Contains(new Point3D(x, y, bypassZ)) && !restrictedPoints.Contains(new Point3D(x, y, bypassZ)))
                        {
                            z = bypassZ;
                        }
                        else
                        {
                            throw new Exception("Маршрут невозможен из-за препятствий.");
                        }
                    }
                }
                else
                {
                    x = nextX;
                }
            }

            while (y != targetY)
            {
                AddPoint(x, y, z);
                if (maxSteps.HasValue && steps >= maxSteps.Value)
                    return path;

                int nextY = y + (y < targetY ? 1 : -1);
                var nextPoint = new Point3D(x, nextY, z);

                if (obstacles.Contains(nextPoint) || restrictedPoints.Contains(nextPoint))
                {
                    int bypassX = x + 1;
                    if (!obstacles.Contains(new Point3D(bypassX, y, z)) && !restrictedPoints.Contains(new Point3D(bypassX, y, z)))
                    {
                        x = bypassX;
                    }
                    else
                    {
                        int bypassZ = z + 1;
                        if (!obstacles.Contains(new Point3D(x, y, bypassZ)) && !restrictedPoints.Contains(new Point3D(x, y, bypassZ)))
                        {
                            z = bypassZ;
                        }
                        else
                        {
                            throw new Exception("Маршрут невозможен из-за препятствий.");
                        }
                    }
                }
                else
                {
                    y = nextY;
                }
            }

            while (z != targetZ)
            {
                AddPoint(x, y, z);
                if (maxSteps.HasValue && steps >= maxSteps.Value)
                    return path;

                int nextZ = z + (z < targetZ ? 1 : -1);
                var nextPoint = new Point3D(x, y, nextZ);

                if (obstacles.Contains(nextPoint) || restrictedPoints.Contains(nextPoint))
                {
                    int bypassX = x + 1;
                    if (!obstacles.Contains(new Point3D(bypassX, y, z)) && !restrictedPoints.Contains(new Point3D(bypassX, y, z)))
                    {
                        x = bypassX;
                    }
                    else
                    {
                        int bypassY = y + 1;
                        if (!obstacles.Contains(new Point3D(x, bypassY, z)) && !restrictedPoints.Contains(new Point3D(x, bypassY, z)))
                        {
                            y = bypassY;
                        }
                        else
                        {
                            throw new Exception("Маршрут невозможен из-за препятствий.");
                        }
                    }
                }
                else
                {
                    z = nextZ;
                }
            }

            AddPoint(x, y, z);
            return path;
        }
    }

    public class Point3D : IEquatable<Point3D>
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        public Point3D(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public int CompareTo(Point3D other)
        {
            if (other == null) return 1;

            int compareX = X.CompareTo(other.X);
            if (compareX != 0) return compareX;

            int compareY = Y.CompareTo(other.Y);
            if (compareY != 0) return compareY;

            return Z.CompareTo(other.Z);
        }


        public override bool Equals(object obj) => obj is Point3D other && Equals(other);
        public bool Equals(Point3D other) => X == other.X && Y == other.Y && Z == other.Z;
        public override int GetHashCode() => (X, Y, Z).GetHashCode();
    }

}

//if (nearbyEnemy != null)
//{
//    int xStep = 0;
//    int yStep = 0;
//    int zStep = 0;
//    switch (nextStep)
//    {
//        case var next when next.X == nearbyEnemy.X:
//            {
//                xStep = Math.Abs(nextStep.X - nearbyEnemy.X);
//                if (xStep.Equals(1))
//                {


//                    snakeBase.direction[0] = nextStep.X - headPoint.X;
//                    snakeBase.direction[1] = nextStep.Y - headPoint.Y;
//                    snakeBase.direction[2] = nextStep.Z - headPoint.Z;
//                }
//                break;
//            }
//        case var next when next.Y == nearbyEnemy.Y:
//            {
//                yStep = Math.Abs(nextStep.Y - nearbyEnemy.Y);
//                if (yStep.Equals(1))
//                {
//                    snakeBase.direction[0] = nextStep.X - headPoint.X;
//                    snakeBase.direction[1] = nextStep.Y - headPoint.Y;
//                    snakeBase.direction[2] = nextStep.Z - headPoint.Z;
//                }

//                break;
//            }
//        case var next when next.Z == nearbyEnemy.Z:
//            {
//                zStep = Math.Abs(nextStep.Z - nearbyEnemy.Z);
//                if (zStep.Equals(1))
//                {
//                    snakeBase.direction[0] = nextStep.X - headPoint.X;
//                    snakeBase.direction[1] = nextStep.Y - headPoint.Y;
//                    snakeBase.direction[2] = nextStep.Z - headPoint.Z;
//                }
//                break;
//            }
//    }
//}




//if (!isCenter)
//{
//    var aroundFood = _normalFood.Where(
//        w => Math.Abs(w.Item1.X - headPoint.X) <= fieldError
//        && Math.Abs(w.Item1.Y - headPoint.Y) <= fieldError
//        && Math.Abs(w.Item1.Z - headPoint.Z) <= fieldError)
//        .Select(s => s.Item1)                 
//        .ToHashSet();

//    var shortedPath = CalculateShortestPath(headPoint, pointCenter, maxSteps: 10, collectibles: aroundFood, 
//        obstacles: aroundEnemy.ToHashSet(), restrictedPoints: isOneElemSnake ?  null : geometrySnake);
//    var intersectList = shortedPath.Intersect(_gameState.fences.Select(s => new Point3D(s[0], s[1], s[2])));


//    while (intersectList.Any())
//    {
//        shortedPath = CalculateShortestPath(headPoint, pointCenter, obstacles: intersectList.Union(aroundEnemy).ToHashSet(), 
//            maxSteps: 10, collectibles: aroundFood, restrictedPoints: isOneElemSnake ? null : geometrySnake);
//        intersectList = shortedPath.Intersect(_gameState.fences.Select(s => new Point3D(s[0], s[1], s[2])));
//    }

//    var nextStep = shortedPath.Skip(1).First();

//    snakeBase.direction[0] = nextStep.X - headPoint.X;
//    snakeBase.direction[1] = nextStep.Y - headPoint.Y;
//    snakeBase.direction[2] = nextStep.Z - headPoint.Z;
//}
//else
//{

//var maxPoint = aroundFood.MaxBy(m => m.points);

//var nearbyFood = aroundFood.MinBy(m => (Math.Abs(m.Item1.X - headPoint.X) + Math.Abs(m.Item1.Y - headPoint.Y) + Math.Abs(m.Item1.Y - headPoint.Y)));