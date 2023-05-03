using System;
using System.Collections.Generic;

public static class StaticMath
{
    private static readonly Random random = new Random();
    private static readonly object syncLock = new object();

    public static void Randomize(Matrix m)
    {
        for (int i = 0; i < m.rows; i++)
        {
            for (int j = 0; j < m.columns; j++)
            {
                m.data[i, j] = GetRandomFloat(-1.0f, 1.0f);
            }
        }
    }

    public static Matrix XavierInitialize(Matrix matrix, int layerSize)
    {
        float variance = (float)Math.Sqrt(2.0 / layerSize);
        for (int i = 0; i < matrix.rows; i++)
        {
            for (int j = 0; j < matrix.columns; j++)
            {
                matrix.data[i, j] = GetRandomFloat(0.0f, 1.0f) * 2 * variance - variance;
            }
        }
        return matrix;
    }

    public static Matrix HeInitialize(Matrix matrix, int inputSize)
    {
        float variance = (float)Math.Sqrt(100.0 / inputSize);
        for (int i = 0; i < matrix.rows; i++)
        {
            for (int j = 0; j < matrix.columns; j++)
            {
                matrix.data[i, j] = GetRandomFloat(0.0f, 1.0f) * variance;
            }
        }
        return matrix;
    }

    public static float GetRandomFloat(float min, float max)
    {
        lock (syncLock)
        {
            return (float)random.NextDouble() * (max - min) + min;
        }
    }

    public static int GetRandomInteger(int min, int max)
    {
        lock (syncLock)
        {
            return random.Next(min, max);
        }
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static float GetDistBetweenPoints(float x1, float y1, float x2, float y2)
    {
        return (float)Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2));
    }

    public static float GetAngleBetweenPoints(float x1, float y1, float x2, float y2)
    {
        float xDiff = x2 - x1;
        float yDiff = y2 - y1;
        return (float)(Math.Atan2(yDiff, xDiff) * 180.0f / Math.PI);
    }

    public static float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}