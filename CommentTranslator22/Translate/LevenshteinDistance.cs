using System;

namespace CommentTranslator22.Translate
{
    /// <summary>
    /// 计算字符串相似度的算法，使用 Levenshtein 距离
    /// <a href="https://zhuanlan.zhihu.com/p/352503879">来源地址</a>
    /// 目前更改为使用滚动数组来减少空间复杂度，但时间复杂度仍为 O(n*m)
    /// </summary>
    internal static class LevenshteinDistance
    {
        /// <summary>
        /// 计算两个字符串的 Levenshtein 距离
        /// </summary>
        private static int CalculateLevenshteinDistance(string str1, string str2)
        {
            int n = str1.Length;
            int m = str2.Length;

            // 确保 str1 是较短的字符串，以减少空间复杂度
            if (n > m)
            {
                (str1, str2) = (str2, str1);
                (n, m) = (m, n);
            }

            // 使用滚动数组来减少空间复杂度
            int[] previousRow = new int[n + 1];
            int[] currentRow = new int[n + 1];

            // 初始化第一行
            for (int i = 0; i <= n; i++)
            {
                previousRow[i] = i;
            }

            for (int j = 1; j <= m; j++)
            {
                currentRow[0] = j; // 初始化当前行的第一个元素

                for (int i = 1; i <= n; i++)
                {
                    int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;

                    currentRow[i] = Math.Min(
                        Math.Min(
                            previousRow[i] + 1,      // 删除操作
                            currentRow[i - 1] + 1),  // 插入操作
                        previousRow[i - 1] + cost    // 替换操作
                    );
                }

                // 交换 previousRow 和 currentRow
                (previousRow, currentRow) = (currentRow, previousRow);
            }

            // 最终结果在 previousRow[n] 中
            return previousRow[n];
        }

        /// <summary>
        /// 计算字符串相似度（百分比）
        /// </summary>
        public static float LevenshteinDistancePercent(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1))
                return string.IsNullOrEmpty(str2) ? 1.0f : 0.0f;
            if (string.IsNullOrEmpty(str2))
                return 0.0f;

            int distance = CalculateLevenshteinDistance(str1, str2);
            int maxLength = Math.Max(str1.Length, str2.Length);

            return 1.0f - (float)distance / maxLength;
        }
    }
}