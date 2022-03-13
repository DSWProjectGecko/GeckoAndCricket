using System;

public static class Mathc
{
    private const float TOLERANCE = 0.0001f;
    public static int GetExponent(int number)
    {
        if (number % 2 == 1)
            return 0;
        
        int exponentValue = 1;
        while (!(Math.Abs(Math.Pow(2, exponentValue) - number) < TOLERANCE))
        {
            exponentValue++;
        }

        return exponentValue;
    }
    
    public static int GetExponent(int? number)
    {
        if (number == null || number % 2 == 1)
            return 0;
        
        int exponentValue = 1;
        do
        {
            if (Math.Abs(Math.Pow(2, exponentValue) - (int) number) < TOLERANCE)
                return exponentValue;
            exponentValue++;
        } while (true);
    }
    
    public static float GetExponent(float number)
    {
        if (Math.Abs(number % 2f - 1f) < TOLERANCE)
            return 0;
        
        float exponentValue = 1;
        do
        {
            if (Math.Abs(Math.Pow(2, exponentValue) - number) < TOLERANCE)
                return exponentValue;
            exponentValue++;
        } while (true);
    }
    
    public static float GetExponent(float? number)
    {
        if (number == null || Math.Abs((float) number % 2f - 1f) < TOLERANCE)
            return 0;
        
        float exponentValue = 1;
        do
        {
            if (Math.Abs(Math.Pow(2, exponentValue) - (float) number) < TOLERANCE)
                return exponentValue;
            exponentValue++;
        } while (true);
    }
}
