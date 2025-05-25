using ConsoleAppFramework;

namespace MilkyCrafter;

class Program
{
    public const long Attempts = 80000000;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        ConsoleApp.Run(args, Commands.Do);
    }
}

static class Commands
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetLevel">До какого + точим</param>
    /// <param name="addedChance">-ac, Отклонение от 50% шанса при точке на +1</param>
    /// <param name="protectSinceLevel">-p, С какого + юзать протек при точке</param>
    /// <param name="noDrinkBlessTea">Чаечек не пиём?</param>
    /// <param name="startingLevel">-sl, Хуйня</param>
    /// <param name="attempts">Сколько итерация в симуляции</param>
    public static void Do([Argument] int targetLevel, double addedChance = 0.0, int protectSinceLevel = 5,
        bool noDrinkBlessTea = false, int startingLevel = 0, double attempts = Program.Attempts)
    {
        for (int i = 0; i < Chances.ResultChanceArray.Length; i++)
        {
            Chances.ResultChanceArray[i] =
                (i < Chances.LevelToChance.Length ? Chances.LevelToChance[i] : Chances.ChanceOver) + addedChance;
        }

        bool drinkBlessTea = !noDrinkBlessTea;

        Console.WriteLine(drinkBlessTea ? "пиём" : "не пиём");

        Random random = new();

        DateTimeOffset start = DateTimeOffset.UtcNow;

        long superSuccess = 0;
        long successes = 0;
        long totalAttempts = 0;
        long totalProtects = 0;
        long totalSupers = 0;
        double totalExp = 0;

        double fails = 0;

        int currentLevel = startingLevel;
        long currentAttempts = 0;
        long currentProtects = 0;
        double currentExp = 0;
        for (long i = 0; i < attempts; i++)
        {
            double chance = Chances.ResultChanceArray[currentLevel];

            currentAttempts++;

            if (random.NextDouble() < chance)
            {
                currentLevel++;

                currentExp += currentLevel;

                if (drinkBlessTea && random.NextDouble() < 0.01)
                {
                    currentLevel++;
                    totalSupers++;

                    // не уверен, но наверное
                    currentExp += currentLevel;
                }

                if (currentLevel == targetLevel)
                {
                    successes++;
                    totalAttempts += currentAttempts;
                    totalProtects += currentProtects;
                    totalExp += currentExp;

                    currentLevel = 0;
                    currentAttempts = 0;
                    currentProtects = 0;
                    currentExp = 0;
                }
                else if (currentLevel > targetLevel)
                {
                    superSuccess++;
                    successes++;
                    totalAttempts += currentAttempts;
                    totalProtects += currentProtects;
                    totalExp += currentExp;

                    currentLevel = 0;
                    currentAttempts = 0;
                    currentProtects = 0;
                    currentExp = 0;
                }
            }
            else if (currentLevel >= protectSinceLevel)
            {
                currentExp += 1.0 / 5.0;

                currentLevel--;
                currentProtects++;
            }
            else
            {
                currentExp += 1.0 / 5.0;

                currentLevel = startingLevel;

                if (startingLevel > 0)
                {
                    fails++;
                }
            }
        }

        TimeSpan timePassed = DateTimeOffset.UtcNow - start;

        Console.WriteLine($"Прошло {timePassed.TotalSeconds:F1} секунд");
        Console.WriteLine($"{(double)attempts / successes}");
        Console.WriteLine();

        if (successes > 0)
        {
            double avgAttempts = (double)(totalAttempts) / successes;
            double avgExp = (totalExp) / successes;

            Console.WriteLine($"Попыток {avgAttempts}");
            if (totalProtects > 0)
            {
                double avgProtects = (double)(totalProtects) / successes;
                Console.WriteLine($"Протектов {avgProtects}");
            }

            Console.WriteLine($"Опыта {avgExp:F2}");

            if (fails > 0)
            {
                double failsPerSuccess = (fails / successes);
                Console.WriteLine($"Фейлов на успех {failsPerSuccess}");
            }

            if (totalSupers > 0)
            {
                double avgTotalSupers = (double)(totalSupers) / successes;
                Console.WriteLine($"Повезлоу {avgTotalSupers}");
            }

            if (superSuccess > 0)
            {
                double avgSuperSuccess = (double)(superSuccess) / successes;
                Console.WriteLine($"СУПЕР ПОВЕЗЛОУ {avgSuperSuccess}");
            }
        }
        else
        {
            Console.WriteLine("Проебали");
        }
    }
}