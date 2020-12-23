using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore;
using WebApi.Helpers;
using WebApi.Services;
using Microsoft.Extensions.Options;
using AutoMapper;
using WebApi.Entities;

namespace WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // add services to the DI container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore().AddApiExplorer().AddDataAnnotations();
            services.AddSwaggerGenNewtonsoftSupport();
            services.AddCors();
            services.AddControllers();

            // configure strongly typed settings object
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<AccommodDatabaseSettings>(Configuration.GetSection(nameof(AccommodDatabaseSettings)));
            services.AddSingleton<IAccommodDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<AccommodDatabaseSettings>>()
                .Value
            );

            // configure DI for application services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<INotificationService, NotificationService>();

            // swagger
            services.AddSwaggerGen();

            // serialize
            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            // auto mapper
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            services.AddSignalR();
        }

        // configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "My API V1");
            });
            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
                endpoint.MapHub<NotiHub>("/notiHub");
            });
        }
    }
}
