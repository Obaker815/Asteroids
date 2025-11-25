using System.Numerics;

namespace Asteroids
{
    internal class LevelManager
    {
        public static readonly LevelManager Instance = new();
        private const int lifeInterval = 10000;
        private float currentSaucerTime = 0f;

        private Rectangle screen;
        private long score;
        private int round;
        public long Score => score;

        public LevelManager()
        {
            score = 0;
            round = 0;
        }

        public void AddScore(int score) 
        {
            long nextLife = lifeInterval % score * lifeInterval;
            this.score += score;
            if (this.score > nextLife && this.score - score > lifeInterval)
            {
                Ship.Ships[0].lives++;
            }
        }

        public Task NewRound(Rectangle screen)
        {
            round++;
            this.screen = screen;
            currentSaucerTime = 0;

            int num = GetAsteroidNums(round);

            for (int i = 0; i < num; i++)
                _ = Asteroid.NewAsteroid(screen, 3);

            return Task.CompletedTask;
        }

        private static int GetAsteroidNums(int round)
        {
            int[] bigAsteroidsByRound =
            [
                4, 6, 8, 10, 11
            ];

            int num = round <= 5 ? bigAsteroidsByRound[round - 1] : 11;

            return num;
        }

        private static float GetSaucerInterval(int asteroidCount)
        {
            if (asteroidCount >= 9) return 18f;
            if (asteroidCount >= 5) return 12f;
            return 7f;
        }

        public void SaucerUpdate(float dt)
        {
            int asteroidCount = Asteroid.AsteroidEntities.Count;

            if (asteroidCount == 0)
            {
                currentSaucerTime = 0f;
                return;
            }

            if (round < 2)
                return;

            currentSaucerTime += dt;

            float interval = GetSaucerInterval(asteroidCount);

            if (currentSaucerTime < interval)
                return;

            if (Saucer.Saucers.Count == 0)
            {
                bool smallSaucer = score >= 40000 || round >= 5;

                Vector2 spawnPos = new(0, screen.Height / 2);

                _ = new Saucer(smallSaucer, spawnPos);
            }
            else
            {
                currentSaucerTime = 0f;
            }
        }
    }
}
