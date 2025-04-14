using DataAccess.Data;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.Extensions.Configuration;
using NovelExtractor.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NovelPublisher
{
    public class ChapterQueueProcessor : IChapterQueueProcessor
    {
        private static readonly ManualResetEventSlim _shutdownEvent = new ManualResetEventSlim(false);
        private readonly IConfiguration configuration;
        private readonly IChapterRepository chpRepository;
        private readonly IVolumeRepository volumeRepository;
        public int NovelId { get; set; }

        public ChapterQueueProcessor(IConfiguration _configuration, IChapterRepository _chpRepository, IVolumeRepository _volRepository)
        {
            configuration = _configuration;
            chpRepository = _chpRepository;
            volumeRepository = _volRepository;

        }

        public async Task ProcessChapterQueue()
        {
            Console.WriteLine("--- Starting RabbitMQ Consumer Application ---");
            Console.WriteLine($"Current time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"); // Added timestamp

            ChapterConsumer? consumer = null;

            // Handle Ctrl+C or SIGTERM for graceful shutdown
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("\n[*] Shutdown signal received. Stopping consumer...");
                e.Cancel = true; // Prevent the process from terminating immediately
                _shutdownEvent.Set(); // Signal the main thread to exit
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                Console.WriteLine("[*] Process exiting. Ensuring cleanup...");
                consumer?.DisposeAsync(); // Ensure disposal on exit
            };

            try
            {
                // Create the consumer.
                // Option 1: Use a temporary, auto-delete queue (good for single instance consumers)
                //consumer = new ChapterConsumer(hostname: "localhost", queueName: null, bindingKey: "vol.#");

                // Option 2: Use a named, durable queue (good for shared queues or persistence)
                consumer = new ChapterConsumer(hostname: "localhost", queueName: "novel_processor_queue", bindingKey: "vol.#");

                // Subscribe to the message received event to process the data
                consumer.MessageReceived += async (sender, args) =>
                {
                    await processMessage(args);
                };

                // Start listening for messages
                await consumer.StartConsuming();

                // Wait for the shutdown signal (Ctrl+C)
                _shutdownEvent.Wait();

            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Consumer Error] Failed to connect or run: {ex.Message}");
                Console.WriteLine($"[Consumer Error] Make sure RabbitMQ is running and accessible.");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine("[*] Initiating final cleanup...");
                consumer?.DisposeAsync(); // Explicitly dispose here as well
            }

            Console.WriteLine("--- Consumer Application Exited ---");
        }
        private async Task processMessage((string RoutingKey, string Message) args)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n--- Received Chapter ---");
            Console.WriteLine($"  Routing Key: {args.RoutingKey}");
            Console.WriteLine($"  Content:");
            Console.ResetColor();
            Console.WriteLine(args.Message);
            ChapterModel data = await generateChapterInformation(args);
            await chpRepository.InsertAsync(data);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("--- End Chapter ---");
            Console.ResetColor();
        }

        private async Task<ChapterModel> generateChapterInformation((string RoutingKey, string Message) args)
        {
            ChapterModel data = new ChapterModel();
            int volume = 0;
            int chapter = 0;    
            bool volumeExists=false;
            int volumeId = 0;
            var parts = args.RoutingKey.Split('.');

            if (parts.Length == 4 && parts[0] == "vol" && parts[2] == "chapter")
            {
                volume = int.Parse(parts[1]);
                chapter = int.Parse(parts[3]);
            }
            var result = await volumeRepository.GetByVolumeNumberAsync(volume);
            if (result == null)
            {
                volumeId = volumeRepository.InsertAsync(new VolumeModel { Volume_Number = volume, Title = $"Volume {volume}", Description="N/a", Novel_Id=NovelId }).Result;
            }
            else 
            {
                volumeId = result.Volume_Id;
            }
            data.Title = $"{args.RoutingKey}";
            data.ContentPlain = args.Message;
            data.ContentHtml= HtmlConverter.GeneratePlainHtml(args.Message);
            data.ContentBulma= HtmlConverter.GenerateStyledHtml(args.Message);
            //data.ContentHtml = generateHtml(args.Message);
            //data.ContentBulma = generateStyledHtml(args.Message);
            data.NovelId = NovelId;
            data.ChapterNumber = chapter;
            data.VolumeId = volumeId;
            return data;
        }

        private string generateStyledHtml(string text)
        {
            // Convert "Chapter X" to <h2 class="title is-3">
            text = Regex.Replace(text, @"(?<=^|\n)(Chapter \d+)", "<h2 class=\"title is-3\">$1</h2>");

            // Convert dialogues into <blockquote class="notification is-light">
            text = Regex.Replace(text, @"(?<=^|\n)(.*?\(.*?\))", "<blockquote class=\"notification is-light\">$1</blockquote>");

            // Convert character names into <span class="tag is-info">
            text = Regex.Replace(text, @"\b(Saga|Abuelo de Silk|Acechador de Sombras|Anciano Elran|Anciano de los elfos|Bandido|Baron Gato|Barry|Barón Gato Tsomi|Benwood|Besio Salas|Biblion|Big Rod|Bran|Bran Crowder|Butlers|Caballero sin nombre|Camilla|Cangrejo de Acero|Capitán Jules|Capitán Jules Stien|Captain Jules|Carmine|Cazador Sombrío|Clover|Conde de Crowder|Condesa Crowder|Cronie A|Dimwit|Dorcas|Dormer|Dragón|Dryad|Dulcus|El mago sin nombre|Elder Elran|Elfos Oscuros|Elran|Feldio|Ferdio|Freia|Full Bound|Fullama|Glad Shi-Im|Glad-Shi-Im|Gold|Gopro-kun|Gopro-kun G|Gruntsblow|Guardias Salmutarianos|Guildmaster|Huevo Andante|Ilwen|Ilwen Pearlwood|Jamie|King Vordan|Lalm|Lefty Hand|Lizard|Loge|Lord Feldio|Lucent|Lun|Lung|Líder de los bandidos|Maid|Maje|Malignant|Malignant the Defiler|Mamal-san|Mamaru|Mamaru|Mamaru-san|Manauela|Manuela|Mapara|Marignant|Marina|Marona|Marqués|Marqués Bedivoir|Marqués de Bedivere|Mastoma|Mastoma-sama|Mayordomo|Mazaara|Mejaluna|Mieche|Miembros de Clover|Miriam|Mob A|Mobs A-D|Moriah|Mujer welmeriana|Nene|Nibelun|Nibelung|Nibelung|One Gold|Padre de Ilwen|Pale Undead King|Patriarca|Persephone|Personal del gremio|Perséfone|Prince Mastoma|Prince Rahma|Prince Rahuma|Príncipe Mastoma|Príncipe Mastoma|Príncipe Rahma|Príncipe Salmutaria|Rafael|Rahma|Rahuma|Rain|Rain|Rain|Rain|Reynise|Rey|Rey Pálido No Muerto|Rey Vaudan|Rey Vincent|Rey Vincent|Rey Vincent V|Rey Vincent V de Wellmeria|Rey Vordan|Rey de Welmeria|Rey del Trono|Reynise|Reynise|Rooge|Saga|Saga|Saga Ferdio|Scordia|Sensei|Shadow Stalker|Shadow Stalkers|Silk|Silk Amberwood|Simon|Simon Barkley|Sir Feldio|Sirviente sombrío de Ilwen|Skordia|Sohar|Soldado|Soldado Elfo Oscuro|Steel Crab|Stinger Joe|Thunder Pike|Thunderpike|Trent|Tymus|Tío Saga|Uno Dorado|Vibrion|Viktor|Vincent|Vincent V|Visconde Boardman|Vizconde Boardman|Vordan|Walkers|Wellmeria|Wilson|Yuke|Yuke Feldio|Yuki|Yuki Ferdio|Zaccardo|Zagnar|Zarnag
)\b", "<span class=\"tag is-info\">$1</span>");

            // Wrap paragraphs in <p class="content">
            text = Regex.Replace(text, @"(\n\s*\n)", "</p><p class=\"content\">");
            text = "<p class=\"content\">" + text + "</p>"; // Ensure first and last paragraphs are wrapped

            // Replace newlines with <br> to preserve spacing
            text = text.Replace("\n", "<br>");

            // Wrap everything in a Bulma section container
            text = $"<section class=\"section\"><div class=\"container\">{text}</div></section>";

            return text;
        }

        private static string generateHtml(string text)
        {
            // Convert "Chapter X" to <h2>
            text = Regex.Replace(text, @"(?<=^|\n)(Chapter \d+)", "<h2>$1</h2>");

            // Convert dialogues into blockquotes
            text = Regex.Replace(text, @"(?<=^|\n)(.*?\(.*?\))", "<blockquote>$1</blockquote>");

            // Convert character names into <strong>
            text = Regex.Replace(text, @"\b(Saga|Abuelo de Silk|Acechador de Sombras|Anciano Elran|Anciano de los elfos|Bandido|Baron Gato|Barry|Barón Gato Tsomi|Benwood|Besio Salas|Biblion|Big Rod|Bran|Bran Crowder|Butlers|Caballero sin nombre|Camilla|Cangrejo de Acero|Capitán Jules|Capitán Jules Stien|Captain Jules|Carmine|Cazador Sombrío|Clover|Conde de Crowder|Condesa Crowder|Cronie A|Dimwit|Dorcas|Dormer|Dragón|Dryad|Dulcus|El mago sin nombre|Elder Elran|Elfos Oscuros|Elran|Feldio|Ferdio|Freia|Full Bound|Fullama|Glad Shi-Im|Glad-Shi-Im|Gold|Gopro-kun|Gopro-kun G|Gruntsblow|Guardias Salmutarianos|Guildmaster|Huevo Andante|Ilwen|Ilwen Pearlwood|Jamie|King Vordan|Lalm|Lefty Hand|Lizard|Loge|Lord Feldio|Lucent|Lun|Lung|Líder de los bandidos|Maid|Maje|Malignant|Malignant the Defiler|Mamal-san|Mamaru|Mamaru|Mamaru-san|Manauela|Manuela|Mapara|Marignant|Marina|Marona|Marqués|Marqués Bedivoir|Marqués de Bedivere|Mastoma|Mastoma-sama|Mayordomo|Mazaara|Mejaluna|Mieche|Miembros de Clover|Miriam|Mob A|Mobs A-D|Moriah|Mujer welmeriana|Nene|Nibelun|Nibelung|Nibelung|One Gold|Padre de Ilwen|Pale Undead King|Patriarca|Persephone|Personal del gremio|Perséfone|Prince Mastoma|Prince Rahma|Prince Rahuma|Príncipe Mastoma|Príncipe Mastoma|Príncipe Rahma|Príncipe Salmutaria|Rafael|Rahma|Rahuma|Rain|Rain|Rain|Rain|Reynise|Rey|Rey Pálido No Muerto|Rey Vaudan|Rey Vincent|Rey Vincent|Rey Vincent V|Rey Vincent V de Wellmeria|Rey Vordan|Rey de Welmeria|Rey del Trono|Reynise|Reynise|Rooge|Saga|Saga|Saga Ferdio|Scordia|Sensei|Shadow Stalker|Shadow Stalkers|Silk|Silk Amberwood|Simon|Simon Barkley|Sir Feldio|Sirviente sombrío de Ilwen|Skordia|Sohar|Soldado|Soldado Elfo Oscuro|Steel Crab|Stinger Joe|Thunder Pike|Thunderpike|Trent|Tymus|Tío Saga|Uno Dorado|Vibrion|Viktor|Vincent|Vincent V|Visconde Boardman|Vizconde Boardman|Vordan|Walkers|Wellmeria|Wilson|Yuke|Yuke Feldio|Yuki|Yuki Ferdio|Zaccardo|Zagnar|Zarnag
)\b", "<strong>$1</strong>");

            // Wrap paragraphs in <p>
            text = Regex.Replace(text, @"(\n\s*\n)", "</p><p>");
            text = "<p>" + text + "</p>";  // Ensure first and last paragraphs are wrapped

            // Replace newlines with <br> to preserve spacing
            text = text.Replace("\n", "<br>");

            return text;
        }
    }
}
