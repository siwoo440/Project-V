// 시트 분할: 투명한 틈을 기준으로 시트를 줄과 칸으로 나눈다.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

public static partial class ThemeProc
{
    // 시트에서 조각의 위치를 읽는 순서(왼쪽 위부터)로 돌려준다.
    public static List<Rectangle> Split(Pix p, int rows, int cols, StringBuilder log)
    {
        List<Rectangle> result = new List<Rectangle>();
        Rectangle whole = new Rectangle(0, 0, p.W, p.H);

        foreach (int[] rowBand in Bands(p, whole, true, rows, log))
        {
            Rectangle rowArea = new Rectangle(0, rowBand[0], p.W, rowBand[1] - rowBand[0]);

            foreach (int[] colBand in Bands(p, rowArea, false, cols, log))
            {
                Rectangle cell = new Rectangle(colBand[0], rowBand[0], colBand[1] - colBand[0], rowBand[1] - rowBand[0]);
                result.Add(p.BoundsIn(cell));
            }
        }

        return result;
    }

    // 그림이 있는 구간 목록 {시작, 끝, 픽셀 수}. byRows가 참이면 가로줄 단위로 본다.
    private static List<int[]> Bands(Pix p, Rectangle area, bool byRows, int expected, StringBuilder log)
    {
        int length = byRows ? area.Height : area.Width;
        int offset = byRows ? area.Top : area.Left;
        int[] counts = new int[length];

        for (int y = area.Top; y < area.Bottom; y++)
        {
            for (int x = area.Left; x < area.Right; x++)
            {
                if (p.Solid(x, y)) { counts[(byRows ? y : x) - offset]++; }
            }
        }

        int emptyLimit = Math.Max(1, (byRows ? area.Width : area.Height) / 400);
        List<int[]> bands = new List<int[]>();
        int start = -1;
        int sum = 0;

        for (int i = 0; i <= length; i++)
        {
            bool filled = i < length && counts[i] > emptyLimit;

            if (filled)
            {
                if (start < 0) { start = i; sum = 0; }
                sum += counts[i];
            }
            else if (start >= 0)
            {
                bands.Add(new int[] { start + offset, i + offset, sum });
                start = -1;
            }
        }

        int largest = 0;

        foreach (int[] band in bands) { largest = Math.Max(largest, band[2]); }

        bands.RemoveAll(delegate(int[] band) { return band[2] < largest * 0.02; });

        // 조각 안에 틈이 있어 구간이 많이 나오면 가장 좁은 틈부터 합친다.
        while (bands.Count > expected)
        {
            int best = 0;

            for (int i = 1; i < bands.Count - 1; i++)
            {
                if (bands[i + 1][0] - bands[i][1] < bands[best + 1][0] - bands[best][1]) { best = i; }
            }

            bands[best] = new int[] { bands[best][0], bands[best + 1][1], bands[best][2] + bands[best + 1][2] };
            bands.RemoveAt(best + 1);
        }

        if (bands.Count == expected) { return bands; }

        // 틈을 찾지 못하면 같은 크기로 나눈다.
        log.AppendLine("    WARN no clear gap (" + (byRows ? "rows" : "cols") + " found " + bands.Count + ", expected " + expected + "): split evenly");

        int from = bands.Count > 0 ? bands[0][0] : offset;
        int to = bands.Count > 0 ? bands[bands.Count - 1][1] : offset + length;
        List<int[]> even = new List<int[]>();

        for (int i = 0; i < expected; i++)
        {
            even.Add(new int[] { from + (to - from) * i / expected, from + (to - from) * (i + 1) / expected, 0 });
        }

        return even;
    }

    // 한 줄(세로줄 또는 가로줄)을 미리 곱한 색으로 읽는다.
    private static float[] Line(Pix p, bool column, int index)
    {
        int length = column ? p.H : p.W;
        float[] line = new float[length * 4];

        for (int k = 0; k < length; k++)
        {
            int i = (column ? k * p.W + index : index * p.W + k) * 4;
            float a = p.D[i + 3] / 255f;

            line[k * 4] = p.D[i] * a;
            line[k * 4 + 1] = p.D[i + 1] * a;
            line[k * 4 + 2] = p.D[i + 2] * a;
            line[k * 4 + 3] = p.D[i + 3];
        }

        return line;
    }

    // 모양이 급하게 바뀌는 곳(장식)을 찾아 양쪽 끝 장식의 길이를 구한다.
    // 가운데 색과 비교하면 광택이나 그라데이션까지 장식으로 보게 되므로, 몇 픽셀 떨어진 이웃 줄과의 변화량을 쓴다.
    // horizontal이 참이면 좌우(first=왼쪽, second=오른쪽), 거짓이면 상하(first=위, second=아래).
    public static void EstimateCaps(Pix p, bool horizontal, out int first, out int second)
    {
        int length = horizontal ? p.W : p.H;
        int across = horizontal ? p.H : p.W;
        int gap = Math.Max(2, length / 200);
        int window = Math.Max(4, across / 16);
        float[] step = new float[length];
        float[][] lines = new float[length][];

        for (int index = 0; index < length; index++) { lines[index] = Line(p, horizontal, index); }

        for (int index = 0; index + gap < length; index++)
        {
            float running = 0f;
            float peak = 0f;

            for (int k = 0; k < across; k++)
            {
                running += PixelDiff(lines[index], lines[index + gap], k);

                if (k >= window) { running -= PixelDiff(lines[index], lines[index + gap], k - window); }
                if (k >= window - 1) { peak = Math.Max(peak, running / window); }
            }

            step[index] = peak;
        }

        // 가운데 구간의 변화량 중앙값을 잡음 기준으로 삼는다.
        List<float> middle = new List<float>();

        for (int index = (int)(length * 0.3f); index < (int)(length * 0.7f); index++) { middle.Add(step[index]); }

        middle.Sort();

        float noise = middle.Count > 0 ? middle[middle.Count / 2] : 0f;
        float limit = Math.Max(5f, noise * 3f + 2f);
        int center = length / 2;
        int left = center;
        int right = center;

        while (left > 1 && !(step[left - 1] > limit && step[left - 2] > limit)) { left--; }
        while (right < length - gap - 2 && !(step[right + 1] > limit && step[right + 2] > limit)) { right++; }

        int margin = Math.Max(3, length / 80);
        int maximum = (int)(length * 0.46f);

        first = Math.Max(2, Math.Min(maximum, left + gap + margin));
        second = Math.Max(2, Math.Min(maximum, length - 1 - right + margin));
    }

    // 한 픽셀의 네 채널 차이 평균
    private static float PixelDiff(float[] line, float[] reference, int k)
    {
        return (Math.Abs(line[k * 4] - reference[k * 4]) +
                Math.Abs(line[k * 4 + 1] - reference[k * 4 + 1]) +
                Math.Abs(line[k * 4 + 2] - reference[k * 4 + 2]) +
                Math.Abs(line[k * 4 + 3] - reference[k * 4 + 3])) / 4f;
    }
}
