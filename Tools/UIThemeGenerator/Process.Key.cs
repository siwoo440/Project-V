// 배경 제거: 실제 알파, 마젠타 단색, 단색·체크무늬 배경을 구분해 투명하게 만든다.
using System;
using System.Collections.Generic;
using System.Drawing;

public static partial class ThemeProc
{
    private static int Dist(byte[] d, int i, int[] key)
    {
        int db = Math.Abs(d[i] - key[0]);
        int dg = Math.Abs(d[i + 1] - key[1]);
        int dr = Math.Abs(d[i + 2] - key[2]);
        return Math.Max(db, Math.Max(dg, dr));
    }

    private static bool IsMagenta(int[] key)
    {
        return key[2] >= 190 && key[0] >= 190 && key[1] <= 110;
    }

    // 배경을 투명하게 만들고 처리 방식을 돌려준다.
    public static string RemoveBackground(Pix p)
    {
        int total = p.W * p.H;
        int clear = 0;

        for (int i = 0; i < total; i++)
        {
            if (p.D[i * 4 + 3] < 128) { clear++; }
        }

        if (clear > total * 0.03)
        {
            CleanAlpha(p);
            Despeckle(p);
            return "alpha";
        }

        List<int> border = BorderPixels(p);
        int[] key1 = DominantColor(p, border, null, 0);
        int count1 = CountNear(p, border, key1, 36);
        int[] key2 = DominantColor(p, border, key1, 36);
        int count2 = key2 == null ? 0 : CountNear(p, border, key2, 36);

        if (IsMagenta(key1) && count1 > border.Count * 0.6)
        {
            KeyMagenta(p, key1);
            Despeckle(p);
            return "magenta";
        }

        if (count1 + count2 > border.Count * 0.9)
        {
            int remain = KeyFlood(p, key1, key2 == null ? key1 : key2, 30);
            Despeckle(p);

            return key2 == null
                ? "solid(" + key1[2] + "," + key1[1] + "," + key1[0] + ") enclosed=" + remain
                : "checker enclosed=" + remain;
        }

        return "opaque";
    }

    private static List<int> BorderPixels(Pix p)
    {
        List<int> result = new List<int>();
        int t = Math.Max(3, Math.Min(p.W, p.H) / 200);

        for (int y = 0; y < p.H; y++)
        {
            bool edgeRow = y < t || y >= p.H - t;

            for (int x = 0; x < p.W; x += edgeRow ? 2 : 1)
            {
                if (!edgeRow && x >= t && x < p.W - t) { x = p.W - t - 1; continue; }

                result.Add(y * p.W + x);
            }
        }

        return result;
    }

    // 가장 많이 나온 색을 구한다. skip이 있으면 그 색과 가까운 표본은 제외한다.
    private static int[] DominantColor(Pix p, List<int> samples, int[] skip, int skipRange)
    {
        Dictionary<int, long[]> bins = new Dictionary<int, long[]>();

        foreach (int index in samples)
        {
            int i = index * 4;

            if (skip != null && Dist(p.D, i, skip) <= skipRange) { continue; }

            int bin = (p.D[i] >> 4) | ((p.D[i + 1] >> 4) << 4) | ((p.D[i + 2] >> 4) << 8);
            long[] sum;

            if (!bins.TryGetValue(bin, out sum))
            {
                sum = new long[4];
                bins.Add(bin, sum);
            }

            sum[0] += p.D[i];
            sum[1] += p.D[i + 1];
            sum[2] += p.D[i + 2];
            sum[3] += 1;
        }

        long[] best = null;

        foreach (long[] sum in bins.Values)
        {
            if (best == null || sum[3] > best[3]) { best = sum; }
        }

        if (best == null || best[3] < Math.Max(8, samples.Count / 20)) { return null; }

        return new int[] { (int)(best[0] / best[3]), (int)(best[1] / best[3]), (int)(best[2] / best[3]) };
    }

    private static int CountNear(Pix p, List<int> samples, int[] key, int range)
    {
        int count = 0;

        foreach (int index in samples)
        {
            if (Dist(p.D, index * 4, key) <= range) { count++; }
        }

        return count;
    }

    private static void CleanAlpha(Pix p)
    {
        for (int i = 0; i < p.W * p.H; i++)
        {
            if (p.D[i * 4 + 3] >= 10) { continue; }

            p.D[i * 4] = 0;
            p.D[i * 4 + 1] = 0;
            p.D[i * 4 + 2] = 0;
            p.D[i * 4 + 3] = 0;
        }
    }

    // 가장자리에서 시작해 조건에 맞는 픽셀을 따라 번져 나간다.
    private static bool[] FloodFromBorder(Pix p, bool[] match)
    {
        bool[] removed = new bool[p.W * p.H];
        Stack<int> stack = new Stack<int>();

        for (int x = 0; x < p.W; x++)
        {
            Push(stack, removed, match, x);
            Push(stack, removed, match, (p.H - 1) * p.W + x);
        }

        for (int y = 0; y < p.H; y++)
        {
            Push(stack, removed, match, y * p.W);
            Push(stack, removed, match, y * p.W + p.W - 1);
        }

        while (stack.Count > 0)
        {
            int index = stack.Pop();
            int x = index % p.W;
            int y = index / p.W;

            if (x > 0) { Push(stack, removed, match, index - 1); }
            if (x < p.W - 1) { Push(stack, removed, match, index + 1); }
            if (y > 0) { Push(stack, removed, match, index - p.W); }
            if (y < p.H - 1) { Push(stack, removed, match, index + p.W); }
        }

        return removed;
    }

    private static void Push(Stack<int> stack, bool[] removed, bool[] match, int index)
    {
        if (removed[index] || !match[index]) { return; }

        removed[index] = true;
        stack.Push(index);
    }
}
