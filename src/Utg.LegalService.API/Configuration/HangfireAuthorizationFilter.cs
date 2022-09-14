using System;
using System.Net.Http.Headers;
using System.Text;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Utg.LegalService.API.Configuration
{
	public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
	{
        private readonly string Login, Password;

		public HangfireAuthorizationFilter(IConfiguration confgifuration)
		{
            Login = confgifuration["HangfireAuth:Login"];
            Password = confgifuration["HangfireAuth:Password"];
		}

        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            var header = httpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(header))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var authValues = AuthenticationHeaderValue.Parse(header);

            if (!authValues.Scheme.Equals("Basic", StringComparison.InvariantCultureIgnoreCase))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var authInfo = Encoding.UTF8.GetString(Convert.FromBase64String(authValues.Parameter));

            if(authInfo == null)
			{
                SetChallengeResponse(httpContext);
                return false;
            }

            var parts = authInfo.Split(':');

            if (parts.Length < 2)
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            var username = parts[0];
            var password = parts[1];

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                SetChallengeResponse(httpContext);
                return false;
            }

            if (username.Equals(Login) && password.Equals(Password))
            {
                return true;
            }

            SetChallengeResponse(httpContext);
            return false;
        }

        private void SetChallengeResponse(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
            httpContext.Response.WriteAsync("Authentication is required.");
        }
    }
}
