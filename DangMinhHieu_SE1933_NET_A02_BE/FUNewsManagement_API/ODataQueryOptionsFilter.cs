using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FUNewsManagement_API;

public class ODataQueryOptionsFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasEnableQuery = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<EnableQueryAttribute>()
            .Any();

        if (hasEnableQuery)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            var odataParams = new List<OpenApiParameter>
            {
                new OpenApiParameter
                {
                    Name = "$filter",
                    In = ParameterLocation.Query,
                    Description = "Filter the results using OData syntax. Example: CategoryID eq 1 or NewsStatus eq true",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string" }
                },
                new OpenApiParameter
                {
                    Name = "$select",
                    In = ParameterLocation.Query,
                    Description = "Select specific properties to return. Example: NewsArticleID,NewsTitle,CreatedDate",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string" }
                },
                new OpenApiParameter
                {
                    Name = "$orderby",
                    In = ParameterLocation.Query,
                    Description = "Order the results. Example: CreatedDate desc",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string" }
                },
                new OpenApiParameter
                {
                    Name = "$expand",
                    In = ParameterLocation.Query,
                    Description = "Expand related entities. Example: Category,CreatedBy,Tags",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "string" }
                },
                new OpenApiParameter
                {
                    Name = "$top",
                    In = ParameterLocation.Query,
                    Description = "Show only the first n items. Example: 10",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "integer" }
                },
                new OpenApiParameter
                {
                    Name = "$skip",
                    In = ParameterLocation.Query,
                    Description = "Skip the first n items. Example: 0",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "integer" }
                },
                new OpenApiParameter
                {
                    Name = "$count",
                    In = ParameterLocation.Query,
                    Description = "Include the total count of items matching the query. Example: true",
                    Required = false,
                    Schema = new OpenApiSchema { Type = "boolean" }
                }
            };

            foreach (var p in odataParams)
            {
                if (!operation.Parameters.Any(existing => existing.Name == p.Name))
                {
                    operation.Parameters.Add(p);
                }
            }
        }
    }
}
