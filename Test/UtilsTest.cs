using FluentAssertions;
using TooYoung.Core.Helpers;
using Xunit;

namespace Test
{
    public class UtilsTest
    {
        [Fact]
        public void EnvParamParserTest()
        {
            var connStr = "mongodb://$(MONGO_USER):$(MONGO_PWD)@$(MONGO_HOST):$(MONGO_PORT)";
            var result = connStr.ParseEnvVarParams();
            result.Should().HaveCount(4)
                .And.Contain("MONGO_USER")
                .And.Contain("MONGO_PWD")
                .And.Contain("MONGO_HOST")
                .And.Contain("MONGO_PORT");
        }
    }
}