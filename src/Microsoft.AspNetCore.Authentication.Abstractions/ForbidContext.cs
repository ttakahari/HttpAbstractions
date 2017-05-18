// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Context used for Forbids.
    /// </summary>
    public class ForbidContext : BaseAuthenticationContext
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="httpContext">The context.</param>
        /// <param name="authenticationScheme">The name of the scheme.</param>
        public ForbidContext(HttpContext httpContext, string authenticationScheme)
            : this(httpContext, authenticationScheme, properties: null)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpContext">The context.</param>
        /// <param name="authenticationScheme">The name of the scheme.</param>
        /// <param name="properties">The properties.</param>
        public ForbidContext(HttpContext httpContext, string authenticationScheme, AuthenticationProperties properties)
            : base(httpContext, authenticationScheme, properties)
        {
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException(nameof(authenticationScheme));
            }
        }
    }
}