using System;
using System.Collections.Generic;
using System.Linq;

namespace TooYoung.Core.Permissions
{
    /// <summary>
    /// 资源访问控制规则
    /// </summary>
    public class ResourceRule
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 对资源的操作
        /// </summary>
        public HashSet<string> Actions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public HashSet<string> Instances { get; set; }

        public ResourceRule(string resource, ICollection<string> actions, ICollection<string> instances)
        {
            Name = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        public ResourceRule(string literalRule)
        {
            Actions = new HashSet<string>();
            Instances = new HashSet<string>();
            var levels = literalRule.Split(':');
            if (levels.Length < 1)
            {
                throw new ArgumentException($"Invalid permission statement: {literalRule}", nameof(literalRule));
            }

            Name = levels[0];
            if (levels.Length == 1)
            {
                Actions.Add("*");
                Instances.Add("*");
            }

            if (levels.Length >= 2)
            {
                var actions = levels[1].Split(',');
                foreach (var action in actions)
                {
                    Actions.Add(action);
                }
            }

            if (levels.Length == 2)
            {
                Instances.Add("*");
            }

            if (levels.Length == 3)
            {
                var instances = levels[2].Split(',');
                foreach (var instance in instances)
                {
                    Instances.Add(instance);
                }
            }

            if (levels.Length > 3)
            {
                throw new ArgumentException($"Invalid permission statement: {literalRule}", nameof(literalRule));
            }
        }

        /// <summary>
        /// 合并两个规则
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">合并的规则必须作用于同一种资源类型</exception>
        public ResourceRule Combine(ResourceRule other)
        {
            if (other.Name != Name)
            {
                throw new InvalidOperationException($"Can not combine resource permission on different resources: {Name}, {other.Name}");
            }
            var actions = new HashSet<string>(Actions);
            actions.UnionWith(other.Actions);
            var instances = new HashSet<string>(Instances);
            instances.UnionWith(other.Instances);
            return new ResourceRule(Name, actions, instances);
        }

        public bool Contains(ResourceRule other)
        {
            if (other.Name != Name)
            {
                return false;
            }

            do
            {
                if (Actions.Contains("*") == false && Actions.IsSupersetOf(other.Actions) == false)
                {
                    break;
                }

                if (Instances.Contains("*") == false && Instances.IsSupersetOf(other.Instances) == false)
                {
                    break;
                }

                return true;
            } while (false);

            return false;
        }


    }

    /// <summary>
    /// 资源权限集合
    /// </summary>
    public class Permission
    {
        private Dictionary<string, ResourceRule> Resources { get; set; }
        public Permission(string resource, ICollection<string> actions, ICollection<string> instances)
        {
            var resourceAcl = new ResourceRule(resource, actions, instances);
            Resources = new Dictionary<string, ResourceRule>
            {
                [resource] = resourceAcl
            };
        }

        private Permission(Dictionary<string, ResourceRule> resources)
        {
            Resources = resources;
        }

        /// <summary>
        /// 合并两个权限并产生新的权限
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public Permission Combine(Permission other)
        {
            var result = new Permission(Resources);
            foreach (var resource in other.Resources)
            {
                result.AddRule(resource.Value);
            }

            return result;
        }

        /// <summary>
        /// 添加一个新的资源访问规则
        /// </summary>
        /// <param name="rule"></param>
        private void AddRule(ResourceRule rule)
        {
            if (Resources.TryGetValue(rule.Name, out var value))
            {
                var newRule = value.Combine(rule);
                Resources[rule.Name] = newRule;
            }
            else
            {
                Resources.Add(rule.Name, rule);
            }
        }

        public Permission(string literalPermission)
        {
            // split by ;
            var rules = literalPermission.Split(';').Select(r => new ResourceRule("r"));
            foreach (var rule in rules)
            {
                AddRule(rule);
            }
        }

        public bool Contains(Permission other)
        {
            var resourceExist = new HashSet<string>(Resources.Keys);
            var resourcePermitted = resourceExist.IsSupersetOf(other.Resources.Keys);
            if (resourcePermitted == false)
            {
                return false;
            }

            foreach (var ruleKey in other.Resources.Keys)
            {
                var ruleExist = Resources[ruleKey];
                var ruleRequired = other.Resources[ruleKey];
                if (ruleExist.Contains(ruleRequired) == false)
                {
                    return false;
                }
            }

            return true;
        }

    }
}
