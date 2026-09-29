using AIVES.Web.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace AIVES.Web.Middleware
{
    /// <summary>
    /// Middleware that captures exceptions and populates ErrorViewModel with details.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "text/html; charset=utf-8";
            context.Response.StatusCode = 500;

            var model = new ErrorViewModel
            {
                RequestId = context.TraceIdentifier ?? Guid.NewGuid().ToString(),
                ErrorMessage = exception.Message,
                ErrorType = exception.GetType().Name,
                StackTrace = exception.StackTrace,
                Url = context.Request.Path.ToString(),
                HttpMethod = context.Request.Method,
                Timestamp = DateTime.UtcNow
            };

            if (exception.InnerException != null)
            {
                model.InnerErrorMessage = exception.InnerException.Message;
                model.InnerErrorType = exception.InnerException.GetType().Name;
                model.InnerStackTrace = exception.InnerException.StackTrace;
            }

            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            model.IsDevelopment = env.IsDevelopment();
            model.Environment = env.EnvironmentName;

            var queryDict = new Dictionary<string, string>();
            foreach (var key in context.Request.Query.Keys)
            {
                queryDict[key] = context.Request.Query[key].ToString();
            }
            model.QueryString = queryDict;

            var userAgentDict = new Dictionary<string, string>
            {
                ["User-Agent"] = context.Request.Headers["User-Agent"].ToString()
            };
            model.UserAgent = userAgentDict;

            var html = RenderErrorView(model);
            await context.Response.WriteAsync(html);
        }

        private static string RenderErrorView(ErrorViewModel model)
        {
            var env = model.IsDevelopment ? "Development" : "Production";
            var isDev = env == "Development";
            var badgeClass = isDev ? "success" : "secondary";
            var result = "<div class=\"container mt-4\"><div class=\"row justify-content-center\"><div class=\"col-md-10\">";
            result += "<div class=\"card border-danger shadow\">";
            result += "<div class=\"card-header bg-danger text-white\"><h4 class=\"mb-0\">Error Occurred</h4></div>";
            result += "<div class=\"card-body\"><h1 class=\"text-danger\">Error.</h1>";
            result += "<h2 class=\"text-danger\">An error occurred while processing your request.</h2>";

            if (isDev)
            {
                result += "<div class=\"alert alert-warning mt-4\"><strong>Development Mode:</strong> Detailed error information is displayed below.</div>";
                if (!string.IsNullOrEmpty(model.ErrorMessage))
                {
                    result += "<div class=\"alert alert-danger mt-3\"><h5>Error Details</h5>";
                    result += $"<p><strong>Type:</strong> {model.ErrorType}</p>";
                    result += $"<p><strong>Message:</strong></p><pre class=\"mb-0\">{model.ErrorMessage}</pre>";
                    if (!string.IsNullOrEmpty(model.InnerErrorMessage))
                    {
                        result += "<hr /><h6>Inner Exception:</h6>";
                        result += $"<p><strong>Type:</strong> {model.InnerErrorType}</p>";
                        result += $"<p><strong>Message:</strong></p><pre class=\"mb-0\">{model.InnerErrorMessage}</pre>";
                    }
                    if (!string.IsNullOrEmpty(model.StackTrace))
                    {
                        result += "<hr /><h6>Stack Trace:</h6>";
                        result += $"<pre class=\"bg-light p-3\" style=\"max-height: 400px; overflow-y: auto; font-size: 0.85rem;\">{model.StackTrace}</pre>";
                    }
                    result += "</div>";
                }
                result += "<div class=\"card mt-4\"><div class=\"card-header bg-secondary text-white\"><h6>Request Context</h6></div>";
                result += "<div class=\"card-body\"><div class=\"row\">";
                result += $"<div class=\"col-md-6 mb-3\"><strong>Timestamp:</strong><br /><span>{model.Timestamp:dd/MM/yyyy HH:mm:ss.fff}</span></div>";
                result += $"<div class=\"col-md-6 mb-3\"><strong>Environment:</strong><br /><span class=\"badge bg-{badgeClass}\">{model.Environment}</span></div>";
                result += $"<div class=\"col-md-6 mb-3\"><strong>URL:</strong><br /><code>{model.Url}</code></div>";
                result += $"<div class=\"col-md-6 mb-3\"><strong>HTTP Method:</strong><br /><span class=\"badge bg-primary\">{model.HttpMethod}</span></div>";
                result += $"<div class=\"col-md-6 mb-3\"><strong>Request ID:</strong><br /><code>{model.RequestId}</code></div>";
                result += "</div></div></div>";
            }
            else
            {
                result += "<div class=\"alert alert-info mt-4\"><h5>Something went wrong</h5>";
                result += "<p>We are sorry, but an unexpected error has occurred. Please try again later.</p>";
                result += $"<hr /><p><strong>Request ID:</strong> <code>{model.RequestId}</code><br /><strong>Timestamp:</strong> {model.Timestamp:dd/MM/yyyy HH:mm:ss}</p></div>";
            }
            result += "</div>";
            result += $"<div class=\"card-footer text-muted\"><a href=\"/\" class=\"btn btn-primary\">Return to Home</a></div>";
            result += "</div></div></div></div>";
            return result;
        }
    }
}
