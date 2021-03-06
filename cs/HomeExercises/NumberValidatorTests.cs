﻿using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    [TestFixture]
    public class NumberValidatorTests
    {
        [TestCase(-1, 2, true, TestName = "On negative precision")]
        [TestCase(2, 4, true, TestName = "On scale >= precision")]
        [TestCase(2, -1, true, TestName = "On negative scale")]
        public void ThrowsException(int precision, int scale, bool onlyPositive)
        {
            Assert.Throws<ArgumentException>(() => new NumberValidator(precision, scale, onlyPositive));
        }

        [TestCase(1, 0, true, TestName = "On small correct precision and scale")]
        [TestCase(int.MaxValue, int.MaxValue - 1, true, TestName = "On big correct precision and scale")]

        public void DoesNotThrowException(int precision, int scale, bool onlyPositive)
        {
            Assert.DoesNotThrow(() => new NumberValidator(precision, scale, onlyPositive));
        }

        [TestCase(17, 2, true, "0.0", TestName = "on fractional zero")]
        [TestCase(17, 2, true, "0", TestName = "on integer zero")]
        [TestCase(4, 2, true, "+1.23", TestName = "on positive fractional")]
        [TestCase(4, 2, false, "-4.56", TestName = "on negative fractional")]
        [TestCase(4, 2, true, "+1,23", TestName = "on positive fractional separated with comma")]
        [TestCase(4, 2, false, "-4,56", TestName = "on negative fractional separated with comma")]

        public void IsValidNumberReturnsTrueOnCorrectData(int precision, int scale, bool onlyPositive, string value)
        {
            var validator = new NumberValidator(precision, scale, onlyPositive);
            validator.IsValidNumber(value).Should().Be(true);
        }


        [TestCase(3, 2, true, "a.sd", TestName = "on non-digits")]
        [TestCase(3, 2, true, null, TestName = "on null")]
        [TestCase(3, 2, true, "", TestName = "on empty string")]
        [TestCase(10, 2, true, "-5.2", TestName = "on negative fractional when onlyPositive=true")]
        [TestCase(10, 2, true, "-5", TestName = "on negative integer when onlyPositive=true")]
        [TestCase(17, 2, true, "0.000", TestName = "on fractional with incorrect scale")]
        [TestCase(3, 2, true, "00.00", TestName = "on unsigned zero fractional with incorrect precision")]
        [TestCase(3, 2, true, "-0.00", TestName = "on negative zero fractional with incorrect precision")]
        [TestCase(3, 2, true, "+0.00", TestName = "on positive zero fractional with incorrect precision")]
        [TestCase(3, 2, true, "+1.23", TestName = "on positive fractional with incorrect precision")]
        [TestCase(3, 2, true, "-1.23", TestName = "on negative fractional with incorrect precision")]
        [TestCase(3, 2, true, ".", TestName = "on single dot")]
        [TestCase(3, 2, true, ",", TestName = "on single comma")]
        [TestCase(17, 5, true, "1.2.0", TestName = "on two dots")]
        public void IsValidNumberReturnsFalseOnIncorrectData(int precision, int scale, bool onlyPositive, string value)
        {
            var validator = new NumberValidator(precision, scale, onlyPositive);
            validator.IsValidNumber(value).Should().Be(false);
        }

    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("precision must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}