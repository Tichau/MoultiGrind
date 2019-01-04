namespace DefaultNamespace
{
    using UnityEngine;

    public static class FloatExtensions
    {
        public static string Prettify(this float number)
        {
            number *= 10000;
            number = Mathf.RoundToInt(number) / 10000f;

            return number.ToString();
        }
    }

    public struct Number
    {
        private int fixedPoint;
        private Exponent exponent;

        public Number(float number)
        {
            this.exponent = (Exponent)((Mathf.Log10(number) % 3) * 3);
            number /= Mathf.Pow(10, (int)this.exponent);
            this.fixedPoint = Mathf.RoundToInt(number * 1000f);
        }

        public enum Exponent
        {
            None = 0,
            Kilo = 3,
            Mega = 6,
            Terra = 9
        }

        public static implicit operator float(Number number)
        {
            return number.fixedPoint / 1000f * Mathf.Pow(10, (int)number.exponent);
        }

        public static implicit operator Number(float number)
        {
            return new Number(number);
        }
    }
}
