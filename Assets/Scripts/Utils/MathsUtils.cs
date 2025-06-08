namespace Utils
{
    public static class MathsUtils
    {
        public static float Modulo(float a, float b)
        {
            return (a % b + b) % b;
        }
    }
}