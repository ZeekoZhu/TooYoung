using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Test.Permissions.Helpers;
using TooYoung.Core.Permissions;
using Xunit;
using Xunit.Abstractions;

namespace Test.Permissions
{
    public class ResourceRuleTests
    {
        public static TheoryData GetStringConstructionData()
        {
            return new TheoryData<string, ResourceRule>
            {
                {
                    "book", new ResourceRule
                    {
                        Actions = new Dictionary<string, ActionRule>
                        {
                            ["*"] = new ActionRule(null, null)
                        },
                        Name = "book"
                    }
                },
                {
                    "book:read", new ResourceRule
                    {
                        Actions = new Dictionary<string, ActionRule>
                        {
                            ["read"] = new ActionRule("read", null)
                        },
                        Name = "book"
                    }
                },
                {
                    "book:read,delete", new ResourceRule
                    {
                        Actions = new Dictionary<string, ActionRule>
                        {
                            ["read"] = new ActionRule("read", null),
                            ["delete"] = new ActionRule("delete", null)
                        },
                        Name = "book"
                    }
                },
                {
                    "book:read:123", new ResourceRule
                    {
                        Actions = new Dictionary<string, ActionRule>
                        {
                            ["read"] = new ActionRule("read", new []{"123"})
                        },
                        Name = "book"
                    }
                },
                {
                    "book:read:123,233", new ResourceRule
                    {
                        Actions = new Dictionary<string, ActionRule>
                        {
                            ["read"] = new ActionRule("read", new []{"123","233"})
                        },
                        Name = "book"
                    }
                },
                {
                    "book:read,delete:123,233", new ResourceRule
                    {
                        Actions = new Dictionary<string, ActionRule>
                        {
                            ["read"] = new ActionRule("read", new []{"123","233"}),
                            ["delete"] = new ActionRule("delete", new []{"123","233"})
                        },
                        Name = "book"
                    }
                },
            };
        }

        [Theory]
        [MemberData(nameof(GetStringConstructionData))]
        void StringConstruction(string str, ResourceRule expected)
        {
            var result = new ResourceRule(str);
            result.SameWith(expected).Should().BeTrue();
        }

        [Fact]
        public void CreateOnlyResourceName()
        {
            var rule = new ResourceRule("foo");

            rule.ShouldBeSameWith("foo");
        }

        public static TheoryData GetCreateWithParamsData()
        {
            return new TheoryData<string[], string[], string[]>
            {
                { new[] { "download", "" }, new[] { "123", "" }, new[] { "music:download:123" } },
                { new[] { "download" }, new[] { "123" }, new[] { "music:download:123" } },
                { new[] { "download", "upload" }, new[] { "123", "21312" }, new[] { "music:download,upload:123,21312" } },
                { new[] { "download", "play", null }, new[] { "123", null }, new[] { "music:download,play:123" } },
                { new[] { "download", "play", null }, null, new[] { "music:play,download" } },
                { null, new[] { "123" }, new[] { "music:*:123" } }
            };
        }

        [Theory]
        [MemberData(nameof(GetCreateWithParamsData))]
        public void CreateWithParams(ICollection<string> actions, ICollection<string> instances, string[] expected)
        {
            var rule = new ResourceRule("music", actions, instances);
            rule.ShouldBeSameWith(expected);
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
                new [] {"book:*,read"}
            };
            yield return new object[]
            {
                new ResourceRule("book", null, null),
                new ResourceRule("book", null, null),
                new [] {"book"}
            };
            yield return new object[]
            {
                new ResourceRule("book", new[] {"read"}, null),
                new ResourceRule("book", new[] {"read"}, null),
                new[] {"book:read"}
            };
            yield return new object[]
            {
                new ResourceRule("book", new[] {"delete"}, null),
                new ResourceRule("book", new[] {"read"}, null),
                new[] {"book:read,delete"}
            };
            yield return new object[]
            {
                new ResourceRule("book", new[] {"delete"}, new[] {"123123"}),
                new ResourceRule("book", new[] {"read"}, new[] {"23333"}),
                new[] {"book:delete:123123", "book:read:23333"}
            };
            yield return new object[]
            {
                new ResourceRule("book", new[] {"delete"}, new[] {"23333"}),
                new ResourceRule("book", new[] {"read"}, new[] {"23333"}),
                new[] {"book:read,delete:23333"}
            };
        }

        [Theory]
        [MemberData(nameof(GetCombineRulesData))]
        void CombineRules(ResourceRule fooRule, ResourceRule barRule, string[] combinedActions)
        {
            var combined = fooRule.Combine(barRule);

            combined.ShouldBeSameWith(combinedActions);
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
                new ResourceRule("book", new[] {"read"}, new[] {"2333", "1234"}),
                new ResourceRule("book", new[] {"read", "delete"}, new[] {"2333"}),
                false
            };
            yield return new object[]
            {
                new ResourceRule("book:read").Combine(new ResourceRule("book:delete:2333")),
                new ResourceRule("book:read:2333"),
                true
            };
            yield return new object[]
            {
                new ResourceRule("book:read").Combine(new ResourceRule("book:delete:2333")),
                new ResourceRule("book:read:2333").Combine(new ResourceRule("book:delete")),
                false
            };
            yield return new object[]
            {
                new ResourceRule("book:read").Combine(new ResourceRule("book:delete:2333")),
                new ResourceRule("book:read:2333").Combine(new ResourceRule("book:delete:2333")),
                true
            };
            yield return new object[]
            {
                new ResourceRule("book:read").Combine(new ResourceRule("book:delete,read")),
                new ResourceRule("book:read:2333").Combine(new ResourceRule("book:delete:2341,2333")),
                true
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
