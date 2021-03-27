using System;
using Xunit;

namespace PaymentGateway.Domain.Tests
{
    public class SensitiveTests
    {
        [Theory]
        [InlineData("example")]
        [InlineData(213)]
        public void When_a_Sensitive_is_constructed_with_a_value_then_the_string_representation_of_the_Sensitive_does_not_contain_that_of_the_input_type(dynamic input)
        {
            var sensitive = new Sensitive<dynamic>(input);

            Assert.DoesNotContain(input.ToString(), sensitive.ToString());
            Assert.Equal("*****", sensitive.ToString());
        }

        [Theory]
        [InlineData("example")]
        [InlineData(213)]
        public void When_a_Sensitive_is_constructed_with_a_value_then_the_value_can_be_retrieved(dynamic input)
        {
            var sensitive = new Sensitive<dynamic>(input);

            Assert.Equal(input, sensitive.Value);
        }

    }
}
