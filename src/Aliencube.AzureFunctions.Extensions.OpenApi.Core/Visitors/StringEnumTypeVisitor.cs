using System;
using System.Collections.Generic;
using System.Linq;

using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Abstractions;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Attributes;
using Aliencube.AzureFunctions.Extensions.OpenApi.Core.Extensions;

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Aliencube.AzureFunctions.Extensions.OpenApi.Core.Visitors
{
    /// <summary>
    /// This represents the type visitor for <see cref="string"/> type enum.
    /// </summary>
    public class StringEnumTypeVisitor : TypeVisitor
    {
        /// <inheritdoc />
        public StringEnumTypeVisitor(VisitorCollection visitorCollection)
            : base(visitorCollection)
        {
        }

        /// <inheritdoc />
        public override bool IsVisitable(Type type)
        {
            var isVisitable = (this.IsVisitable(type, TypeCode.Int16) || this.IsVisitable(type, TypeCode.Int32) || this.IsVisitable(type, TypeCode.Int64)) &&
                              type.IsUnflaggedEnumType() &&
                              type.HasJsonConverterAttribute<StringEnumConverter>()
                              ;

            return isVisitable;
        }

        /// <inheritdoc />
        public override void Visit(IAcceptor acceptor, KeyValuePair<string, Type> type, NamingStrategy namingStrategy, params Attribute[] attributes)
        {
            var name = type.Key;

            var instance = acceptor as OpenApiSchemaAcceptor;
            if (instance.IsNullOrDefault())
            {
                return;
            }

            // Adds enum values to the schema.
            var enums = type.Value.ToOpenApiStringCollection(namingStrategy);

            var schema = new OpenApiSchema()
            {
                Type = "string",
                Format = null,
                Enum = enums,
                Default = enums.First()
            };

            // Adds the visibility property.
            if (attributes.Any())
            {
                var visibilityAttribute = attributes.OfType<OpenApiSchemaVisibilityAttribute>().SingleOrDefault();
                if (!visibilityAttribute.IsNullOrDefault())
                {
                    var extension = new OpenApiString(visibilityAttribute.Visibility.ToDisplayName());

                    schema.Extensions.Add("x-ms-visibility", extension);
                }
            }

            instance.Schemas.Add(name, schema);
        }

        /// <inheritdoc />
        public override bool IsParameterVisitable(Type type)
        {
            var isVisitable = this.IsVisitable(type);

            return isVisitable;
        }

        /// <inheritdoc />
        public override OpenApiSchema ParameterVisit(Type type, NamingStrategy namingStrategy)
        {
            var schema = this.ParameterVisit(dataType: "string", dataFormat: null);

            // Adds enum values to the schema.
            var enums = type.ToOpenApiStringCollection(namingStrategy);

            schema.Enum = enums;
            schema.Default = enums.First();

            return schema;
        }

        /// <inheritdoc />
        public override bool IsPayloadVisitable(Type type)
        {
            var isVisitable = this.IsVisitable(type);

            return isVisitable;
        }

        /// <inheritdoc />
        public override OpenApiSchema PayloadVisit(Type type, NamingStrategy namingStrategy)
        {
            var schema = this.PayloadVisit(dataType: "string", dataFormat: null);

            // Adds enum values to the schema.
            var enums = type.ToOpenApiStringCollection(namingStrategy);

            schema.Enum = enums;
            schema.Default = enums.First();

            return schema;
        }
    }
}
