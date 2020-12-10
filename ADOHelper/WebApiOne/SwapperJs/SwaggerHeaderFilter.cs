using Swashbuckle.Swagger;
using System.Collections.Generic;
using System.Web.Http.Description;

namespace WebApiOne
{
    public class SwaggerHeaderFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
                operation.parameters = new List<Parameter>();

            operation.parameters.Add(new Parameter
            {
                name = "Authorization",
                @in = "header",
                type = "string",
                description = "验证头(token)",
                required = true
            });
        }
        }
}