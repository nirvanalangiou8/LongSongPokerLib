using System.Collections.Concurrent;
using System.Threading.Tasks;
using GenericPoker;
using GenericPoker.EightCard;

namespace LongSongPokerLibCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            if (args.Length > 0 && args[0] == "analyze")
            {
                LongSongPokerLibCore.GenericPoker.EightCard.DataAnalysis.InitEightCardHandSplitProbAna.Run();
                return;
            }
            // Initial call to Init for the main thread if needed
            global::GenericPoker.XRandom.Init();
            TestHandSplit();
            //EightCardGameTest();
        }

        static void EightCardGameTest()
        {
            int totalIterations = 50000000;
            int workerCount = 10;
            bool useParallel = true;

            Console.WriteLine($"Running {totalIterations} iterations (Parallel: {useParallel}, Workers: {workerCount})...");

            var finalStats = new ConcurrentDictionary<string, long>();
            int completedIterations = 0;
            int reportThreshold = totalIterations / 10;
            int nextReport = reportThreshold;
            object syncLock = new object();

            void UpdateStats(Dictionary<string, int> workerStats)
            {
                foreach (var entry in workerStats)
                {
                    finalStats.AddOrUpdate(entry.Key, entry.Value, (key, old) => old + entry.Value);
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
                        
                        // Output accumulated data to screen
                        Console.WriteLine("Current Accumulated Stats:");
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
                    // Each thread initializes its own XRandom instance via the ThreadStatic property
                    // if it hasn't been initialized yet.
                    
                    var factory = new EightCardPlayerFactory();
                    var gameManager = new ConsoleGameManager<EightCardPokerCard, EightCardConsolePlayer>(factory);
                    
                    int iterationsToRun = (i == workerCount - 1) 
                        ? totalIterations - (iterationsPerWorker * (workerCount - 1)) 
                        : iterationsPerWorker;

                    for (int j = 0; j < iterationsToRun; j++)
                    {
                        gameManager.CollectPlayersCardsAndShuffle();
                        gameManager.DealCardsToPlayers();
                        gameManager.ProcessPlayersHands();

                        if ((j + 1) % 10000 == 0) // Progress reporting from workers
                        {
                            int currentCompleted = System.Threading.Interlocked.Add(ref completedIterations, 10000);
                            PrintProgress(currentCompleted);
                        }
                    }
                    
                    // Add remaining iterations that were not reported in chunks of 10000
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
                var factory = new EightCardPlayerFactory();
                var gameManager = new ConsoleGameManager<EightCardPokerCard, EightCardConsolePlayer>(factory);

                for (int i = 0; i < totalIterations; i++)
                {
                    gameManager.CollectPlayersCardsAndShuffle();
                    gameManager.DealCardsToPlayers();
                    gameManager.ProcessPlayersHands();

                    if ((i + 1) % 10000 == 0)
                    {
                        PrintProgress(i + 1);
                    }
                }
                UpdateStats(gameManager.statDict);
            }

            Console.WriteLine("EightCard Game Test completed.");
            
            // Sort by frequency descending
            var sortedStats = finalStats.OrderByDescending(x => x.Value).ToList();
            long totalHands = sortedStats.Sum(x => x.Value);
            
            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string targetPath = System.IO.Path.Combine(projectDirectory, "..", "..", "..", "stats_result.csv");
            
            using (var writer = new System.IO.StreamWriter(targetPath))
            {
                writer.WriteLine($"# Total Iterations: {totalIterations}");
                writer.WriteLine("Hand Type,Count,Probability");

                Console.WriteLine("Final Hand Type Distribution:");
                foreach (var stat in sortedStats)
                {
                    double probability = (double)stat.Value / totalHands;
                    string csvLine = $"{stat.Key},{stat.Value},{probability:F6}";
                    string displayLine = $"{stat.Key}: {stat.Value} ({probability:P4})";
                    
                    Console.WriteLine(displayLine);
                    writer.WriteLine(csvLine);
                }
            }
            
            Console.WriteLine($"Results saved to stats_result.csv");
        }

        static void TestHandSplit()
        {
            Console.WriteLine("Hello World!");
            
            //var inputCardStr = "J♣️,J🔶,3♣️,5♣️,6♣️,A♣️,A❤️,A♠️";
            var inputCardStr = "J♣️,J🔶,3♣️,6♣️,6❤️,A♣️,A❤️,A♠️";
            var pokerHand = PokerHandCalculator.CreateInstance(inputCardStr);
            pokerHand.MinFlushStraightCards = 3;
            var resHand = pokerHand.Test8CardsTwoHandsDeploy();

            // Display front hand result
            Console.WriteLine("\n=== Front Hand ===");
            Console.WriteLine($"Rank: {resHand.FrontHand.BattleHandRank}");
            Console.Write("Cards: ");
            Console.WriteLine(resHand.FrontHand.GetHandString());

            // Display back hand result
            Console.WriteLine("\n=== Back Hand ===");
            Console.WriteLine($"Rank: {resHand.BackHand.BattleHandRank}");
            Console.Write("Cards: ");
            Console.WriteLine(resHand.BackHand.GetHandString());

            Console.WriteLine("\nSuccessfully created poker hand and deployed.");
        }
    }
}
