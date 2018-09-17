using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TooYoung.Core.Models;

namespace TooYoung.Web.Json
{
    public class PropertyIgnoreContractResolver : CamelCasePropertyNamesContractResolver
    {
        public static readonly PropertyIgnoreContractResolver Instance = new PropertyIgnoreContractResolver();

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

//            property.Ignore((ImageInfo i) => i.Image);

            return property;
        }
    }

    internal static class Helper
    {
        public static JsonProperty Ignore<TObject, TProp>(this JsonProperty property, Expression<Func<TObject, TProp>> selector)
        {
            var objectType = selector.Parameters.First().Type;
            if (selector.Body.NodeType == ExpressionType.MemberAccess)
            {
                var propExp = selector.Body as MemberExpression;
                Debug.Assert(propExp != null, nameof(propExp) + " != null");
                var propName = propExp.Member.Name;
                if (property.DeclaringType == objectType && property.UnderlyingName == propName)
                {
                    property.ShouldSerialize = _ => false;
                }
            }

            return property;
        }
    }
}
