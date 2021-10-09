namespace covergen;

internal static class ScaleRatioCaculator
{
    internal static int CalculateRatio(int sourceWidth, int targetWidth)
    {
        int ratio = 1;
        while (ratio * sourceWidth < targetWidth) ratio *= 2;
        return ratio;
    }
}