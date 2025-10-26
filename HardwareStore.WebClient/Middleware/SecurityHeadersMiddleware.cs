namespace HardwareStore.WebClient.Middleware
{
    /// <summary>
    /// Middleware that adds essential HTTP security headers to every response.
    /// Hardened based on OWASP + ZAP recommendations.
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes the middleware with the next delegate in the pipeline.
        /// </summary>
        /// <param name="next"></param>
        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Invokes the middleware to add security headers to the HTTP response.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Prevent MIME sniffing
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";

            // Clickjacking protection
            context.Response.Headers["X-Frame-Options"] = "DENY";

            // Disable referrer info to third parties
            context.Response.Headers["Referrer-Policy"] = "no-referrer";

            // Disable browser features
            context.Response.Headers["Permissions-Policy"] =
               "geolocation=(), microphone=(), camera=()";

            // Enforce HTTPS for 2 years, include subdomains
            context.Response.Headers["Strict-Transport-Security"] =
                "max-age=63072000; includeSubDomains; preload";

            // Cache-control: prevent storing sensitive content
            context.Response.Headers["Cache-Control"] =
               "no-store, no-cache, must-revalidate";
            context.Response.Headers["Pragma"] = "no-cache";

            context.Response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' https://cdn.tailwindcss.com https://cdn.jsdelivr.net https://unpkg.com; " +
                "style-src 'self' https://cdn.jsdelivr.net https://fonts.googleapis.com; " +
                "font-src 'self' https://fonts.gstatic.com https://cdn.jsdelivr.net data:; " +
                "img-src 'self' data:; " +
                "connect-src 'self'; " +
                "form-action 'self'; " +
                "frame-ancestors 'none'; " +
                "object-src 'none'; " +
                "base-uri 'self';";

            await _next(context);
        }
    }
    /// <summary>
    /// Middleware extension for clean registration in Program.cs.
    /// </summary>
    public static class SecurityHeadersMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SecurityHeadersMiddleware>();
        }
    }
}