using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using Owin;
using LoginTest2.Models;
using LoginTest2.Logic;

[assembly: OwinStartup(typeof(LoginTest2.Startup))]
namespace LoginTest2
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            //other1 configurations
app.UseCors(CorsOptions.AllowAll);
            ConfigureOAuth(app);
            
            app.UseWebApi(config);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            var oAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/api/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(12),
                Provider = new AuthorizationServerProvider()
            };
            app.UseOAuthAuthorizationServer(oAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }
    }

    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
            string errmsg = "";
            string sName = "", sUserId = "0";
            try
            {
                int SocietyId = 0;
                var form = await context.Request.ReadFormAsync();
                if (form["SocietyId"] != null)
                    SocietyId = Convert.ToInt32(form["SocietyId"]);

                //Convert Password to PasswordHash
                string PasswordHash = CryptoHashing.Encrypt(context.Password);

                DbRepository db = new DbRepository();
                Users u = db.LoginUser(context.UserName, PasswordHash, out errmsg);

                if (errmsg != "success")
                {
                    context.SetError("invalid_grant", "Error-" + errmsg);
                    return;
                }
                else
                {
                    if (u == null)
                    {
                        context.SetError("invalid_grant", "Invalid username or password !");
                        return;
                    }
                    else
                    {
                        sUserId = u.Id.ToString();
                        sName = u.Name;

                        var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                        identity.AddClaim(new Claim("MemberName", sName));
                        identity.AddClaim(new Claim("UserId", sUserId));

                        //roles example
                        var rolesTechnicalNamesUser = new List<string>();

                        var principal = new GenericPrincipal(identity, rolesTechnicalNamesUser.ToArray());

                        Thread.CurrentPrincipal = principal;

                        var props = new AuthenticationProperties(new Dictionary<string, string>
                        {
                            {"userid", sUserId},
                            {"name", sName}
                        });
                        var ticket = new AuthenticationTicket(identity, props);
                        context.Validated(ticket);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("See the inner exception for details"))
                    errmsg = ex.InnerException.Message;
                else
                    errmsg = ex.Message;

                context.SetError("error_occured", errmsg);
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }
            return Task.FromResult<object>(null);
        }
    }

}