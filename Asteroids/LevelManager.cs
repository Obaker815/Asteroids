using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;

namespace Asteroids
{
    internal class LevelManager
    {
        private readonly Dictionary<int, (int big, int med, int sma)> rounds = new()
        {
            {0, (0, 0, 5) },
            {1, (0, 1, 5) },
            {2, (0, 2, 6) },
            {3, (0, 2, 6) },
            {4, (0, 2, 6) },
            {5, (0, 2, 6) },
            {6, (0, 2, 6) },
            {7, (0, 2, 6) },
            {8, (0, 2, 6) },
            {9, (0, 2, 6) },
            {10, (0, 0, 5) },
            {11, (0, 0, 5) },
            {12, (0, 0, 5) },
            {13, (0, 0, 5) },
            {14, (0, 0, 5) },
            {15, (0, 0, 5) },
            {16, (0, 0, 5) },
            {17, (0, 0, 5) },
            {18, (0, 0, 5) },
            {19, (0, 0, 5) },
        };
        private Rectangle screen;
        private long score;
        private int round;
        public long Score => score;

        public LevelManager()
        {
            score = 0;
            round = 1;
        }

        public void NewRound(Rectangle screen)
        {
            this.screen = screen;

            int numAsteroids = 4 + round;
            (int big, int medium, int small) = rounds[numAsteroids];

            for (int i = 0; i < big; i++)
                _ = Asteroid.NewAsteroid(screen, 3);

            for (int i = 0; i < medium; i++)
                _ = Asteroid.NewAsteroid(screen, 2);

            for (int i = 0; i < small; i++)
                _ = Asteroid.NewAsteroid(screen, 1);

            if (round >= 5)
            {
                Task.Run(SaucerClock);
            }
        }

        bool running = false;
        public void SaucerClock()
        {
            if (running) return;
            running = true;

            float lastSaucer = 0;
            Stopwatch saucerClock = Stopwatch.StartNew();
            while (running)
            {
                while (saucerClock.Elapsed.TotalSeconds < lastSaucer + 25)
                {
                    Thread.Sleep(5000);
                }

                lastSaucer = (float)saucerClock.Elapsed.TotalSeconds;

                if (Saucer.Saucers.Count !> 0)
                {
                    _ = new Saucer((round >= 10) ? true : false, new(0, screen.Height / 2));
                }
            }
        }
    }
}
