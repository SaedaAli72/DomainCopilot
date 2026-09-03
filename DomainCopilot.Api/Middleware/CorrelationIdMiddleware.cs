using Serilog.Context;

namespace DomainCopilot.Api.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate a new unique ID for this request
            var correlationId = Guid.NewGuid().ToString();

            // Make it available for the rest of the request pipeline
            context.Items["CorrelationId"] = correlationId;

            // Attach it to every log line written during this request
            //لحد ما الطلب ده يخلص أي سطر لوج هيتكتب، حطي معاه الرقم ده تلقائي
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
