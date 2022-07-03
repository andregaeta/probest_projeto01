using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Probest
{
    class Projeto01
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Questao 1:");
            //1
            var polySim = new PolygonSim(12, int.MaxValue);
            Console.WriteLine(polySim.GetAverage(100000));

            //1d
            var pmf = polySim.GetPMF(10001, 10001);
            foreach (var pmfPair in pmf)
            {
                Console.WriteLine($"[{pmfPair.Key}] = {pmfPair.Value}");
            }

            Console.WriteLine("\n");
            Console.WriteLine("Questao 2:");

            //2
            for (int i = 0; i < 9; i++)
            {
                var pos = Vector2Int.FromIndex(i, 3);
                var boardSim = new BoardSim(pos);
                boardSim.GetAverage(100000, out var steps, out var centerHits, out var tlTrapHits, out var brTrapHits);
                Console.WriteLine($"Starting Position: [{pos.x}, {pos.y}]");
                Console.WriteLine($"Steps = {steps}, Center Hits = {centerHits}, Top Left = {tlTrapHits}, Bottom Right = {brTrapHits}");
                Console.WriteLine("\n");
            }

            Console.ReadKey();
        }
    }

    class PolygonSim
    {
        private int _vertexCount;
        private int _maxSteps;
        private Random _random;

        public PolygonSim(int vertexCount, int maxSteps)
        {
            _vertexCount = vertexCount;
            _maxSteps = maxSteps;
            _random = new Random();
        }

        public Dictionary<int, float> GetPMF(int t, int iterations)
        {
            Dictionary<int, float> vertexHits = new Dictionary<int, float>();
            for (int i = 0; i < _vertexCount; i++)
            {
                vertexHits.Add(i, 0);
            }

            for (int i = 0; i < iterations; i++)
            {
                int vertexIndex = 0;

                for (int j = 0; j < t; j++)
                {
                    int movement = _random.NextDouble() < 0.5f ? -1 : 1;

                    vertexIndex = (vertexIndex + movement + _vertexCount) % _vertexCount;
                }

                vertexHits[vertexIndex] += 1;
            }

            for (int i = 0; i < _vertexCount; i++)
            {
                vertexHits[i] /= iterations;
            }

            return vertexHits;
        }

        public float GetAverage(int iterations)
        {
            int totalSteps = 0;

            for (int i = 0; i < iterations; i++)
                totalSteps += Run();

            return totalSteps / (float) iterations;
        }

        public int Run()
        {
            int vertexIndex = 0;
            
            HashSet<int> remainingIndexes = new HashSet<int>();
            for (int i = 0; i < _vertexCount; i++)
                remainingIndexes.Add(i);

            // inicial já conta como visitado? (considerei que sim)
            remainingIndexes.Remove(vertexIndex);

            for (int i = 0; i < _maxSteps; i++)
            {
                int movement = _random.NextDouble() < 0.5f ? -1 : 1;
                vertexIndex = (vertexIndex + movement + _vertexCount) % _vertexCount;
                remainingIndexes.Remove(vertexIndex);

                if(remainingIndexes.Count <= 0)
                    return i + 1;
            }

            return -1;
        }
    }

    class BoardSim
    {
        private Vector2Int _initialPos;
        private TransitionMatrix _matrix;
       
        public BoardSim(int x, int y) : this(new Vector2Int(x, y)) { }

        public BoardSim(Vector2Int initialPos)
        {
            _initialPos = initialPos;

            _matrix = new TransitionMatrix(new float[,]
            {
                { 1,    1/3f,   0,     1/3f,   0,      0,       0,     0,       0 },
                { 0,    0,      1/2f,  0,      1/4f,   0,       0,     0,       0 },
                { 0,    1/3f,   0,     0,      0,      1/3f,    0,     0,       0 },
                { 0,    0,      0,     0,      1/4f,   0,       1/2f,  0,       0 },
                { 0,    1/3f,   0,     1/3f,   0,      1/3f,    0,     1/3f,    0 },
                { 0,    0,      1/2f,  0,      1/4f,   0,       0,     0,       0 },
                { 0,    0,      0,     1/3f,   0,      0,       0,     1/3f,    0 },
                { 0,    0,      0,     0,      1/4f,   0,       1/2f,  0,       0 },
                { 0,    0,      0,     0,      0,      1/3f,    0,     1/3f,    1 }
            });
        }

        public void GetAverage(int iterations, out float steps, out float centerHits, out float tlTrapHits, out float brTrapHits)
        {
            steps = 0;
            centerHits = 0;
            tlTrapHits = 0;
            brTrapHits = 0;

            for (int i = 0; i < iterations; i++)
            {
                Run(out var isteps, out var icenterHits, out var itlTrapHits, out var ibrTrapHits);

                steps      += isteps;
                centerHits += icenterHits;
                tlTrapHits += itlTrapHits;
                brTrapHits += ibrTrapHits;
            }

            steps      /= iterations;
            centerHits /= iterations;
            tlTrapHits /= iterations;
            brTrapHits /= iterations;
        }

        public void Run(out int steps, out int centerHits, out int tlTrapHits, out int brTrapHits)
        {
            Vector2Int currentPos = new Vector2Int(_initialPos.x, _initialPos.y);
            steps = 0;
            centerHits = 0;
            tlTrapHits = 0;
            brTrapHits = 0;

            while (true)
            {
                //Console.WriteLine($"[{currentPos.x}, {currentPos.y}]");

                if (currentPos.Equals(0, 0))
                {
                    tlTrapHits = 1;
                    break;
                }
                else if (currentPos.Equals(2, 2))
                {
                    brTrapHits = 1;
                    break;
                }

                currentPos = _matrix.RollNextPos(currentPos, 3);
                steps++;

                // começar no centro conta como visita à casa central? (considerei que não)
                if (currentPos.Equals(1, 1))
                {
                    centerHits++;
                }
            }
        }
    }

    enum TileType
    {
        Trap,
        Corner,
        Side,
        Center
    }

    class Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int ToIndex(int dimension)
        {
            return x * dimension + y;
        }

        public static Vector2Int FromIndex(int index, int dimension)
        {
            return new Vector2Int(index / dimension, index % dimension);
        }

        public bool Equals(Vector2Int other)
        {
            return this.x == other.x && this.y == other.y;
        }

        public bool Equals(int x, int y)
        {
            return this.x == x && this.y == y;
        }
    }

    class TransitionMatrix
    {
        private float[,] _matrix;
        private Random _random;

        public TransitionMatrix(float[,] matrix)
        {
            _matrix = matrix;
            _random = new Random();
        }

        public TileType RollNextType(TileType current)
        {
            var rand = _random.NextDouble();
            int currentIndex = (int) current;

            float threshold = 0;

            for (int i = 0; i < _matrix.GetLength(1); i++)
            {
                threshold += _matrix[i, currentIndex];

                if (rand < threshold)
                    return (TileType)i;
            }

            return TileType.Center;
        }

        public Vector2Int RollNextPos(Vector2Int currentPos, int dimension)
        {
            var index = currentPos.ToIndex(dimension);
            var rand = _random.NextDouble();

            float threshold = 0;

            for (int i = 0; i < _matrix.GetLength(1); i++)
            {
                threshold += _matrix[i, index];

                if (rand < threshold)
                    return Vector2Int.FromIndex(i, dimension);
            }

            Console.WriteLine("ERRO");
            return new Vector2Int(0, 0);
        }
    }

    class BoardSimSimple
    {
        private Vector2Int _initialPos;
        private TileType[,] _board;
        private TransitionMatrix _matrix;
      
        private TileType GetTile(Vector2Int pos) => _board[pos.x, pos.y];

        public BoardSimSimple(Vector2Int initialPos)
        {
            _initialPos = initialPos;
            _board = new TileType[,]
            {
                { TileType.Trap,    TileType.Side,      TileType.Corner },
                { TileType.Side,    TileType.Center,    TileType.Side   },
                { TileType.Corner,  TileType.Side,      TileType.Trap   }
            };

            _matrix = new TransitionMatrix(new float[,]
            {
                { 1,    0,     1/3f,   0 },
                { 0,    0,     1/3f,   0 },
                { 0,    1,     0,      1 },
                { 0,    0,     1/3f,   0 }
            });
        }

        public void Run(out int steps, out int centerHits)
        {
            TileType currentTile = GetTile(_initialPos);
            steps = 0;
            centerHits = 0;

            while (currentTile != TileType.Trap)
            {
                currentTile = _matrix.RollNextType(currentTile);

                if (currentTile == TileType.Center)
                    centerHits++;

                steps++;
            }
        }
    }

}
