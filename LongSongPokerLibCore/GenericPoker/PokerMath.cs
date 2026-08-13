
using System;
using System.Collections.Generic;


namespace GenericPoker.EightCard
{
    // 1. 定義區塊的數學特性
    public enum SpaceType
    {
        Cartesian,   // 笛卡兒空間：獨立變數 (如：同花順帶頭、對子、獨立散牌)
        Combination  // 組合空間：連動變數，需用巴斯卡解碼 (如：多張散牌)
    }

    // 2. 定義數學子空間的幾何結構 (Schema)
    public struct SpaceDef
    {
        public SpaceType Type;
        
        /// <summary>
        /// 這個空間的底數大小 (例如: 11 種同花順, 13 種散牌)
        /// </summary>
        public int PoolSize; 
        
        /// <summary>
        /// 這個區塊佔用幾個獨立變數維度 (Offsets)
        /// 取代原本容易造成混淆的 NumCards
        /// </summary>
        public int Dimensions; 

        public SpaceDef(SpaceType type, int poolSize, int dimensions)
        {
            Type = type; 
            PoolSize = poolSize; 
            Dimensions = dimensions;
        }
    }

    // 3. 終極大一統內插引擎
    public static class PokerMath
    {
        // 硬編碼巴斯卡三角形對照表：[poolSize (最大14), k (最大8)]
        // 完全杜絕執行期運算，記憶體佈局極度緊湊
        private static readonly int[,] PascalTable = new int[15, 9]
        {
            /* Row 0  */ { 1, 0, 0, 0, 0, 0, 0, 0, 0 },
            /* Row 1  */ { 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            /* Row 2  */ { 1, 2, 1, 0, 0, 0, 0, 0, 0 },
            /* Row 3  */ { 1, 3, 3, 1, 0, 0, 0, 0, 0 },
            /* Row 4  */ { 1, 4, 6, 4, 1, 0, 0, 0, 0 },
            /* Row 5  */ { 1, 5, 10, 10, 5, 1, 0, 0, 0 },
            /* Row 6  */ { 1, 6, 15, 20, 15, 6, 1, 0, 0 },
            /* Row 7  */ { 1, 7, 21, 35, 35, 21, 7, 1, 0 },
            /* Row 8  */ { 1, 8, 28, 56, 70, 56, 28, 8, 1 },
            /* Row 9  */ { 1, 9, 36, 84, 126, 126, 84, 36, 9 },
            /* Row 10 */ { 1, 10, 45, 120, 210, 252, 210, 120, 45 },
            /* Row 11 */ { 1, 11, 55, 165, 330, 462, 462, 330, 165 },
            /* Row 12 */ { 1, 12, 66, 220, 495, 792, 924, 792, 495 },
            /* Row 13 */ { 1, 13, 78, 286, 715, 1287, 1716, 1716, 1287 },
            /* Row 14 */ { 1, 14, 91, 364, 1001, 2002, 3003, 3432, 3003 }
        };

        /// <summary>
        /// 混合基底 (Mixed-Radix) 大一統內插計算機：支援任何形狀的手牌結構
        /// </summary>
        /// <param name="parsedOffsets">已經轉化為 0 起始的變數偏移量陣列</param>
        /// <param name="schema">這副牌的數學空間定義 (從高位元到低位元排列)</param>
        /// <param name="probMin">勝率區間下限</param>
        /// <param name="probMax">勝率區間上限</param>
        public static double GetUnifiedWinRate(int[] parsedOffsets, SpaceDef[] schema, double probMin, double probMax)
        {
            long globalIndex = 0;
            long currentMultiplier = 1;

            // 陣列指針，用來從後面 (低位元) 提取 offset
            int offsetPointer = parsedOffsets.Length - 1;

            // 從右到左 (從最低位元開始) 計算混合基底進位
            for (int i = schema.Length - 1; i >= 0; i--)
            {
                SpaceDef space = schema[i];
                long blockIndex = 0;
                long blockTotal = 0;

                if (space.Type == SpaceType.Cartesian)
                {
                    // Cartesian 空間：單純的維度偏移提取
                    blockIndex = parsedOffsets[offsetPointer--];
                    blockTotal = space.PoolSize;
                }
                else if (space.Type == SpaceType.Combination)
                {
                    // Combination 空間：啟動巴斯卡解碼
                    blockTotal = PascalTable[space.PoolSize, space.Dimensions];
                    
                    int varsLeft = space.Dimensions;
                    for (int v = 0; v < space.Dimensions; v++)
                    {
                        int currentRankOffset = parsedOffsets[offsetPointer - (space.Dimensions - 1) + v];
                        if (currentRankOffset >= varsLeft)
                        {
                            blockIndex += PascalTable[currentRankOffset, varsLeft];
                        }
                        varsLeft--;
                    }
                    offsetPointer -= space.Dimensions;
                }

                // 將當前區塊的索引加入 Global 總和中，並乘上累積的基底權重
                globalIndex += blockIndex * currentMultiplier;
                
                // 擴大下一個區塊 (更高位元) 的進位倍率
                currentMultiplier *= blockTotal; 
            }

            // 計算這個多維宇宙的總組合數 (-1 是因為 Index 從 0 開始)
            long maxIndex = currentMultiplier - 1;
            if (maxIndex <= 0) return probMin;

            // 歸一化並內插至指定勝率區間
            double normalizedStrength = (double)globalIndex / maxIndex;
            return probMin + normalizedStrength * (probMax - probMin);
        }
    }
}

/*
namespace GenericPoker.EightCard
{
    public static class PokerMath
    {
        /// <summary>
        /// 完全依據動態迴圈與純數學公式計算 N 張牌烏龍的微觀勝率
        /// </summary>
        /// <param name="sortedRanks">已由大到小排序的 Kicker 點位清單 (長度為變數 N，點位 2~14)</param>
        /// <param name="probMin">該牌型的 CDF 最小勝率</param>
        /// <param name="probMax">該牌型的 CDF 最大勝率</param>
        public static double GetDynamicWinRate(IReadOnlyList<int> sortedRanks, double probMin, double probMax)
        {
            int numCards = sortedRanks.Count;
            if (numCards == 0) return probMin;

            // ------------------------------------------------------------
            // 步驟 1：動態計算 N 張牌的宇宙總組合數 C(13, numCards)
            // ------------------------------------------------------------
            long totalCombinations = ComputeCombinationDynamically(13, numCards);
            long maxIndex = totalCombinations - 1;

            if (maxIndex <= 0) return probMin;

            // ------------------------------------------------------------
            // 步驟 2：使用迴圈遍歷每一張牌，動態累加計算出全球相對排名 (globalIndex)
            // ------------------------------------------------------------
            long globalIndex = 0;
            for (int i = 0; i < numCards; i++)
            {
                int currentRank = sortedRanks[i];
                
                // k 代表當前位置往後，還需要選取幾張牌（動態調整剩餘空位）
                int k = numCards - i; 
                
                // poolSize 代表比當前牌面點數小的可用牌堆總數 (c - 2)
                int poolSize = currentRank - 2;

                // 只有當剩餘的牌堆數量夠填滿接下來的空位時，才動態計算組合數並累加
                if (poolSize >= k)
                {
                    globalIndex += ComputeCombinationDynamically(poolSize, k);
                }
            }

            // ------------------------------------------------------------
            // 步驟 3：基於步驟 1 與 2 的結果，將 globalIndex 比例內插至機率區間
            // ------------------------------------------------------------
            double normalizedStrength = (double)globalIndex / maxIndex;
            return probMin + normalizedStrength * (probMax - probMin);
        }

        /// <summary>
        /// 核心數學工具：利用動態迴圈，純手工計算 C(n, k) 
        /// 這裡會自動處理你提到的動態分母 (如 1*2*3...*k) 與分子展開
        /// </summary>
        private static long ComputeCombinationDynamically(int n, int k)
        {
            if (k > n || k < 0) return 0;
            if (k == 0 || k == n) return 1;
            
            // 組合數學優化：C(n, k) 等於 C(n, n-k)，可以減少迴圈次數
            if (k > n / 2) k = n - k;

            long result = 1;
            for (int i = 1; i <= k; i++)
            {
                result *= (n - i + 1); // 分子階層展開
                result /= i;           // 分母階層展開 (也就是你提到的 /6, /2, /1 的動態來源)
            }
            return result;
        }
    }
}
*/