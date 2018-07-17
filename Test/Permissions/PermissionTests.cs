using System.Collections.Generic;
using FluentAssertions;
using TooYoung.Core.Permissions;
using Xunit;

namespace Test.Permissions
{
    public class PermissionTests
    {
        public static TheoryData<Permission, Permission, Permission> GetAddRuleData()
        {
            var data = new TheoryData<Permission, Permission, Permission>();
            data.Add(
                new Permission("book", new[] { "read" }, null),
                new Permission("book", new[] { "read" }, new[] { "2333" }),
                new Permission("book", new[] { "delete", "read" }, new[] { "*", "2333" })
            );
            data.Add(
                new Permission("book", new[] { "read" }, null),
                new Permission("music", new[] { "read" }, new[] { "2333" }),
                new Permission
                {
                    Resources = new Dictionary<string, ResourceRule>
                    {
                        ["book"] = new ResourceRule("book", new[] { "read" }, null),
                        ["book"] = new ResourceRule("music", new[] { "read" }, new[] { "2333" }),
                    }
                }
            );
            return data;
        }

        [Theory]
        [MemberData(nameof(GetAddRuleData))]
        public void CombinePermissions(Permission p1, Permission p2, Permission expected, string[] res)
        {
            p1.Combine(p2).SameWith(expected).Should().BeTrue();
        }
    }
}
