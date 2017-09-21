// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Http
{
    public static class RequestOptionsExtensions {
        public static IServiceCollection AddRequestOptions(this IServiceCollection services)
        {
            services.AddOptions();
            services.AddScoped(typeof(IRequestOptions<>), typeof(RequestOptions<>));
            services.AddTransient(typeof(IRequestOptionsFactory<>), typeof(RequestOptionsFactory<>));
            return services;
        }

        public static IServiceCollection ConfigureAsync<TOptions>(this IServiceCollection services, Func<string, TOptions, HttpContext, Task> configure) where TOptions : class, new()
            => services.AddSingleton<IConfigureRequestOptions<TOptions>>(new ConfigureRequestOptions<TOptions>(configure));

        public static Task<TOptions> GetOptionsAsync<TOptions>(this HttpContext context, string name) where TOptions : class, new()
        {
            var options = context.RequestServices.GetRequiredService<IRequestOptions<TOptions>>();
            return options.GetAsync(name, context);
        }
    }

    public interface IConfigureRequestOptions<TOptions> where TOptions : class
    {
        Task ConfigureAsync(string name, TOptions options, HttpContext context);
    }

    public class ConfigureRequestOptions<TOptions> : IConfigureRequestOptions<TOptions> where TOptions : class
    {
        private readonly Func<string, TOptions, HttpContext, Task> _configure;
        public ConfigureRequestOptions(Func<string, TOptions, HttpContext, Task> configure)
            => _configure = configure;

        public Task ConfigureAsync(string name, TOptions options, HttpContext context)
            => _configure.Invoke(name, options, context);
    }

    public interface IRequestOptions<TOptions> where TOptions : class
    {
        Task<TOptions> GetAsync(string name, HttpContext context);
    }

    public class RequestOptions<TOptions> : IRequestOptions<TOptions> where TOptions : class, new()
    {
        private readonly IRequestOptionsFactory<TOptions> _factory;
        private readonly Dictionary<string, TOptions> _cache = new Dictionary<string, TOptions>();

        public RequestOptions(IRequestOptionsFactory<TOptions> factory) => _factory = factory;

        public async Task<TOptions> GetAsync(string name, HttpContext context)
        {
            name = name ?? Options.DefaultName;
            if (_cache.ContainsKey(name))
            {
                return _cache[name];
            }

            // REVIEW: do we need to worry about concurrency with request options?  Can't use lock with async
            _cache[name] = await _factory.CreateAsync(name, context);
            return _cache[name];
        }
    }


    public interface IRequestOptionsFactory<TOptions> where TOptions : class, new()
    {
        Task<TOptions> CreateAsync(string name, HttpContext context);
    }

    public class RequestOptionsFactory<TOptions> : IRequestOptionsFactory<TOptions> where TOptions : class, new()
    {
        private readonly IOptionsFactory<TOptions> _factory;
        private readonly IEnumerable<IConfigureRequestOptions<TOptions>> _configures;

        // TODO: This can't really use the factory directly, rather it would need to rationlize things, since post configures should run last...
        public RequestOptionsFactory(IOptionsFactory<TOptions> factory, IEnumerable<IConfigureRequestOptions<TOptions>> configures) {
            _factory = factory;
            _configures = configures;
        }

        public async Task<TOptions> CreateAsync(string name, HttpContext context)
        {
            var options = _factory.Create(name);
            foreach (var configure in _configures)
            {
                await configure.ConfigureAsync(name, options, context);
            }
            return options;
        }
    }

}
