using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QueryMapper;
using QueryMapper.Examples.Core;


namespace QueryBase.Examples.Core
{
    public static class ServiceRegistrar
    {
        public static IServiceCollection Register(this IServiceCollection services, string connString)
        {
            services
                .AddQueryMapper<BookMapper>()
                .AddDbContext<BookContext>(builder => builder.UseSqlServer(connString))
                .RegisterRepos()
                ;
            return services;
        }

        public static IServiceCollection RegisterRepos(this IServiceCollection services)
        {
            services.AddScoped<IBookRepository, BookRepository>();
            return services;
        }
    }
}
