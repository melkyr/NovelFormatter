using System;
using System.Text.RegularExpressions;
using System.Threading;
using DataAccess.Data;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovelExtractor.Messaging; // Make sure the namespace matches
using NovelPublisher;
using static System.Net.Mime.MediaTypeNames;

public class Program
{
    

    public static async Task Main(string[] args)
    {
        // Set up Configuration
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Set base path
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load JSON config
            .Build();

        // Dependency Injection Container
        var serviceProvider = new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton<DbConnectionFactory>()
            .AddSingleton<INovelRepository, NovelRepository>()
            .AddSingleton<IVolumeRepository, VolumeRepository>()
            .AddSingleton<IChapterRepository, ChapterRepository>()
            .AddScoped<IChapterQueueProcessor, ChapterQueueProcessor>()
            .BuildServiceProvider();

        // App Run
        IChapterQueueProcessor processor = serviceProvider.GetService<IChapterQueueProcessor>();
        processor.NovelId = 2;
        await processor.ProcessChapterQueue();
    }

   

    
}