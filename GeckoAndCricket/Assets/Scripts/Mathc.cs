using System;
      
// ReSharper disable MemberCanBePrivate.Global

public static class Mathc
{
    private const float TOLERANCE = 0.0001f;
    public static int GetExponent(int number)
    {
        if (number % 2 == 1)
            return 0;
        
        int exponentValue = 1;
        do
        {
            if (Math.Abs(Math.Pow(2, exponentValue) - number) < TOLERANCE)
                return exponentValue;
            exponentValue++;
        } while (true);
    }
    
    public static int GetExponent(int? number)
    {
        return number == null ? 0 : GetExponent((int)number);
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
        return number == null ? 0 : GetExponent((float)number);
    }
}
