using System;
using System.IO;
using DataAccess.Data;
using DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovelExtractor;

class Program
{
    static void Main(string[] args)
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
            .BuildServiceProvider();

        

        NovelParameters myParameters = new NovelParameters();
        SplitterStatus splitterStatus = new SplitterStatus();
        Processor NovelProcessor = new Processor(myParameters, splitterStatus);
        NovelProcessor.ProcessText();
    }
}



