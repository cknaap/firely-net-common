﻿/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://github.com/FirelyTeam/fhir-net-api/blob/master/LICENSE
 */

#nullable enable

using Hl7.Fhir.Utility;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hl7.Fhir.Specification
{
    public class UnionTypeDefinition : TypeDefinition
    {
        public UnionTypeDefinition(TypeDefinition? @base, IReadOnlyCollection<TypeDefinition> members,
            AnnotationList? annotations,
            IDictionary<TypeDefinition, MethodInfo>? casts) : base(@base, annotations,
                isAbstract: false, isOrdered: false, casts)
        {
            MemberTypes = members ?? throw new ArgumentNullException(nameof(members));
        }

        public UnionTypeDefinition(string name, NamedTypeDefinition? @base, IReadOnlyCollection<NamedTypeDefinition> members)
            : this(@base, members, annotations: null, casts: null)
        {
            // nothing
        }

        public IReadOnlyCollection<TypeDefinition> MemberTypes { get; }

        protected internal override void FixReferences(IDictionary<string, ModelDefinition> models)
        {
            base.FixReferences(models);
            foreach (var memberType in MemberTypes) memberType.FixReferences(models);
        }
    }
}

#nullable disable