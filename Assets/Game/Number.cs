using System;
using System.Numerics;

using UnityEngine;

[Serializable]
public struct Number : IEquatable<Number>, IComparable<Number>
{
    public static Number Zero = new Number(0);
    public static Number Delta = new Number(new BigInteger(1));

    private const int PrecisionFactor = 1000;

    private readonly BigInteger fixedPoint;

    public Number(float number)
    {
        this.fixedPoint = (long)Math.Round((double)number * PrecisionFactor);
    }

    public Number(int number)
    {
        this.fixedPoint = number;
        this.fixedPoint *= PrecisionFactor;
    }

    public Number(long number)
    {
        this.fixedPoint = number;
        this.fixedPoint *= PrecisionFactor;
    }

    private Number(BigInteger fixedPoint)
    {
        this.fixedPoint = fixedPoint;
    }

    public enum Exponent
    {
        Mili = -3,
        None = 0,
        Kilo = 3,
        Mega = 6,
        Terra = 9
    }

    public static Number FromFixedPoint(long fixedPoint)
    {
        return new Number(new BigInteger(fixedPoint));
    }

    public static explicit operator int(Number number)
    {
        return (int)(number.fixedPoint / PrecisionFactor);
    }

    public static explicit operator long(Number number)
    {
        return (long)(number.fixedPoint / PrecisionFactor);
    }

    //public static explicit operator Number(long number)
    //{
    //    return new Number(number);
    //}

    public static explicit operator float(Number number)
    {
        return (float)number.fixedPoint / PrecisionFactor;
    }

    //public static explicit operator Number(int number)
    //{
    //    return new Number(number);
    //}

    //public static explicit operator Number(float number)
    //{
    //    return new Number(number);
    //}

    public static Number operator +(Number left, Number right)
    {
        return new Number(left.fixedPoint + right.fixedPoint);
    }

    public static Number operator +(Number left, float right)
    {
        return left + new Number(right);
    }

    public static Number operator +(Number left, int right)
    {
        return left + new Number(right);
    }

    public static Number operator -(Number left, Number right)
    {
        return new Number(left.fixedPoint - right.fixedPoint);
    }

    public static Number operator -(Number left, float right)
    {
        return left - new Number(right);
    }

    public static Number operator -(Number left, int right)
    {
        return left - new Number(right);
    }

    public static Number operator *(Number left, Number right)
    {
        return new Number(left.fixedPoint * right.fixedPoint / PrecisionFactor);
    }

    public static Number operator *(Number left, float right)
    {
        return left * new Number(right);
    }

    public static Number operator *(Number left, int right)
    {
        return left * new Number(right);
    }

    public static Number operator /(Number left, Number right)
    {
        return new Number(PrecisionFactor * left.fixedPoint / right.fixedPoint);
    }

    public static Number operator /(Number left, float right)
    {
        return left / new Number(right);
    }

    public static Number operator /(Number left, int right)
    {
        return left / new Number(right);
    }

    public static bool operator ==(Number left, Number right)
    {
        return left.fixedPoint == right.fixedPoint;
    }

    public static bool operator !=(Number left, Number right)
    {
        return left.fixedPoint != right.fixedPoint;
    }

    public static bool operator >(Number left, Number right)
    {
        return left.fixedPoint > right.fixedPoint;
    }

    public static bool operator >=(Number left, Number right)
    {
        return left.fixedPoint >= right.fixedPoint;
    }

    public static bool operator <(Number left, Number right)
    {
        return left.fixedPoint < right.fixedPoint;
    }

    public static bool operator <=(Number left, Number right)
    {
        return left.fixedPoint <= right.fixedPoint;
    }

    public static Number Min(Number left, Number right)
    {
        return left.fixedPoint < right.fixedPoint ? left : right;
    }

    public static Number Max(Number left, Number right)
    {
        return left.fixedPoint > right.fixedPoint ? left : right;
    }

    public bool Equals(Number other)
    {
        return this.fixedPoint == other.fixedPoint;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        return obj is Number && Equals((Number)obj);
    }

    public override int GetHashCode()
    {
        return this.fixedPoint.GetHashCode();
    }

    public int CompareTo(Number other)
    {
        return this.fixedPoint.CompareTo(other.fixedPoint);
    }

    public override string ToString()
    {
        return this.ToString(false);
    }

    public string ToString(bool alwaysDisplaySign)
    {
        var absFixedPoint = this.fixedPoint;
        int sign = 1;
        if (absFixedPoint < 0)
        {
            sign = -1;
            absFixedPoint = -this.fixedPoint;
        }

        Exponent exponent = Exponent.None;
        if (absFixedPoint > 0)
        {
            float exponentIndex = Mathf.Log10((float)absFixedPoint) - Mathf.Log10(PrecisionFactor);
            exponent = (Exponent)(Mathf.FloorToInt(exponentIndex / 3) * 3);
        }

        uint pow = (uint)Mathf.Max(0, (int)exponent);
        int divisor = Number.IntPow(10, pow) * PrecisionFactor;
        double value = (double)absFixedPoint / divisor;

        double displayedValue = 0;
        if (value > 0)
        {
            var precision = Math.Min(3, 2 - Mathf.FloorToInt((float) Math.Log10(value)));
            int precisionFactor = Number.IntPow(10, (uint)precision);
            displayedValue = Math.Floor(value * precisionFactor) / precisionFactor;
        }

        displayedValue *= sign;

        string stringValue = $"{displayedValue}";
        if (alwaysDisplaySign)
        {
            if (displayedValue > 0)
            {
                stringValue = "+" + displayedValue;
            }
        }

        if (exponent <= Exponent.None)
        {
            return stringValue;
        }

        return $"{stringValue}{Abbreviation(exponent)}";
    }

    private string Abbreviation(Exponent exponent)
    {
        switch (exponent)
        {
            case Exponent.Mili:
                return "m";

            case Exponent.None:
                return string.Empty;

            case Exponent.Kilo:
                return "K";

            case Exponent.Mega:
                return "M";

            case Exponent.Terra:
                return "T";

            default:
                throw new ArgumentOutOfRangeException(nameof(exponent), exponent, null);
        }
    }

    private static int IntPow(int x, uint pow)
    {
        int ret = 1;
        while (pow != 0)
        {
            if ((pow & 1) == 1)
            {
                ret *= x;
            }

            x *= x;
            pow >>= 1;
        }

        return ret;
    }
}
