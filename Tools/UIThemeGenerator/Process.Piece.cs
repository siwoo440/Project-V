// 조각 가공: 늘어나는 구간 줄이기, 크기 맞추기
using System;
using System.Drawing;

public static partial class ThemeProc
{
    // 가공이 끝난 조각 하나
    public sealed class Piece
    {
        public string Key;     // 파일 이름(확장자 제외)
        public string TmpName; // 글자 사이 아이콘 이름 (아이콘만)
        public string Kind;    // nine, three, card, emblem, icon
        public string Folder;  // Art 기준 폴더
        public Pix Image;
        public Pix Source;     // 줄이기 전 모습 (미리보기용)
        public int L, B, R, T; // 9분할 경계
    }

    private static Pix FitMax(Pix p, int maxSide)
    {
        int longest = Math.Max(p.W, p.H);

        if (longest <= maxSide) { return p; }

        return p.Resize(Math.Max(1, p.W * maxSide / longest), Math.Max(1, p.H * maxSide / longest));
    }

    private static Pix FitHeight(Pix p, int maxHeight)
    {
        if (p.H <= maxHeight) { return p; }

        return p.Resize(Math.Max(1, p.W * maxHeight / p.H), maxHeight);
    }

    private static Pix Transpose(Pix p)
    {
        Pix result = new Pix(p.H, p.W);

        for (int y = 0; y < p.H; y++)
        {
            for (int x = 0; x < p.W; x++)
            {
                Buffer.BlockCopy(p.D, (y * p.W + x) * 4, result.D, (x * p.H + y) * 4, 4);
            }
        }

        return result;
    }

    // 좌우로 늘어나는 가운데 구간을 짧게 다시 그려 조각을 줄인다.
    // 평균색으로 바꾸지 않고 축소하므로 광택과 그라데이션이 그대로 남는다.
    public static Pix CompactH(Pix p, int left, int right, int middle)
    {
        int from = left;
        int to = p.W - right;

        if (to - from <= middle) { return p; }

        Pix shrunk = p.Crop(new Rectangle(from, 0, to - from, p.H)).Resize(middle, p.H);
        Pix result = new Pix(left + middle + right, p.H);

        for (int y = 0; y < p.H; y++)
        {
            Buffer.BlockCopy(p.D, y * p.W * 4, result.D, y * result.W * 4, left * 4);
            Buffer.BlockCopy(shrunk.D, y * middle * 4, result.D, (y * result.W + left) * 4, middle * 4);
            Buffer.BlockCopy(p.D, (y * p.W + to) * 4, result.D, (y * result.W + left + middle) * 4, right * 4);
        }

        return result;
    }

    // 상하로 늘어나는 가운데 구간을 줄인다.
    public static Pix CompactV(Pix p, int top, int bottom, int middle)
    {
        return Transpose(CompactH(Transpose(p), top, bottom, middle));
    }

    // 틀 안쪽에 갇힌 투명한 부분을 안쪽 색으로 메운다. (카드 틀 안쪽이 비쳐 보이는 문제 방지)
    // 메운 픽셀 수를 돌려준다.
    public static int FillInterior(Pix p)
    {
        int total = p.W * p.H;
        bool[] loose = new bool[total];

        for (int n = 0; n < total; n++) { loose[n] = p.D[n * 4 + 3] < 250; }

        bool[] outer = FloodFromBorder(p, loose); // 바깥과 이어진 투명 영역
        long[] sum = new long[4];

        for (int y = p.H / 4; y < p.H * 3 / 4; y++)
        {
            for (int x = p.W / 4; x < p.W * 3 / 4; x++)
            {
                int i = (y * p.W + x) * 4;

                if (p.D[i + 3] < 250) { continue; }

                sum[0] += p.D[i];
                sum[1] += p.D[i + 1];
                sum[2] += p.D[i + 2];
                sum[3] += 1;
            }
        }

        if (sum[3] == 0) { return 0; }

        int[] fill = { (int)(sum[0] / sum[3]), (int)(sum[1] / sum[3]), (int)(sum[2] / sum[3]) };
        int filled = 0;

        for (int n = 0; n < total; n++)
        {
            int i = n * 4;
            int alpha = p.D[i + 3];

            if (outer[n]) { continue; }

            // 비쳐 보이던 픽셀과 안쪽 색에 가까운 픽셀은 안쪽 색으로 통일해 얼룩을 없앤다.
            bool isHole = alpha < 250;
            bool isNearFill = Dist(p.D, i, fill) <= 30;

            if (!isHole && !isNearFill) { continue; }

            p.D[i] = (byte)fill[0];
            p.D[i + 1] = (byte)fill[1];
            p.D[i + 2] = (byte)fill[2];
            p.D[i + 3] = 255;

            if (isHole) { filled++; }
        }

        return filled;
    }

    // 16:9로 가운데를 잘라 지정한 크기로 맞춘다. (배경 그림)
    public static Pix CropToAspect(Pix p, int width, int height)
    {
        int cropW = p.W;
        int cropH = p.W * height / width;

        if (cropH > p.H)
        {
            cropH = p.H;
            cropW = p.H * width / height;
        }

        Rectangle area = new Rectangle((p.W - cropW) / 2, (p.H - cropH) / 2, cropW, cropH);

        return p.Crop(area).Resize(width, height);
    }
}
