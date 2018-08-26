using System.Collections.Generic;
using FluentAssertions;
using TooYoung.Core.Permissions;
using Xunit;

namespace Test.Permissions
{
    public class PermissionTests
    {
        public static TheoryData GetSameWithData()
        {
            return new TheoryData<Permission, Permission, bool>
            {
                {
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book")
                        }
                    },
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book")
                        }
                    },
                    true
                },
                {
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book"),
                        }
                    },
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["music"] = new ResourceRule("music")
                        }
                    },
                    false
                },
                {
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book"),
                            ["music"] = new ResourceRule("music"),
                        }
                    },
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["music"] = new ResourceRule("music")
                        }
                    },
                    false
                },
                {
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book"),
                            ["music"] = new ResourceRule("music"),
                        }
                    },
                    null,
                    false
                },
                {
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book"),
                        }
                    },
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["music"] = new ResourceRule("music")
                        }
                    },
                    false
                }
            };
        }

        [Theory]
        [MemberData(nameof(GetSameWithData))]
        void SameWith(Permission p1, Permission p2, bool expected)
        {
            p1.SameWith(p2).Should().Be(expected);
        }

        public static TheoryData GetStringConstructionData()
        {
            return new TheoryData<string, Permission>
            {
                {
                    "book", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book")
                        }
                    }
                },
                {
                    "book:read", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:read")
                        }
                    }
                },
                {
                    "book:read,delete", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:read,delete")
                        }
                    }
                },
                {
                    "book:read:123", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:read:123")
                        }
                    }
                },
                {
                    "book:read:123,233", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:read:123,233")
                        }
                    }
                },
                {
                    "book:read,delete:123,233", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:read,delete:123,233")
                        }
                    }
                },
                {
                    "book:*:123,233", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:*:123,233")
                        }
                    }
                },
                {
                    "book:*:123,233;music", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:*:123,233"),
                            ["music"] = new ResourceRule("music"),
                        }
                    }
                },
                {
                    "book:*:123,233;music:read;book:read", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:*:123,233").Combine(new ResourceRule("book:read")),
                            ["music"] = new ResourceRule("music:read"),
                        }
                    }
                },
                {
                    "book:*:123,233;music:read;book:read;music:delete:2345,1234", new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:*:123,233").Combine(new ResourceRule("book:read")),
                            ["music"] = new ResourceRule("music:read").Combine(new ResourceRule("music:delete:2345,1234")),
                        }
                    }
                },
            };
        }

        [Theory, MemberData(nameof(GetStringConstructionData))]
        void StringConstruction(string str, Permission p)
        {
            new Permission(str).SameWith(p).Should().BeTrue();
        }

        public static TheoryData<Permission, Permission, Permission> GetCombinePermissionData()
        {
            var data = new TheoryData<Permission, Permission, Permission>
            {
                {
                    new Permission("book", new[] {"read"}, null),
                    new Permission("book", new[] {"delete"}, new[] {"2333"}),
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book:read").Combine(new ResourceRule("book:delete:2333"))
                        }
                    }
                },
                {
                    new Permission("book", new[] {"read"}, null),
                    new Permission("music", new[] {"read"}, new[] {"2333"}),
                    new Permission
                    {
                        Resources = new Dictionary<string, ResourceRule>
                        {
                            ["book"] = new ResourceRule("book", new[] {"read"}, null),
                            ["music"] = new ResourceRule("music", new[] {"read"}, new[] {"2333"}),
                        }
                    }
                }
            };
            return data;
        }

        [Theory]
        [MemberData(nameof(GetCombinePermissionData))]
        public void CombinePermissions(Permission p1, Permission p2, Permission expected)
        {
            p1.Combine(p2).SameWith(expected).Should().BeTrue();
        }


        public static TheoryData GetContainsData()
        {
            return new TheoryData<Permission, Permission, bool>
            {
                {
                    new Permission("book"),new Permission("music"),
                    false
                },
                {
                    new Permission("book"),new Permission("music;book"),
                    false
                },
                {
                    new Permission("music;book"),new Permission("book"),
                    true
                },
                {
                    new Permission("book:read"),new Permission("book:read"),
                    true
                },
                {
                    new Permission("book:read,delete"),new Permission("book:read"),
                    true
                },
                {
                    new Permission("book:read"),new Permission("book:read,delete"),
                    false
                },
                {
                    new Permission("book;music"),new Permission("book:read,delete;music:delete"),
                    true
                },
                {
                    new Permission("book;music:upload"),new Permission("book:read,delete;music:delete"),
                    false
                },
            };
        }

        [Theory]
        [MemberData(nameof(GetContainsData))]
        void Contains(Permission p1, Permission p2, bool expected)
        {
            p1.Contains(p2).Should().Be(expected);
        }
    }
}
