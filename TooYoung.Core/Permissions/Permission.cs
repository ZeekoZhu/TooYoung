using System;
using System.Collections.Generic;
using System.Linq;
#if DEBUG
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Test")]
#endif
namespace TooYoung.Core.Permissions
{
    /// <summary>
    /// 操作访问控制规则
    /// </summary>
    internal class ActionRule
    {
        public string Action { get; set; }
        public HashSet<string> Instances { get; set; }

        public ActionRule()
        {

        }

        public ActionRule(string action, ICollection<string> instances)
        {
            Action = action ?? "*";
            instances = instances?.Where(s => string.IsNullOrWhiteSpace(s) == false).ToList();
            if (instances != null && instances.Any())
            {
                Instances = new HashSet<string>(instances);
            }
            else
            {
                Instances = new HashSet<string>(new[] { "*" });
            }
        }

        public override string ToString()
        {
            var instances = string.Join(",", Instances);
            return $"{Action}:{instances}";
        }

        public bool SameWith(ActionRule other)
        {
            return Action == other.Action && Instances.SetEquals(other.Instances);
        }

        public bool Contains(ActionRule other)
        {
            if (Action != "*" && Action != other.Action)
            {
                return false;
            }

            return ContainsInstances(other.Instances);
        }


        private bool ContainsInstances(ICollection<string> instances)
        {
            return Instances.Contains("*") || Instances.IsSupersetOf(instances);
        }
    }
    /// <summary>
    /// 资源访问控制规则
    /// </summary>
    internal class ResourceRule
    {
        /// <summary>
        /// 资源名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 对资源的操作
        /// </summary>
        public Dictionary<string, ActionRule> Actions { get; set; }

        public ResourceRule(string resource, ICollection<string> actions, ICollection<string> instances)
        {
            Setup(resource, actions, instances);
        }

        /// <summary>
        /// Copy construction
        /// </summary>
        /// <param name="other"></param>
        public ResourceRule(ResourceRule other)
        {
            Name = other.Name;
            Actions = new Dictionary<string, ActionRule>();
            foreach (var action in other.Actions.Values)
            {
                Actions.Add(action.Action, new ActionRule(action.Action, action.Instances));
            }
        }

        public ResourceRule()
        {
        }

        public ResourceRule(string literalRule)
        {
            var levels = literalRule.Split(':');
            if (levels.Length < 1 || levels.Length > 3)
            {
                throw new ArgumentException($"Invalid permission statement: {literalRule}", nameof(literalRule));
            }

            var actions = levels.Length > 1 ? levels[1].Split(',') : null;
            var instances = levels.Length == 3 ? levels[2]?.Split(',') : null;
            Setup(levels[0], actions, instances);
        }

        public override string ToString()
        {
            var actions = Actions.Values.Select(a => $"{Name}:{a}");
            return string.Join(";", actions);
        }

        private void Setup(string resource, ICollection<string> actions, ICollection<string> instances)
        {
            Name = resource ?? throw new ArgumentNullException(nameof(resource));
            actions = actions?.Where(s => string.IsNullOrWhiteSpace(s) == false).Distinct().ToList();
            if (actions != null && actions.Any())
            {
                var actionRules = actions.Select(a => new ActionRule(a, instances));
                Actions = new Dictionary<string, ActionRule>();
                foreach (var rule in actionRules)
                {
                    Actions.Add(rule.Action, rule);
                }
            }
            else
            {
                Actions = new Dictionary<string, ActionRule>
                {
                    ["*"] = new ActionRule(null, instances)
                };
            }
        }



        public bool SameWith(ResourceRule other)
        {
            if (other == null
                || other.Name != Name
                || other.Actions.Count != Actions.Count)
            {
                return false;
            }

            foreach (var actionPair in Actions)
            {
                if (other.Actions.TryGetValue(actionPair.Key, out var rule) == false)
                {
                    return false;
                }

                if (rule.SameWith(actionPair.Value) == false)
                {
                    return false;
                }

            }
            return true;
        }

        public void AddRule(ActionRule rule)
        {
            if (Actions.TryGetValue(rule.Action, out var existRule))
            {
                foreach (var instance in rule.Instances)
                {
                    existRule.Instances.Add(instance);
                }
            }
            else
            {
                var newRule = new ActionRule(rule.Action, rule.Instances);
                Actions[rule.Action] = newRule;
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
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other.Name != Name)
            {
                throw new InvalidOperationException($"Can not combine resource permission on different resources: {Name}, {other.Name}");
            }

            var result = new ResourceRule(other);
            foreach (var action in Actions.Values)
            {
                result.AddRule(action);
            }

            return result;
        }

        /// <summary>
        /// 判断是否包含指定权限
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Contains(ResourceRule other)
        {
            if (other == null || other.Name != Name)
            {
                return false;
            }

            // 查找通配符
            var hasWildCard = Actions.TryGetValue("*", out var wildCard);

            foreach (var otherActionRule in other.Actions.Values)
            {
                // 检查通配符是否匹配
                var matched = hasWildCard && wildCard.Contains(otherActionRule);
                // 检查对应操作规则是否匹配
                if (matched == false
                    && (Actions.TryGetValue(otherActionRule.Action, out var existsRule) == false
                        || existsRule.Contains(otherActionRule) == false))
                {
                    return false;
                }
            }

            return true;
        }


    }

    /// <summary>
    /// 资源权限集合
    /// </summary>
    public class Permission
    {
        internal Dictionary<string, ResourceRule> Resources { get; set; }
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

        internal Permission()
        {
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

        public override string ToString()
        {
            var resources = Resources.Values.Select(r => r.ToString());
            return string.Join(";", resources);
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

        public bool SameWith(Permission other)
        {
            if (other == null)
            {
                return false;
            }

            if (Resources.Keys.Count != other.Resources.Count)
            {
                return false;
            }

            foreach (var pair in Resources)
            {
                var r1 = pair.Value;
                if (other.Resources.TryGetValue(pair.Key, out var r2) == false)
                {
                    return false;
                }

                if (r1.SameWith(r2) == false)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
