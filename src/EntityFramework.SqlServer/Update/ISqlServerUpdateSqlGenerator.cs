// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.SqlServer;

namespace Microsoft.Data.Entity.Update
{
    public interface ISqlServerUpdateSqlGenerator : IUpdateSqlGenerator
    {
        SqlServerUpdateSqlGenerator.ResultsGrouping AppendBulkInsertOperation(
            [NotNull] StringBuilder commandStringBuilder,
            [NotNull] IReadOnlyList<ModificationCommand> modificationCommands);

        string GenerateLiteral(Guid literal);
    }
}
