﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sabatex.Blazor.Identity.UI.Components.Account.Services;

/// <summary>
/// Extended Identity
/// </summary>
public static class IdentityExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddIsConfiguredGoogle(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        var clientId = configuration["Authentication:Google:ClientId"];
        var clientSecret = configuration["Authentication:Google:ClientSecret"];
        if (clientId != null && clientSecret != null)
        {
            builder.Services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
            });
        }
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddIsConfiguredMicrosoft(this AuthenticationBuilder builder, IConfiguration configuration)
    {
        var clientId = configuration["Authentication:Microsoft:ClientId"];
        var clientSecret = configuration["Authentication:Microsoft:ClientSecret"];
        if (clientId != null && clientSecret != null)
        {
            builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var email = context.Identity.FindFirst(ClaimTypes.Email)?.Value;
                        if (string.IsNullOrEmpty(email))
                        {
                            throw new ArgumentException("Email claim is missing.");
                        }
                        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityUser>>();
                        var user = await userManager.FindByEmailAsync(email);
                        if (user != null)
                        {
                            var loginProvider = context.Options.ClaimsIssuer ?? throw new ArgumentNullException(nameof(context.Options.ClaimsIssuer));
                            var providerKey = context.Identity.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new ArgumentNullException(nameof(ClaimTypes.NameIdentifier));
                            var loginInfo = new UserLoginInfo(loginProvider, providerKey, loginProvider);
                            await userManager.AddLoginAsync(user, loginInfo);
                        }
                    }
                };
            });
        }
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="userFileConfig"></param>
    /// <returns></returns>
    public static ConfigurationManager AddUserConfiguration(this ConfigurationManager manager, string userFileConfig)
    {
        var confogFileName = $"/etc/sabatex/{userFileConfig}";
        if (File.Exists(confogFileName))
            manager.AddJsonFile(confogFileName, optional: true, reloadOnChange: false);
        return manager;
    }

}
