namespace PlayPao.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            if (path.HasValue && path.Value.StartsWith("/Auth"))
            {
                Console.WriteLine("Auth ");
            }

            return _next(context);
        }
    }
}