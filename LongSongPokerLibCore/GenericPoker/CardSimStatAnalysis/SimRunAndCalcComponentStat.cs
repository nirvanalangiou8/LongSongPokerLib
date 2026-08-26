using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenericPoker;

namespace GenericPoker.CardSimStatAnalysis
{
    public class SimCardGameManager : ConsoleGameManager<SimPokerCard, SimConsolePlayer, SimPokerHandStructure>
    {
        public SimCardGameManager(EightCard.IPlayerFactory<SimConsolePlayer> factory, int cardsPerHand = 8) : base(factory, cardsPerHand) { }

        public override void ProcessPlayersHands()
        {
            foreach (var player in _players)
            {
                var ret = player.ProcessSimHands();
                
                if (ret.Count == 0)
                {
                    UpdateStat("Nothing");
                }
                
                foreach (var combo in ret)
                {
                    UpdateStat(combo.FinalCompsStr);
                }
            }
        }

        private void UpdateStat(string key)
        {
            if (statDict.ContainsKey(key))
            {
                statDict[key] += 1;
            }
            else
            {
                statDict[key] = 1;
            }
        }
    }

    public static class SimRunAndCalcComponentStat
    {
        public static void SimCardRunStat(int totalIterations = 10000, int cardsPerHand = 8, bool useParallel = false)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int workerCount = 10;
            //bool useParallel = false;

            Console.WriteLine($"Running {totalIterations} iterations for {cardsPerHand} cards (Parallel: {useParallel}, Workers: {workerCount})...");

            var finalStats = new ConcurrentDictionary<string, long>();
            int completedIterations = 0;
            int reportThreshold = totalIterations / 1000;
            int nextReport = reportThreshold;
            object syncLock = new object();

            void UpdateStats(Dictionary<string, int> workerStats)
            {
                foreach (var entry in workerStats)
                {
                    finalStats.AddOrUpdate(entry.Key, entry.Value, (key, old) => old + (long)entry.Value);
                }
            }

            void PrintProgress(int current)
            {
                lock (syncLock)
                {
                    if (current >= nextReport || current >= totalIterations)
                    {
                        double percent = (double)current / totalIterations * 100;
                        Console.WriteLine($"Progress: {percent:F0}% ({current}/{totalIterations})");
                        
                        var currentSorted = finalStats.OrderByDescending(x => x.Value).Take(10).ToList();
                        foreach (var stat in currentSorted)
                        {
                            Console.WriteLine($"  {stat.Key}: {stat.Value}");
                        }
                        
                        while (nextReport <= current)
                        {
                            nextReport += reportThreshold;
                        }
                    }
                }
            }

            if (useParallel)
            {
                int iterationsPerWorker = totalIterations / workerCount;
                Parallel.For(0, workerCount, i =>
                {
                    var factory = new SimCardPlayerFactory();
                    var gameManager = new SimCardGameManager(factory, cardsPerHand);
                    
                    int iterationsToRun = (i == workerCount - 1) 
                        ? totalIterations - (iterationsPerWorker * (workerCount - 1)) 
                        : iterationsPerWorker;

                    for (int j = 0; j < iterationsToRun; j++)
                    {
                        gameManager.CollectPlayersCardsAndShuffle();
                        gameManager.DealCardsToPlayers(cardsPerHand);
                        gameManager.ProcessPlayersHands();

                        if ((j + 1) % 10000 == 0)
                        {
                            int currentCompleted = System.Threading.Interlocked.Add(ref completedIterations, 10000);
                            PrintProgress(currentCompleted);
                        }
                    }
                    
                    int remaining = iterationsToRun % 10000;
                    if (remaining > 0)
                    {
                        int currentCompleted = System.Threading.Interlocked.Add(ref completedIterations, remaining);
                        PrintProgress(currentCompleted);
                    }

                    UpdateStats(gameManager.statDict);
                });
            }
            else
            {
                var factory = new SimCardPlayerFactory();
                var gameManager = new SimCardGameManager(factory, cardsPerHand);

                for (int i = 0; i < totalIterations; i++)
                {
                    gameManager.CollectPlayersCardsAndShuffle();
                    gameManager.DealCardsToPlayers(cardsPerHand);
                    gameManager.ProcessPlayersHands();
                    
                    // help revise if 1 % of totalItertions completed, please progress percentage.  
                    if ((i + 1) % 10000 == 0)
                    {
                        PrintProgress(i + 1);
                    }
                }
                UpdateStats(gameManager.statDict);
            }

            sw.Stop();
            Console.WriteLine($"{cardsPerHand}Card Game Test completed in {sw.ElapsedMilliseconds} ms.");
            
            var sortedStats = finalStats.OrderByDescending(x => x.Value).ToList();
            long totalHands = sortedStats.Sum(x => x.Value);
            
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string baseDir = System.IO.Path.Combine(projectDirectory, "..", "..", "..");
            string dataDir;
            if (System.IO.Directory.Exists(System.IO.Path.Combine(baseDir, "GenericPoker", "CardSimStatAnalysis", "Data")))
            {
                dataDir = System.IO.Path.Combine(baseDir, "GenericPoker", "CardSimStatAnalysis", "Data");
            }
            else if (System.IO.Directory.Exists(System.IO.Path.Combine(baseDir, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", "Data")))
            {
                dataDir = System.IO.Path.Combine(baseDir, "LongSongPokerLibCore", "GenericPoker", "CardSimStatAnalysis", "Data");
            }
            else
            {
                dataDir = System.IO.Path.Combine(baseDir, "GenericPoker", "CardSimStatAnalysis", "Data");
                System.IO.Directory.CreateDirectory(dataDir);
            }
            string targetPath = System.IO.Path.Combine(dataDir, $"stats_result_{cardsPerHand}cards.csv");
            
            using (var writer = new System.IO.StreamWriter(targetPath))
            {
                writer.WriteLine($"# Total Iterations: {totalIterations}");
                writer.WriteLine($"# Cards per Hand: {cardsPerHand}");
                writer.WriteLine("Hand Type,Count,Probability");

                foreach (var stat in sortedStats)
                {
                    double probability = (double)stat.Value / totalHands;
                    string csvLine = $"{stat.Key},{stat.Value},{probability:F6}";
                    writer.WriteLine(csvLine);
                }
            }
            
            Console.WriteLine($"Results saved to {targetPath}");
        }
    }
}
