using System;
using Xunit;

namespace PaymentGateway.Domain.Tests
{
    public class CardTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("111122223333444")] // Too short
        [InlineData("12412412412412412412424")] // Too long
        [InlineData("ABCDABCDABCDABCD")] // All letters
        [InlineData("1234ABCDABCDABCD")] // Some letters
        public void When_CardPan_constructor_called_with_invalid_value_then_an_ArgumentException_is_thrown(string input)
        {
            var exception = Assert.Throws<ArgumentException>(() => new Pan(input));
            Assert.Equal("Card PAN must be exactly 16 characters. Only the characters 0-9 are allowed.", exception.Message);
        }

        [Theory]
        [InlineData("1111222233334444")]
        [InlineData("1234567891234567")]
        [InlineData("5544123498762231")]
        public void When_CardPan_constructor_called_with_16_digit_value_then_pan_is_created(string input)
        {
            var pan = new Pan(input);

            Assert.Equal(input, pan.Value.Value);
        }

        [Fact]
        public void CardPan_equality_can_be_checked()
        {
            var input1 = "1111222233334444";
            var input2 = "5555666677778888";

            var pan1a = new Pan(input1);
            var pan1b = new Pan(input1);
            var pan2 = new Pan(input2);

            Assert.Equal(pan1a, pan1b);
            Assert.NotEqual(pan1a, pan2);
            Assert.NotEqual(pan1b, pan2);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("ABCDAB")] // All letters
        [InlineData("12CD")] // Some letters
        public void When_CardCvv_constructor_called_with_invalid_value_then_an_ArgumentException_is_thrown(string input)
        {
            var exception = Assert.Throws<ArgumentException>(() => new Cvv(input));
            Assert.Equal("Card CVV must be non-empty. Only the characters 0-9 are allowed.", exception.Message);
        }

        [Theory]
        [InlineData("123")]
        [InlineData("1234")]
        public void When_CardCvv_constructor_called_with_non_empty_value_then_pan_is_created(string input)
        {
            var cvv = new Cvv(input);

            Assert.Equal(input, cvv.Value.Value);
        }

        [Fact]
        public void CardCvv_equality_can_be_checked()
        {
            var input1 = "123";
            var input2 = "5678";

            var cvv1a = new Cvv(input1);
            var cvv1b = new Cvv(input1);
            var cvv2 = new Cvv(input2);

            Assert.Equal(cvv1a, cvv1b);
            Assert.NotEqual(cvv1a, cvv2);
            Assert.NotEqual(cvv1b, cvv2);
        }
    }
}

