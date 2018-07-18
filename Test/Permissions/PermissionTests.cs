using System.Collections.Generic;
using FluentAssertions;
using TooYoung.Core.Permissions;
using Xunit;

namespace Test.Permissions
{
    public class PermissionTests
    {
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
    }
}
