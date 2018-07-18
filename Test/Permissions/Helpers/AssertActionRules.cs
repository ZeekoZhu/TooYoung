using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using TooYoung.Core.Permissions;

namespace Test.Permissions.Helpers
{
    internal static class AssertActionRules
    {
        public static void ShouldBeSameWith(this ResourceRule rule, params string[] expectedRules)
        {
            var expected =
                expectedRules
                    .Select(r => new ResourceRule(r))
                    .Aggregate((r1, r2) => r1.Combine(r2));
            rule.SameWith(expected).Should().BeTrue();
        }
    }
}
