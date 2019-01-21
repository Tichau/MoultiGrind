using System.Collections;
using System.Collections.Generic;
using Gameplay;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NumberTest
{
    private const int BigNumber = 1000000000;

    private IEnumerable<int> Numbers
    {
        get
        {
            for (int value = -BigNumber; value < 0; value /= 10)
            {
                yield return value;
            }

            yield return 0;

            for (int value = 1; value < BigNumber; value *= 10)
            {
                yield return value;
            }
        }
    }

    [Test]
    public void ConversionFromValueTypeIsCorrect()
    {
        Number number = Number.Zero;
        Assert.AreEqual((int)number, 0);

        number = new Number(0f);
        Assert.AreEqual((float)number, 0f);

        foreach (int index in Numbers)
        {
            number = new Number(index);
            Assert.AreEqual(index, (int)number);

            number = new Number((float)index);
            Assert.AreEqual((float)index, (float)number);
        }
    }

    [Test]
    public void OperationAreCorrect()
    {
        foreach (int value1 in Numbers)
        {
            Number number1 = new Number(value1);
            foreach (int value2 in Numbers)
            {
                Number number2 = new Number(value2);

                // Addition
                long excepted = (long)value1 + (long)value2;
                long actual = (long)(number1 + number2);
                Assert.AreEqual(excepted, actual, $"{value1} + {value2}");

                // Subtraction
                excepted = (long)value1 - (long)value2;
                actual = (long)(number1 - number2);
                Assert.AreEqual(excepted, actual, $"{value1} - {value2}");

                // Multiplication
                excepted = (long)value1 * (long)value2;
                actual = (long)(number1 * number2);
                Assert.AreEqual(excepted, actual, $"{value1} * {value2}");

                // Division
                if (value2 != 0)
                {
                    excepted = (long)value1 / (long)value2;
                    actual = (long)(number1 / number2);
                    Assert.AreEqual(excepted, actual, $"{value1} / {value2}");
                }
            }
        }
    }

    [Test]
    public void ComparisonAreCorrect()
    {
        foreach (int value1 in Numbers)
        {
            Number number1 = new Number(value1);
            foreach (int value2 in Numbers)
            {
                Number number2 = new Number(value2);

                // Equality
                bool excepted = value1 == value2;
                bool actual = number1 == number2;
                Assert.AreEqual(excepted, actual, $"{value1} == {value2}");

                // Difference
                excepted = value1 != value2;
                actual = number1 != number2;
                Assert.AreEqual(excepted, actual, $"{value1} != {value2}");

                // Greater
                excepted = value1 > value2;
                actual = number1 > number2;
                Assert.AreEqual(excepted, actual, $"{value1} > {value2}");

                // Greater or equal
                excepted = value1 >= value2;
                actual = number1 >= number2;
                Assert.AreEqual(excepted, actual, $"{value1} >= {value2}");

                // Lower
                excepted = value1 < value2;
                actual = number1 < number2;
                Assert.AreEqual(excepted, actual, $"{value1} < {value2}");

                // Lower or equal
                excepted = value1 <= value2;
                actual = number1 <= number2;
                Assert.AreEqual(excepted, actual, $"{value1} <= {value2}");
            }
        }
    }

    [Test]
    public void NumbersAreDisplayedCorrectly()
    {
        Assert.AreEqual("0", new Number(0).ToString());
        Assert.AreEqual("0.001", new Number(0.001f).ToString());
        Assert.AreEqual("0.01", new Number(0.01f).ToString());
        Assert.AreEqual("0.1", new Number(0.1f).ToString());
        Assert.AreEqual("1", new Number(1).ToString());
        Assert.AreEqual("10", new Number(10).ToString());
        Assert.AreEqual("100", new Number(100).ToString());
        Assert.AreEqual("1K", new Number(1000).ToString());
        Assert.AreEqual("10K", new Number(10000).ToString());
        Assert.AreEqual("100K", new Number(100000).ToString());
        Assert.AreEqual("1M", new Number(1000000).ToString());
        Assert.AreEqual("10M", new Number(10000000).ToString());

        Assert.AreEqual("-0.001", new Number(-0.001f).ToString());
        Assert.AreEqual("-0.01", new Number(-0.01f).ToString());
        Assert.AreEqual("-0.1", new Number(-0.1f).ToString());
        Assert.AreEqual("-1", new Number(-1).ToString());
        Assert.AreEqual("-10", new Number(-10).ToString());
        Assert.AreEqual("-100", new Number(-100).ToString());
        Assert.AreEqual("-1K", new Number(-1000).ToString());
        Assert.AreEqual("-10K", new Number(-10000).ToString());
        Assert.AreEqual("-100K", new Number(-100000).ToString());
        Assert.AreEqual("-1M", new Number(-1000000).ToString());
        Assert.AreEqual("-10M", new Number(-10000000).ToString());

        Assert.AreEqual("0.567", new Number(0.567f).ToString());
        Assert.AreEqual("1.56", new Number(1.567f).ToString());
        Assert.AreEqual("10.5", new Number(10.567f).ToString());
        Assert.AreEqual("100", new Number(100.567f).ToString());

        Assert.AreEqual("-0.567", new Number(-0.567f).ToString());
        Assert.AreEqual("-1.56", new Number(-1.567f).ToString());
        Assert.AreEqual("-10.5", new Number(-10.567f).ToString());
        Assert.AreEqual("-100", new Number(-100.567f).ToString());
    }
}
