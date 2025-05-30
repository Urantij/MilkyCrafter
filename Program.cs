using ConsoleAppFramework;

namespace MilkyCrafter;

class LevelStat(double avgTries, double avgLevelEnhances, double avgProtects, double avgExp, double avgSuperSuccess)
{
    /// <summary>
    /// Сколько попыток было сделано на 1 успешную попытку. (При сбросе в 0 нужно добавить авергу прошлой статы)
    /// </summary>
    public double AvgTries { get; } = avgTries;

    /// <summary>
    /// Сколько раз в среднем нужно было прожать енханс на 1 успешную попытку
    /// </summary>
    public double AvgLevelEnhances { get; } = avgLevelEnhances;
    
    public double AvgTotalEnhances { get; set; }
    public double AvgTotalPureEnhances { get; set; }
    
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
    /// <param name="resultTargetLevel">До какого + точим</param>
    /// <param name="addedChance">-ac, Отклонение от 50% шанса при точке на +1</param>
    /// <param name="protectSinceLevel">-p, С какого + юзать протек при точке</param>
    /// <param name="drinkBlessTea">Чаечек пиём?</param>
    /// <param name="_startingLevel">-sl, Хуйня</param>
    /// <param name="enhances">Сколько итерация в симуляции</param>
    public static void Do([Argument] int resultTargetLevel, double addedChance = 0.0, int protectSinceLevel = 5,
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

        for (int nowTargetLevel = 1; nowTargetLevel <= resultTargetLevel; nowTargetLevel++)
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
            }

            LevelStat stat = new(avgTries, avgEnhances, avgProtects, avgExp, avgSuperSuccesses);

            if (nowTargetLevel == 1)
            {
                stat.AvgTotalEnhances = avgEnhances;
                stat.AvgTotalPureEnhances = (1 - avgSuperSuccesses) * avgEnhances;
            }
            else if (nowTargetLevel == 2)
            {
                LevelStat prevStat = stats[nowTargetLevel - 2];
                
                // // при каче 2 есть 2 варианта развития событий
                // // +0 ... +2
                // // +0 ... +1 ... +2
                // // Значит нужно взять +0, взять его 1% енхансы
                // // Затем взять 99% енхансы, апнуть до +1, а затем апнуть до +2
                //
                // double procEnhances = prevStat.AvgSuperSuccess * prevStat.AvgLevelEnhances;
                // double nonProcSubEnhances = (1 - prevStat.AvgSuperSuccess) * prevStat.AvgLevelEnhances;
                //
                // double a = stat.AvgLevelEnhances + stat.AvgTries * nonProcSubEnhances;
                //
                // stat.AvgTotalPureEnhances = procEnhances + a;
                
                double procEnhances = prevStat.AvgSuperSuccess * prevStat.AvgLevelEnhances;
                double nonProcEnhances = (1 - prevStat.AvgSuperSuccess) * prevStat.AvgLevelEnhances;

                double tryEnhanceCost = avgTries * (nonProcEnhances - procEnhances);
                
                stat.AvgTotalEnhances = tryEnhanceCost + avgEnhances;
            }
            else
            {
                LevelStat prevStat = stats[nowTargetLevel - 2];
                LevelStat prevestStat = stats[nowTargetLevel - 3];
                
                double a = prevestStat.AvgTotalEnhances * prevStat.AvgSuperSuccess;
                
                double procEnhances = prevStat.AvgSuperSuccess * prevStat.AvgTotalEnhances;
                double nonProcEnhances = (1 - prevStat.AvgSuperSuccess) * prevStat.AvgTotalEnhances;
                
                double tryEnhanceCost = avgTries * (nonProcEnhances + a - procEnhances);
                
                stat.AvgTotalEnhances = tryEnhanceCost + avgEnhances;
            }
            
            stats.Add(stat);
            Console.WriteLine($"+{nowTargetLevel} {stat.AvgTotalEnhances}");
        }

        TimeSpan timePassed = DateTimeOffset.UtcNow - start;

        Console.WriteLine($"Прошло {timePassed.TotalSeconds:F1} секунд");
        // Console.WriteLine($"{(double)enhances / successes}");
        Console.WriteLine();
        
        return;

        Console.WriteLine("Enhances/Protects/Exp");

        List<LevelStat> totalStats = new();
        for (int displayLevel = 1; displayLevel <= resultTargetLevel; displayLevel++)
        {
            LevelStat itemStat = stats[displayLevel - 1];

            double currentAvgEnhances = itemStat.AvgLevelEnhances;

            if (displayLevel > 1)
            {
                LevelStat prevItemStat = totalStats[displayLevel - 2];

                double procEnhances = prevItemStat.AvgSuperSuccess * prevItemStat.AvgLevelEnhances;
                double nonProcEnhances = (1 - prevItemStat.AvgSuperSuccess) * prevItemStat.AvgLevelEnhances;

                double tryEnhanceCost = itemStat.AvgTries * (nonProcEnhances - procEnhances);
                
                currentAvgEnhances = tryEnhanceCost + itemStat.AvgLevelEnhances;
                
                // да нет блять. у меня есть среднее количество енхансов при апе с +1 на +2
                // мне нужно взять эти енхансы, и добавить енхансы, которые нужны, чтобы получить +1
                // в тотале получения +1 есть те енхансы, которые на самом деле с +0 до +2 грейд.
                // то есть мне нужно получить "чистые" +1, а грейды с +0 до +2 добавить отдельно
                // чистые +1 это тотал енхансов на +1 минус та часть 1%. то есть мне нужно умножить тотал на .99.
                // но это не рабоатет блйть

                // При апе +5 нужно учитывать траи апа с +0. 1% апа с +0 на +2, после чего 99% апа с +0 га +1 и затем 100% апа с +1 на +2

                // // Количество траев на +2 это среднее количество траев с 1 на 2, плюс компенсация
                // // И с шансом 1 % вместо средних траев с 1 на 2 юзается с 0 на 1 ?
                // // мммм. с шансом 1% юзается среднее на получение 0 (а это 0). дааа, походу оно. типа компенсация + дабл енчант
                //
                // double normalAdditionalAttempts = (itemStat.AvgEnhances + itemStat.AvgTries * prevItemStat.AvgEnhances) * (1 - prevItemStat.AvgSuperSuccess);
                // double procAdditionalAttempts = 0 * prevItemStat.AvgSuperSuccess;
                //
                // // double normalAdditionalAttempts = (itemStat.AvgEnhances + itemStat.AvgTries * prevItemStat.AvgEnhances) * (1 - prevItemStat.AvgSuperSuccess);
                // // double procAdditionalAttempts = prevItemStat.AvgEnhances * prevItemStat.AvgSuperSuccess;
                // //
            }

            Console.WriteLine($"+{displayLevel}");
            Console.WriteLine($"{currentAvgEnhances}");

            totalStats.Add(new LevelStat(itemStat.AvgTries, currentAvgEnhances, 0, 0, itemStat.AvgSuperSuccess));
        }

        return;

        for (int itemLevel = 0; itemLevel < stats.Count; itemLevel++)
        {
            LevelStat stat = stats[itemLevel];

            // itemLevel 0 == target +1
            // itemLevel 1 == target +2

            double currentAvgEnhances = stat.AvgLevelEnhances;
            double currentAvgProtects = stat.AvgProtects;
            double currentAvgExp = stat.AvgExp;

            if (itemLevel > 1)
            {
                LevelStat prevStat = totalStats[itemLevel - 1];
                LevelStat prevPrevStat = totalStats[itemLevel - 2];

                double prevPart = 1 - prevPrevStat.AvgSuperSuccess;

                currentAvgEnhances += stat.AvgTries * (prevStat.AvgLevelEnhances * prevPart +
                                                       prevPrevStat.AvgLevelEnhances * prevPrevStat.AvgSuperSuccess);
                currentAvgProtects += stat.AvgTries * (prevStat.AvgProtects * prevPart +
                                                       prevPrevStat.AvgProtects * prevPrevStat.AvgSuperSuccess);
                currentAvgExp += stat.AvgTries * (prevStat.AvgExp * prevPart +
                                                  prevPrevStat.AvgExp * prevPrevStat.AvgSuperSuccess);

                // Количество AvgEnhances включает в себя енхансы и давшие именно этот тир,
                // а также енхансы, где прокнул 1% и получился тир на 1 выше нужного
                // Мы плюсуем количество траев, необходимые для получения текущего уровня
                // То есть мы берём те прошлые уровни, где не прокнул 1%
                // И добавляем позапрошлые уровни, где прокнул 1%

                // currentAvgEnhances += stat.AvgTries * (prevStat.AvgEnhances * prevPart);
                // currentAvgEnhances += stat.AvgTries * (prevPrevStat.AvgEnhances * prevPrevStat.AvgSuperSuccess);
                //
                // currentAvgProtects += stat.AvgTries * (prevStat.AvgProtects * prevPart);
                // currentAvgProtects += stat.AvgTries * (prevPrevStat.AvgProtects * prevPrevStat.AvgSuperSuccess);
                //
                // currentAvgExp += stat.AvgTries * (prevStat.AvgExp * prevPart);
                // currentAvgExp += stat.AvgTries * (prevPrevStat.AvgExp * prevPrevStat.AvgSuperSuccess);
            }
            else if (itemLevel == 1)
            {
                LevelStat prevStat = totalStats[itemLevel - 1];

                currentAvgEnhances += stat.AvgTries * prevStat.AvgLevelEnhances;
                currentAvgProtects += stat.AvgTries * prevStat.AvgProtects;
                currentAvgExp += stat.AvgTries * prevStat.AvgExp;
            }

            Console.WriteLine($"+{itemLevel + 1}");
            // Console.WriteLine($"{displayCurrentAvgEnhances} / {currentAvgProtects} / {currentAvgExp}");
            Console.WriteLine($"{currentAvgEnhances} / {currentAvgEnhances}");

            totalStats.Add(new LevelStat(-1, currentAvgEnhances, currentAvgProtects, currentAvgExp,
                stat.AvgSuperSuccess));
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