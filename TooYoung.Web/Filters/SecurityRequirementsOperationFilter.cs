using System.Collections.Generic;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using ZeekoUtilsPack.AspNetCore.Jwt;

namespace TooYoung.Web.Filters
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        class HeaderParameter : NonBodyParameter
        {
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<IParameter>();
            var attrs = context.ApiDescription.ActionAttributes().Distinct();
            foreach (var attr in attrs)
            {
                if (attr.GetType() == typeof(BearerAuthorizeAttribute))
                {
                    operation.Parameters.Add(new HeaderParameter
                    {
                        Name = "Authorization",
                        In = "header",
                        Type = "string",
                        Required = false
                    });
                    operation.Responses.Add("401", new Response { Description = "Unauthorized" });
                    operation.Responses.Add("403", new Response { Description = "Forbidden" });
                }
            }
        }
    }
}
