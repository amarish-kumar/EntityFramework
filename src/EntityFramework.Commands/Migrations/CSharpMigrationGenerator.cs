// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Commands.Utilities;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Operations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Commands.Migrations
{
    public class CSharpMigrationGenerator : MigrationCodeGenerator
    {
        private readonly CSharpHelper _code;
        private readonly CSharpMigrationOperationGenerator _operationGenerator;
        private readonly CSharpModelGenerator _modelGenerator;

        public CSharpMigrationGenerator(
            [NotNull] CSharpHelper code,
            [NotNull] CSharpMigrationOperationGenerator operationGenerator,
            [NotNull] CSharpModelGenerator modelGenerator)
        {
            Check.NotNull(code, nameof(code));
            Check.NotNull(operationGenerator, nameof(operationGenerator));
            Check.NotNull(modelGenerator, nameof(modelGenerator));

            _code = code;
            _operationGenerator = operationGenerator;
            _modelGenerator = modelGenerator;
        }

        public override string Language => ".cs";

        public override string Generate(
            string migrationNamespace,
            string migrationName,
            IReadOnlyList<MigrationOperation> upOperations,
            IReadOnlyList<MigrationOperation> downOperations)
        {
            Check.NotEmpty(migrationNamespace, nameof(migrationNamespace));
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotNull(upOperations, nameof(upOperations));
            Check.NotNull(downOperations, nameof(downOperations));

            var builder = new IndentedStringBuilder();
            var namespaces = new List<string>
            {
                "System",
                "System.Collections.Generic",
                "Microsoft.Data.Entity.Migrations"
            };
            namespaces.AddRange(GetNamespaces(upOperations.Concat(downOperations)));
            foreach (var n in namespaces.Distinct())
            {
                builder
                    .Append("using ")
                    .Append(n)
                    .AppendLine(";");
            }
            builder
                .AppendLine()
                .Append("namespace ").AppendLine(_code.Namespace(migrationNamespace))
                .AppendLine("{");
            using (builder.Indent())
            {
                builder
                    .Append("public partial class ").Append(_code.Identifier(migrationName)).AppendLine(" : Migration")
                    .AppendLine("{");
                using (builder.Indent())
                {
                    builder
                        .AppendLine("protected override void Up(MigrationBuilder migrationBuilder)")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        _operationGenerator.Generate("migrationBuilder", upOperations, builder);
                    }
                    builder
                        .AppendLine("}")
                        .AppendLine()
                        .AppendLine("protected override void Down(MigrationBuilder migrationBuilder)")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        _operationGenerator.Generate("migrationBuilder", downOperations, builder);
                    }
                    builder.AppendLine("}");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");

            return builder.ToString();
        }

        public override string GenerateMetadata(
            string migrationNamespace,
            Type contextType,
            string migrationName,
            string migrationId,
            IModel targetModel)
        {
            Check.NotEmpty(migrationNamespace, nameof(migrationNamespace));
            Check.NotNull(contextType, nameof(contextType));
            Check.NotEmpty(migrationName, nameof(migrationName));
            Check.NotEmpty(migrationId, nameof(migrationId));
            Check.NotNull(targetModel, nameof(targetModel));

            var builder = new IndentedStringBuilder();
            var namespaces = new List<string>
            {
                "System",
                "Microsoft.Data.Entity",
                "Microsoft.Data.Entity.Infrastructure",
                "Microsoft.Data.Entity.Metadata",
                "Microsoft.Data.Entity.Migrations",
                contextType.Namespace
            };
            namespaces.AddRange(GetNamespaces(targetModel));
            foreach (var n in namespaces.Distinct())
            {
                builder
                    .Append("using ")
                    .Append(n)
                    .AppendLine(";");
            }
            builder
                .AppendLine()
                .Append("namespace ").AppendLine(_code.Namespace(migrationNamespace))
                .AppendLine("{");
            using (builder.Indent())
            {
                builder
                    .Append("[DbContext(typeof(").Append(_code.Reference(contextType)).AppendLine("))]")
                    .Append("[Migration(").Append(_code.Literal(migrationId)).AppendLine(")]")
                    .Append("partial class ").AppendLine(_code.Identifier(migrationName))
                    .AppendLine("{");
                using (builder.Indent())
                {
                    builder
                        .AppendLine("protected override void BuildTargetModel(ModelBuilder modelBuilder)")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        // TODO: Optimize. This is repeated below
                        _modelGenerator.Generate("modelBuilder", targetModel, builder);
                    }
                    builder.AppendLine("}");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");

            return builder.ToString();
        }

        public override string GenerateSnapshot(
            string modelSnapshotNamespace,
            Type contextType,
            string modelSnapshotName,
            IModel model)
        {
            Check.NotEmpty(modelSnapshotNamespace, nameof(modelSnapshotNamespace));
            Check.NotNull(contextType, nameof(contextType));
            Check.NotEmpty(modelSnapshotName, nameof(modelSnapshotName));
            Check.NotNull(model, nameof(model));

            var builder = new IndentedStringBuilder();
            var namespaces = new List<string>
            {
                "System",
                "Microsoft.Data.Entity",
                "Microsoft.Data.Entity.Infrastructure",
                "Microsoft.Data.Entity.Metadata",
                "Microsoft.Data.Entity.Migrations",
                contextType.Namespace
            };
            namespaces.AddRange(GetNamespaces(model));
            foreach (var n in namespaces.Distinct())
            {
                builder
                    .Append("using ")
                    .Append(n)
                    .AppendLine(";");
            }
            builder
                .AppendLine()
                .Append("namespace ").AppendLine(_code.Namespace(modelSnapshotNamespace))
                .AppendLine("{");
            using (builder.Indent())
            {
                builder
                    .Append("[DbContext(typeof(").Append(_code.Reference(contextType)).AppendLine("))]")
                    .Append("partial class ").Append(_code.Identifier(modelSnapshotName)).AppendLine(" : ModelSnapshot")
                    .AppendLine("{");
                using (builder.Indent())
                {
                    builder
                        .AppendLine("protected override void BuildModel(ModelBuilder modelBuilder)")
                        .AppendLine("{");
                    using (builder.Indent())
                    {
                        _modelGenerator.Generate("modelBuilder", model, builder);
                    }
                    builder.AppendLine("}");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}
