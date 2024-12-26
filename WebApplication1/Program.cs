using System.Globalization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configuración del Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    //options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
    //    httpcontext => RateLimitPartition.GetFixedWindowLimiter(
    //        partitionKey: httpcontext.Request.Headers.Host.ToString(),
    //        factory: partition => new FixedWindowRateLimiterOptions
    //        {                
    //            AutoReplenishment = true,
    //            PermitLimit = 1000,
    //            Window = TimeSpan.FromMinutes(1)
    //        }
    //    )
    //);

    options.AddPolicy("fixed", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(httpContext.Request.Headers.Host.ToString(),
    partition => new FixedWindowRateLimiterOptions
    {
        AutoReplenishment = true,
        PermitLimit = 100,
        Window = TimeSpan.FromMinutes(1)
    }));

    options.AddPolicy("extremelyLimit", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(httpContext.Request.Headers.Host.ToString(),
    partition => new FixedWindowRateLimiterOptions
    {
        AutoReplenishment = true,
        PermitLimit = 3,
        Window = TimeSpan.FromSeconds(10)
    }));

    options.OnRejected = (context, options) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");

        return new ValueTask();
    };
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRateLimiter();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
