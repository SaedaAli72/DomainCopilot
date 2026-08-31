
using DomainCopilot.Application.Interfaces;
using DomainCopilot.Application.UseCases;
using DomainCopilot.Infrastructure.Embeddings;
using DomainCopilot.Infrastructure.Llm;
using DomainCopilot.Infrastructure.Persistence;
using DomainCopilot.Infrastructure.Repositories;
using DomainCopilot.Infrastructure.Tenancy;
using DomainCopilot.Infrastructure.VectorStore;
using Microsoft.EntityFrameworkCore;

namespace DomainCopilot.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.Services.AddScoped<ILlmClient, FakeLlmClient>();
            builder.Services.AddHttpClient<ILlmClient, GeminiLlmClient>();
            //builder.Services.AddScoped<IVectorStore, FakeVectorStore>();
            builder.Services.AddScoped<IVectorStore, SqlVectorStore>();
            //builder.Services.AddScoped<IEmbeddingService, FakeEmbeddingService>();
            builder.Services.AddHttpClient<IEmbeddingService, GeminiEmbeddingService>();
            builder.Services.AddScoped<AskQuestionUseCase>();
            builder.Services.AddScoped<IDocumentRepository, EfDocumentRepository>();
            builder.Services.AddScoped<IngestDocumentUseCase>();
            builder.Services.AddScoped<ITenantProvider, StaticTenantProvider>();
            builder.Services.AddScoped<IDocumentChunkRepository, EfDocumentChunkRepository>();

            builder.Services.AddDbContext<AppDbContext>(options =>
             options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
