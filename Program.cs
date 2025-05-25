using ConsoleAppFramework;

namespace MilkyCrafter;

class LevelStat(double avgTries, double avgEnhances, double avgProtects, double avgExp, double avgSuperSuccess)
{
    /// <summary>
    /// Сколько попыток было сделано на 1 успешную попытку. (При сбросе в 0 нужно добавить авергу прошлой статы)
    /// </summary>
    public double AvgTries { get; } = avgTries;

    /// <summary>
    /// Сколько раз в среднем нужно было прожать енханс на 1 успешную попытку
    /// </summary>
    public double AvgEnhances { get; } = avgEnhances;
    /// <summary>
    /// Сколько в среднем уходит проектов на 1 успешную попытку
    /// </summary>
    public double AvgProtects { get; } = avgProtects;
    public double AvgExp { get; } = avgExp;
    
    public double AvgSuperSuccess { get; } = avgSuperSuccess;
}

class Program
{
    public const long Enhances = 80000000;

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
    /// <param name="drinkBlessTea">Чаечек пиём?</param>
    /// <param name="_startingLevel">-sl, Хуйня</param>
    /// <param name="enhances">Сколько итерация в симуляции</param>
    public static void Do([Argument] int targetLevel, double addedChance = 0.0, int protectSinceLevel = 5,
        bool drinkBlessTea = true, int _startingLevel = 0, double enhances = Program.Enhances)
    {
        for (int i = 0; i < Chances.ResultChanceArray.Length; i++)
        {
            Chances.ResultChanceArray[i] =
                (i < Chances.LevelToChance.Length ? Chances.LevelToChance[i] : Chances.ChanceOver) + addedChance;
        }

        List<LevelStat> stats = new();

        Random random = new();

        DateTimeOffset start = DateTimeOffset.UtcNow;

        for (int nowTargetLevel = 1; nowTargetLevel <= targetLevel; nowTargetLevel++)
        {
            // currentLevel = startingLevel;
            int startingLevel = nowTargetLevel - 1;
            
            long superSuccess = 0;
            long successes = 0;
            long totalEnhances = 0;
            long totalProtects = 0;
            long totalTries = 0;
            long totalSupers = 0;
            double totalExp = 0;
            double fails = 0;

            int currentLevel = startingLevel;
            long currentEnhances = 0;
            long currentProtects = 0;
            long currentTries = 1;
            double currentExp = 0;
            for (long i = 0; i < enhances; i++)
            {
                double chance = Chances.ResultChanceArray[currentLevel];

                currentEnhances++;

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

                    if (currentLevel == nowTargetLevel)
                    {
                        successes++;
                        totalEnhances += currentEnhances;
                        totalProtects += currentProtects;
                        totalTries += currentTries;
                        totalExp += currentExp;

                        currentLevel = startingLevel;
                        currentEnhances = 0;
                        currentProtects = 0;
                        currentTries = 1;
                        currentExp = 0;
                    }
                    else if (currentLevel > nowTargetLevel)
                    {
                        superSuccess++;
                        successes++;
                        totalEnhances += currentEnhances;
                        totalProtects += currentProtects;
                        totalTries += currentTries;
                        totalExp += currentExp;

                        currentLevel = startingLevel;
                        currentEnhances = 0;
                        currentProtects = 0;
                        currentTries = 1;
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
                    currentTries++;
                    // currentLevel = _startingLevel;
                    // if (_startingLevel > 0)
                    // {
                    //     fails++;
                    // }
                }
            }
            
            double avgEnhances = (double)totalEnhances / successes;
            double avgExp = totalExp / successes;
            double avgProtects = 0;
            double avgTries = (double)totalTries / successes;
            double avgSuperSuccesses = (double)superSuccess / successes;

            if (totalProtects > 0)
            {
                avgProtects = (double)(totalProtects) / successes;
                Console.WriteLine($"Протектов {avgProtects}");
            }
            
            stats.Add(new LevelStat(avgTries, avgEnhances, avgProtects, avgExp, avgSuperSuccesses));
        }

        TimeSpan timePassed = DateTimeOffset.UtcNow - start;

        Console.WriteLine($"Прошло {timePassed.TotalSeconds:F1} секунд");
        // Console.WriteLine($"{(double)enhances / successes}");
        Console.WriteLine();
        
        Console.WriteLine("Enhances/Protects/Exp");

        List<LevelStat> totalStats = new();
        for (int level = 0; level < stats.Count; level++)
        {
            LevelStat stat = stats[level];
            
            double currentAvgEnhances = stat.AvgEnhances;
            double currentAvgProtects = stat.AvgProtects;
            double currentAvgExp = stat.AvgExp;

            if (level > 1)
            {
                LevelStat prevStat = totalStats[level - 1];
                LevelStat prevPrevStat = totalStats[level - 2];
                
                // Количество AvgEnhances включает в себя енхансы и давшие именно этот тир,
                // а также енхансы, где прокнул 1% и получился тир на 1 выше нужного
                // Мы плюсуем количество траев, необходимые для получения текущего уровня
                // То есть мы берём те прошлые уровни, где не прокнул 1%
                // И добавляем позапрошлые уровни, где прокнул 1%
                
                currentAvgEnhances += stat.AvgTries * (prevStat.AvgEnhances * (1 - prevPrevStat.AvgSuperSuccess));
                currentAvgEnhances += stat.AvgTries * (prevPrevStat.AvgEnhances * prevPrevStat.AvgSuperSuccess);

                currentAvgProtects += stat.AvgTries * (prevStat.AvgProtects * (1 - prevPrevStat.AvgSuperSuccess));
                currentAvgProtects += stat.AvgTries * (prevPrevStat.AvgProtects * prevPrevStat.AvgSuperSuccess);
                
                currentAvgExp += stat.AvgTries * (prevStat.AvgExp * (1 - prevPrevStat.AvgSuperSuccess));
                currentAvgExp += stat.AvgTries * (prevPrevStat.AvgExp * prevPrevStat.AvgSuperSuccess);
            }
            else if (level > 0)
            {
                LevelStat prevStat = totalStats[level - 1];
                
                currentAvgEnhances += stat.AvgTries * prevStat.AvgEnhances;
                currentAvgProtects += stat.AvgTries * prevStat.AvgProtects;
                currentAvgExp += stat.AvgTries * prevStat.AvgExp;
            }
            
            Console.WriteLine($"+{level+1}");
            Console.WriteLine($"{currentAvgEnhances} / {currentAvgProtects} / {currentAvgExp}");
            
            totalStats.Add(new LevelStat(-1, currentAvgEnhances, currentAvgProtects, currentAvgExp, stat.AvgSuperSuccess));
        }

        // if (successes > 0)
        // {
        //     double avgEnhances = (double)(totalEnhances) / successes;
        //     double avgExp = (totalExp) / successes;
        //
        //     Console.WriteLine($"Попыток {avgEnhances}");
        //     if (totalProtects > 0)
        //     {
        //         double avgProtects = (double)(totalProtects) / successes;
        //         Console.WriteLine($"Протектов {avgProtects}");
        //     }
        //
        //     Console.WriteLine($"Опыта {avgExp:F2}");
        //
        //     if (fails > 0)
        //     {
        //         double failsPerSuccess = (fails / successes);
        //         Console.WriteLine($"Фейлов на успех {failsPerSuccess}");
        //     }
        //
        //     if (totalSupers > 0)
        //     {
        //         double avgTotalSupers = (double)(totalSupers) / successes;
        //         Console.WriteLine($"Повезлоу {avgTotalSupers}");
        //     }
        //
        //     if (superSuccess > 0)
        //     {
        //         double avgSuperSuccess = (double)(superSuccess) / successes;
        //         Console.WriteLine($"СУПЕР ПОВЕЗЛОУ {avgSuperSuccess}");
        //     }
        // }
        // else
        // {
        //     Console.WriteLine("Проебали");
        // }
    }
}