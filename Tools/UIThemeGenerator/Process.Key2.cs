// 배경 제거 2: 마젠타·단색 배경을 지우고 가장자리를 다듬는다.
using System;
using System.Collections.Generic;

public static partial class ThemeProc
{
    private static void KeyMagenta(Pix p, int[] key)
    {
        int total = p.W * p.H;
        bool[] strong = new bool[total];
        bool[] pure = new bool[total];

        for (int n = 0; n < total; n++)
        {
            int i = n * 4;
            int b = p.D[i], g = p.D[i + 1], r = p.D[i + 2];
            int distance = Dist(p.D, i, key);

            strong[n] = distance <= 70 || (Math.Min(r, b) - g >= 150 && r >= 165 && b >= 165);
            pure[n] = distance <= 40;
        }

        bool[] removed = FloodFromBorder(p, strong);

        // 그림 안에 갇힌 배경(고리 안쪽 등)도 지운다.
        foreach (List<int> hole in Components(p.W, p.H, pure, removed))
        {
            if (hole.Count < 48) { continue; }

            foreach (int n in hole) { removed[n] = true; }
        }

        SoftenEdge(p, removed, key, key, 2, true);
        Erase(p, removed);
    }

    // 가장자리와 이어진 배경색만 지운다. 지우지 못한 배경색 픽셀 수를 돌려준다.
    private static int KeyFlood(Pix p, int[] key1, int[] key2, int tolerance)
    {
        int total = p.W * p.H;
        bool[] match = new bool[total];

        for (int n = 0; n < total; n++)
        {
            match[n] = Dist(p.D, n * 4, key1) <= tolerance || Dist(p.D, n * 4, key2) <= tolerance;
        }

        bool[] removed = FloodFromBorder(p, match);
        int remain = 0;

        for (int n = 0; n < total; n++)
        {
            if (match[n] && !removed[n]) { remain++; }
        }

        SoftenEdge(p, removed, key1, key2, 1, false);
        Erase(p, removed);

        return remain;
    }

    private static void Erase(Pix p, bool[] removed)
    {
        for (int n = 0; n < removed.Length; n++)
        {
            if (!removed[n]) { continue; }

            p.D[n * 4] = 0;
            p.D[n * 4 + 1] = 0;
            p.D[n * 4 + 2] = 0;
            p.D[n * 4 + 3] = 0;
        }
    }

    // 지운 영역과 맞닿은 픽셀에서 배경색이 섞인 만큼 투명도를 낮추고 색을 되돌린다.
    private static void SoftenEdge(Pix p, bool[] removed, int[] key1, int[] key2, int width, bool magenta)
    {
        bool[] near = new bool[removed.Length];

        for (int y = 0; y < p.H; y++)
        {
            for (int x = 0; x < p.W; x++)
            {
                int n = y * p.W + x;

                if (removed[n]) { continue; }

                for (int dy = -width; dy <= width && !near[n]; dy++)
                {
                    for (int dx = -width; dx <= width; dx++)
                    {
                        int xx = x + dx, yy = y + dy;

                        if (xx < 0 || yy < 0 || xx >= p.W || yy >= p.H) { continue; }
                        if (!removed[yy * p.W + xx]) { continue; }

                        near[n] = true;
                        break;
                    }
                }
            }
        }

        for (int n = 0; n < near.Length; n++)
        {
            if (!near[n]) { continue; }

            int i = n * 4;
            int[] key = Dist(p.D, i, key1) <= Dist(p.D, i, key2) ? key1 : key2;
            float mix; // 배경이 섞인 비율

            if (magenta)
            {
                float level = Math.Min(p.D[i + 2], p.D[i]) - p.D[i + 1];
                float keyLevel = Math.Max(120f, Math.Min(key[2], key[0]) - key[1]);
                mix = (level - 40f) / (keyLevel - 40f);
            }
            else
            {
                mix = 1f - (Dist(p.D, i, key) - 30f) / 60f;
            }

            mix = Math.Max(0f, Math.Min(1f, mix));

            if (mix <= 0.02f) { continue; }

            float alpha = 1f - mix;

            if (alpha < 0.08f)
            {
                removed[n] = true;
                continue;
            }

            for (int c = 0; c < 3; c++)
            {
                float value = (p.D[i + c] - mix * key[c]) / alpha;
                p.D[i + c] = (byte)Math.Max(0f, Math.Min(255f, value));
            }

            p.D[i + 3] = (byte)(p.D[i + 3] * alpha);
        }
    }

    // 조건에 맞고 제외 대상이 아닌 픽셀의 연결 덩어리 목록
    private static List<List<int>> Components(int w, int h, bool[] match, bool[] exclude)
    {
        List<List<int>> result = new List<List<int>>();
        bool[] seen = new bool[match.Length];
        Stack<int> stack = new Stack<int>();

        for (int start = 0; start < match.Length; start++)
        {
            if (seen[start] || !match[start]) { continue; }
            if (exclude != null && exclude[start]) { continue; }

            List<int> part = new List<int>();
            seen[start] = true;
            stack.Push(start);

            while (stack.Count > 0)
            {
                int index = stack.Pop();
                int x = index % w;
                int y = index / w;

                part.Add(index);

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        int xx = x + dx, yy = y + dy;

                        if (xx < 0 || yy < 0 || xx >= w || yy >= h) { continue; }

                        int next = yy * w + xx;

                        if (seen[next] || !match[next]) { continue; }
                        if (exclude != null && exclude[next]) { continue; }

                        seen[next] = true;
                        stack.Push(next);
                    }
                }
            }

            result.Add(part);
        }

        return result;
    }

    // 배경 제거 뒤 남은 작은 점을 지운다.
    private static void Despeckle(Pix p)
    {
        int total = p.W * p.H;
        bool[] solid = new bool[total];

        for (int n = 0; n < total; n++) { solid[n] = p.D[n * 4 + 3] > OpaqueAlpha; }

        int limit = Math.Max(16, total / 25000);

        foreach (List<int> part in Components(p.W, p.H, solid, null))
        {
            if (part.Count >= limit) { continue; }

            foreach (int n in part)
            {
                p.D[n * 4] = 0;
                p.D[n * 4 + 1] = 0;
                p.D[n * 4 + 2] = 0;
                p.D[n * 4 + 3] = 0;
            }
        }
    }
}
