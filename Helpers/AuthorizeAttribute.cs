using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using WebApi.Entities;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    List<string> Roles = new List<string>();
    public AuthorizeAttribute(params string[] roles)
    {
        Roles = new List<string>(roles);
    }
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = (User)context.HttpContext.Items["User"];
        var role = context.HttpContext.Items["Role"];
        if (user == null || role == null || (Roles.Count > 0 && !Roles.Contains(role.ToString()) ))
        {
            // not logged in
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}