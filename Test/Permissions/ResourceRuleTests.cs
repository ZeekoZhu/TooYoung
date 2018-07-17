using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using TooYoung.Core.Permissions;
using Xunit;
using Xunit.Abstractions;

namespace Test.Permissions
{
    public class ResourceRuleTests
    {
        [Fact]
        public void CreateOnlyResourceName()
        {
            var rule = new ResourceRule("foo");

            rule.Actions.Should().OnlyContain(s => s == "*");
            rule.Instances.Should().OnlyContain(s => s == "*");
        }

        public static IEnumerable<object[]> GetCreateWithParamsData()
        {
            yield return new[] { new[] { "download", "" }, new[] { "123", "" } };
            yield return new[] { new[] { "download" }, new[] { "123" } };
            yield return new[] { new[] { "download", "upload" }, new[] { "123", "21312" } };
            yield return new[] { new[] { "download", "play", null }, new[] { "123", null } };
            yield return new[] { new[] { "download", "play", null }, null };
            yield return new[] { null, new[] { "123" } };
        }

        [Theory]
        [MemberData(nameof(GetCreateWithParamsData))]
        public void CreateWithParams(ICollection<string> actions, ICollection<string> instances)
        {
            var rule = new ResourceRule("music", actions, instances);
            bool Predicate(string s) => string.IsNullOrWhiteSpace(s) == false;
            var expectedActions = actions?.Where(Predicate).Distinct().ToList();
            if (expectedActions != null && expectedActions.Any())
            {
                rule.Actions.Should().BeEquivalentTo(expectedActions);
            }
            else
            {
                rule.Actions.Should().OnlyContain(s => s == "*");
            }
            var expectedInstances = instances?.Where(Predicate).Distinct().ToList();
            if (expectedInstances != null && expectedInstances.Any())
            {
                rule.Instances.Should().BeEquivalentTo(expectedInstances);
            }
            else
            {
                rule.Instances.Should().OnlyContain(s => s == "*");
            }
        }

        [Fact]
        public void CombineWithDifferentResource()
        {
            var rule = new ResourceRule("book", null, null);
            var otherRule = new ResourceRule("music", null, null);
            Assert.Throws<InvalidOperationException>(() =>
            {
                rule.Combine(otherRule);
            });
        }

        public class CombineRulesData : IXunitSerializable
        {
            public void Deserialize(IXunitSerializationInfo info)
            {
                throw new NotImplementedException();
            }

            public void Serialize(IXunitSerializationInfo info)
            {
                throw new NotImplementedException();
            }
        }
        public static IEnumerable<object[]> GetCombineRulesData()
        {
            yield return new object[]
            {
                new ResourceRule("book", null, null),
                new ResourceRule("book", new[] {"read"}, null),
                new[] {"*", "read"},
                new[] {"*"}
            };
            yield return new object[]
            {
                new ResourceRule("book", null, null),
                new ResourceRule("book", null, null),
                new[] {"*"},
                new[] {"*"}
            };
            yield return new object[]
            {
                new ResourceRule("book", new[] {"read"}, null),
                new ResourceRule("book", new[] {"read"}, null),
                new[] {"read"},
                new[] {"*"}
            };
            yield return new object[]
            {
                new ResourceRule("book", new[] {"delete"}, null),
                new ResourceRule("book", new[] {"read"}, null),
                new[] {"read", "delete"},
                new[] {"*"}
            };
            yield return new object[]
            {
                new ResourceRule("book", new[] {"delete"}, new[] {"123123"}),
                new ResourceRule("book", new[] {"read"}, new[] {"23333"}),
                new[] {"read", "delete"},
                new[] {"123123", "23333"}
            };
            yield return new object[]
            {
                new ResourceRule("book", new[] {"delete"}, new[] {"23333"}),
                new ResourceRule("book", new[] {"read"}, new[] {"23333"}),
                new[] {"read", "delete"},
                new[] {"23333"}
            };
        }

        [Theory]
        [MemberData(nameof(GetCombineRulesData))]
        void CombineRules(ResourceRule fooRule, ResourceRule barRule, string[] combinedActions, string[] combinedInstances)
        {
            var combined = fooRule.Combine(barRule);

            combined.Actions.Should().BeEquivalentTo(combinedActions);
            combined.Instances.Should().BeEquivalentTo(combinedInstances);
        }

        public static IEnumerable<object[]> GetContainsData()
        {
            yield return new object[]
            {
                new ResourceRule("book",null,null),
                new ResourceRule("music", null, null),
                false
            };
            yield return new object[]
            {
                new ResourceRule("book",null,null),
                new ResourceRule("book", null, null),
                true
            };
            yield return new object[]
            {
                new ResourceRule("book",null,null),
                new ResourceRule("book", new []{"read"}, new []{"9527"}),
                true
            };
            yield return new object[]
            {
                new ResourceRule("book",null,null),
                new ResourceRule("book", new []{"read","delete"}, null),
                true
            };
            yield return new object[]
            {
                new ResourceRule("book", new []{"read","delete"}, null),
                new ResourceRule("book",null,null),
                false
            };
            yield return new object[]
            {
                new ResourceRule("book",null, new[] {"2333"}),
                new ResourceRule("book", new []{"read","delete"}, null),
                false
            };
            yield return new object[]
            {
                new ResourceRule("book",null, new[] {"2333", "1234"}),
                new ResourceRule("book", new []{"read","delete"}, new []{"2333"}),
                true
            };
            yield return new object[]
            {
                new ResourceRule("book",new []{ "read", "delete"}, new[] {"2333", "1234"}),
                new ResourceRule("book", new []{"read","delete"}, new []{"2333"}),
                true
            };
            yield return new object[]
            {
                new ResourceRule("book",new []{ "read"}, new[] {"2333", "1234"}),
                new ResourceRule("book", new []{"read","delete"}, new []{"2333"}),
                false
            };
        }

        public static TheoryData GetSameWithData()
        {
            return new TheoryData<ResourceRule, ResourceRule, bool>
            {
                {new ResourceRule("book", null, null), new ResourceRule("book", null, null), true},
                {new ResourceRule("book", new [] {"read"}, null), new ResourceRule("book", null, null), false},
                {new ResourceRule("book", new [] {"read"}, null), new ResourceRule("book", new []{"read"}, null), true},
                {new ResourceRule("book", new [] {"read", "delete"}, null), new ResourceRule("book", new []{"read"}, null), false},
                {new ResourceRule("book", new [] {"read", "delete"}, null), new ResourceRule("book", new []{"delete","read"}, null), true},
                {new ResourceRule("book", null, null), new ResourceRule("music", null, null), false},
                {new ResourceRule("book", new []{"read"}, new []{"2333"}), new ResourceRule("book", null, new []{"2333"}), false},
                {new ResourceRule("book", new []{"read"}, new []{"2333", "1234"}), new ResourceRule("book", new []{"read"}, new []{"2333", "1234"}), true},
                {new ResourceRule("book", new []{"read"}, new []{"2333", "1234"}), new ResourceRule("book", new []{"read"}, new []{"1234", "2333"}), true},
            };
        }

        [Theory]
        [MemberData(nameof(GetSameWithData))]
        void SameWith(ResourceRule r1, ResourceRule r2, bool expected)
        {
            r1.SameWith(r2).Should().Be(expected);
        }

        [Theory]
        [MemberData(nameof(GetContainsData))]
        void Contains(ResourceRule fooRule, ResourceRule barRule, bool expected)
        {
            fooRule.Contains(barRule).Should().Be(expected);
        }

        public static TheoryData GetCreateFromStringData()
        {
            var data = new TheoryData<string, ResourceRule>
            {
                { "book", new ResourceRule("book", null, null) },
                { "book:*", new ResourceRule("book", null, null) },
                { "book:*:*", new ResourceRule("book", null, null) },
                { "book:read", new ResourceRule("book", new[] { "read" }, null) },
                { "book:read:*", new ResourceRule("book", new[] { "read" }, null) },
                { "book:read,delete", new ResourceRule("book", new[] { "read", "delete" }, null) },
                { "book:read,delete:*", new ResourceRule("book", new[] { "read", "delete" }, null) },
                { "book:*:2333", new ResourceRule("book", null, new[] { "2333" }) },
                { "book:*:2333,1234", new ResourceRule("book", null, new[] { "2333", "1234" }) }
            };

            return data;
        }

        [Theory]
        [MemberData(nameof(GetCreateFromStringData))]
        void CreateFromString(string str, ResourceRule expected)
        {
            var result = new ResourceRule(str);
            result.SameWith(expected).Should().BeTrue();
        }
    }
}
