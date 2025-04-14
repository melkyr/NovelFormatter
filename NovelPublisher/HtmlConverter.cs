using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NovelPublisher
{
    public class HtmlConverter
    {
        // Use a HashSet for efficient lookups and easier maintenance
        private static readonly HashSet<string> CharacterNames = new HashSet<string>(StringComparer.Ordinal)
    {
        "Saga", "Abuelo de Silk", "Acechador de Sombras", "Anciano Elran", "Anciano de los elfos", "Bandido",
        "Baron Gato", "Barry", "Barón Gato Tsomi", "Benwood", "Besio Salas", "Biblion", "Big Rod", "Bran",
        "Bran Crowder", "Butlers", "Caballero sin nombre", "Camilla", "Cangrejo de Acero", "Capitán Jules",
        "Capitán Jules Stien", "Captain Jules", "Carmine", "Cazador Sombrío", "Clover", "Conde de Crowder",
        "Condesa Crowder", "Cronie A", "Dimwit", "Dorcas", "Dormer", "Dragón", "Dryad", "Dulcus",
        "El mago sin nombre", "Elder Elran", "Elfos Oscuros", "Elran", "Feldio", "Ferdio", "Freia",
        "Full Bound", "Fullama", "Glad Shi-Im", "Glad-Shi-Im", "Gold", "Gopro-kun", "Gopro-kun G",
        "Gruntsblow", "Guardias Salmutarianos", "Guildmaster", "Huevo Andante", "Ilwen", "Ilwen Pearlwood",
        "Jamie", "King Vordan", "Lalm", "Lefty Hand", "Lizard", "Loge", "Lord Feldio", "Lucent", "Lun",
        "Lung", "Líder de los bandidos", "Maid", "Maje", "Malignant", "Malignant the Defiler", "Mamal-san",
        "Mamaru", "Mamaru-san", "Manauela", "Manuela", "Mapara", "Marignant", "Marina", "Marona", "Marqués",
        "Marqués Bedivoir", "Marqués de Bedivere", "Mastoma", "Mastoma-sama", "Mayordomo", "Mazaara",
        "Mejaluna", "Mieche", "Miembros de Clover", "Miriam", "Mob A", "Mobs A-D", "Moriah",
        "Mujer welmeriana", "Nene", "Nibelun", "Nibelung", "One Gold", "Padre de Ilwen", "Pale Undead King",
        "Patriarca", "Persephone", "Personal del gremio", "Perséfone", "Prince Mastoma", "Prince Rahma",
        "Prince Rahuma", "Príncipe Mastoma", "Príncipe Rahma", "Príncipe Salmutaria", "Rafael", "Rahma",
        "Rahuma", "Rain", "Reynise", "Rey", "Rey Pálido No Muerto", "Rey Vaudan", "Rey Vincent",
        "Rey Vincent V", "Rey Vincent V de Wellmeria", "Rey Vordan", "Rey de Welmeria", "Rey del Trono",
        "Rooge", "Saga Ferdio", "Scordia", "Sensei", "Shadow Stalker", "Shadow Stalkers", "Silk",
        "Silk Amberwood", "Simon", "Simon Barkley", "Sir Feldio", "Sirviente sombrío de Ilwen", "Skordia",
        "Sohar", "Soldado", "Soldado Elfo Oscuro", "Steel Crab", "Stinger Joe", "Thunder Pike",
        "Thunderpike", "Trent", "Tymus", "Tío Saga", "Uno Dorado", "Vibrion", "Viktor", "Vincent",
        "Vincent V", "Visconde Boardman", "Vizconde Boardman", "Vordan", "Walkers", "Wellmeria", "Wilson",
        "Yuke", "Yuke Feldio", "Yuki", "Yuki Ferdio", "Zaccardo", "Zagnar", "Zarnag"
        // Add/Remove names here easily
    };

        // Pre-compile Regex for performance
        private static readonly Regex ChapterRegex = new Regex(@"^Chapter (\d+)$", RegexOptions.Compiled);
        // This dialogue regex assumes the format "Speaker (Dialogue Text)" or just "(Dialogue Text)" on its own line.
        // It might need adjustment based on exact dialogue formatting rules.
        // Using a simpler check for lines containing parentheses, similar to original:
        private static readonly Regex DialogueRegex = new Regex(@"^\s*.*\(.*\)\s*$", RegexOptions.Compiled);
        private static readonly Regex CharacterNameRegex = BuildCharacterNameRegex();

        // Helper class to define HTML structure and classes
        private class HtmlStyleConfig
        {
            public string SectionClass { get; set; } = "";
            public string ContainerClass { get; set; } = "";
            public string ChapterTag { get; set; } = "h2";
            public string ChapterClass { get; set; } = "";
            public string DialogueTag { get; set; } = "blockquote";
            public string DialogueClass { get; set; } = "";
            public string NameTag { get; set; } = "strong";
            public string NameClass { get; set; } = "";
            public string ParagraphTag { get; set; } = "p";
            public string ParagraphClass { get; set; } = "";
            public bool UseWrapper { get; set; } = false; // Flag to wrap in section/container
        }

        // Builds the character name Regex dynamically
        private static Regex BuildCharacterNameRegex()
        {
            // Escape names in case they contain special Regex characters, then join with | (OR)
            string pattern = @"\b(" + string.Join("|", CharacterNames.Select(Regex.Escape)) + @")\b";
            return new Regex(pattern, RegexOptions.Compiled);
        }

        // Applies highlighting to character names within a line
        private static string HighlightNames(string line, string tag, string cssClass)
        {
            string replacement = string.IsNullOrEmpty(cssClass)
                ? $"<{tag}>$1</{tag}>"
                : $"<{tag} class=\"{cssClass}\">$1</{tag}>";

            return CharacterNameRegex.Replace(line, replacement);
        }

        // Core HTML generation logic
        private static string GenerateHtmlCore(string text, HtmlStyleConfig config)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var htmlBuilder = new StringBuilder();
            // Split into lines, keeping empty lines which are important for paragraph breaks
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            bool inParagraph = false;

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim(); // Trim whitespace for checks, but use rawLine for content spacing

                // 1. Check for Chapter heading
                var chapterMatch = ChapterRegex.Match(line);
                if (chapterMatch.Success)
                {
                    if (inParagraph)
                    {
                        htmlBuilder.AppendLine($"</{config.ParagraphTag}>");
                        inParagraph = false;
                    }
                    string chapterClassAttr = string.IsNullOrEmpty(config.ChapterClass) ? "" : $" class=\"{config.ChapterClass}\"";
                    // Use the original matched text "Chapter X"
                    htmlBuilder.AppendLine($"<{config.ChapterTag}{chapterClassAttr}>{chapterMatch.Groups[0].Value}</{config.ChapterTag}>");
                    continue; // Move to next line
                }

                // 2. Check for Dialogue
                // Using the simplified DialogueRegex check
                if (DialogueRegex.IsMatch(rawLine)) // Check raw line to preserve indentation if desired inside blockquote
                {
                    if (inParagraph)
                    {
                        htmlBuilder.AppendLine($"</{config.ParagraphTag}>");
                        inParagraph = false;
                    }
                    string dialogueClassAttr = string.IsNullOrEmpty(config.DialogueClass) ? "" : $" class=\"{config.DialogueClass}\"";
                    // Highlight names *within* the dialogue line before wrapping
                    string processedLine = HighlightNames(rawLine.TrimStart(), config.NameTag, config.NameClass); // TrimStart to align left in quote
                    htmlBuilder.AppendLine($"<{config.DialogueTag}{dialogueClassAttr}>{processedLine}</{config.DialogueTag}>");
                    continue; // Move to next line
                }

                // 3. Handle Paragraphs and Line Breaks
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Empty line signifies a paragraph break
                    if (inParagraph)
                    {
                        htmlBuilder.AppendLine($"</{config.ParagraphTag}>");
                        inParagraph = false;
                    }
                }
                else
                {
                    // Line has content
                    if (!inParagraph)
                    {
                        string paragraphClassAttr = string.IsNullOrEmpty(config.ParagraphClass) ? "" : $" class=\"{config.ParagraphClass}\"";
                        htmlBuilder.Append($"<{config.ParagraphTag}{paragraphClassAttr}>"); // Append, don't add newline yet
                        inParagraph = true;
                    }

                    // Highlight names in the line content
                    string processedLine = HighlightNames(rawLine, config.NameTag, config.NameClass);
                    htmlBuilder.Append(processedLine); // Append the processed line content

                    // Add <br> for the newline, preserving spacing within paragraphs
                    htmlBuilder.Append("<br />"); // Use self-closing for XHTML compatibility
                    htmlBuilder.AppendLine(); // Add a newline in the HTML source for readability
                }
            }

            // Close the last paragraph if needed
            if (inParagraph)
            {
                // Remove the last <br /> before closing the paragraph
                string currentHtml = htmlBuilder.ToString();
                int lastBr = currentHtml.LastIndexOf("<br />", StringComparison.OrdinalIgnoreCase);
                if (lastBr > 0 && lastBr == currentHtml.Length - "<br />".Length - Environment.NewLine.Length) // Check if it's at the very end
                {
                    htmlBuilder.Length = lastBr; // Remove <br /> and trailing newline
                }
                htmlBuilder.AppendLine($"</{config.ParagraphTag}>");
            }

            // Wrap in container if configured
            string finalHtml = htmlBuilder.ToString().TrimEnd(); // Trim trailing whitespace/newlines
            if (config.UseWrapper)
            {
                string sectionClassAttr = string.IsNullOrEmpty(config.SectionClass) ? "" : $" class=\"{config.SectionClass}\"";
                string containerClassAttr = string.IsNullOrEmpty(config.ContainerClass) ? "" : $" class=\"{config.ContainerClass}\"";
                return $"<section{sectionClassAttr}>\n<div{containerClassAttr}>\n{finalHtml}\n</div>\n</section>";
            }
            else
            {
                return finalHtml;
            }
        }

        // Public method for Bulma Styled HTML
        public static string GenerateStyledHtml(string text)
        {
            var config = new HtmlStyleConfig
            {
                SectionClass = "section",
                ContainerClass = "container",
                ChapterTag = "h2",
                ChapterClass = "title is-3",
                DialogueTag = "blockquote",
                DialogueClass = "notification is-light", // Using notification for distinct styling
                NameTag = "span",
                NameClass = "tag is-info",
                ParagraphTag = "p",
                ParagraphClass = "content", // Bulma's class for typography styling
                UseWrapper = true
            };
            return GenerateHtmlCore(text, config);
        }

        // Public method for Plain HTML
        public static string GeneratePlainHtml(string text)
        {
            var config = new HtmlStyleConfig
            {
                // No classes for plain HTML
                ChapterTag = "h2",
                DialogueTag = "blockquote",
                NameTag = "strong",
                ParagraphTag = "p",
                UseWrapper = false // No section/container wrap
            };
            return GenerateHtmlCore(text, config);
        }
    }
}
