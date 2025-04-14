package generator

import (
	"bytes"
	"fmt"
	"html/template"
	"io"
	"log"
	"NovelStaticGenerator/internal/models" // Adjust import path
	"NovelStaticGenerator/internal/utils"  // Adjust import path
	"os"
	"path/filepath"
	"sort"
)

// SiteGenerator holds the state and configuration for the generation process.
type SiteGenerator struct {
	Chapters    []*models.Chapter
	OutputDir   string
	Templates   map[string]*template.Template
	StaticDir   string // Path to the source static assets directory
}

// NewSiteGenerator creates a new generator instance.
func NewSiteGenerator(chapters []*models.Chapter, outputDir string, tpl map[string]*template.Template, staticDir string) *SiteGenerator {
	return &SiteGenerator{
		Chapters:    chapters,
		OutputDir:   outputDir,
		Templates:   tpl,
		StaticDir:   staticDir,
	}
}

// GenerateSite orchestrates the entire site generation process.
func (sg *SiteGenerator) GenerateSite() error {
	log.Println("Starting static site generation...")

	// 1. Prepare output directory
	if err := sg.prepareOutputDir(); err != nil {
		return fmt.Errorf("failed to prepare output directory: %w", err)
	}

    // 2. Copy static assets (like CSS)
    if err := sg.copyStaticAssets(); err != nil {
		return fmt.Errorf("failed to copy static assets: %w", err)
	}

	// 3. Organize chapters by novel and process them
	novels := sg.organizeChapters()
	if len(novels) == 0 {
		log.Println("No novels found to generate.")
		return nil
	}

	// 4. Generate the main index page
	if err := sg.generateIndexPage(novels); err != nil {
		return fmt.Errorf("failed to generate index page: %w", err)
	}

	// 5. Generate pages for each chapter
	if err := sg.generateChapterPages(novels); err != nil {
		return fmt.Errorf("failed to generate chapter pages: %w", err)
	}

	log.Println("Static site generation completed successfully.")
	return nil
}

// prepareOutputDir ensures the output directory exists and is clean.
func (sg *SiteGenerator) prepareOutputDir() error {
	// Optional: Remove existing directory for a clean build
	// if err := os.RemoveAll(sg.OutputDir); err != nil {
	//  log.Printf("Warning: Could not remove existing output directory '%s': %v", sg.OutputDir, err)
	// }

	// Create the base output directory
	if err := os.MkdirAll(sg.OutputDir, 0755); err != nil {
		return fmt.Errorf("could not create output directory '%s': %w", sg.OutputDir, err)
	}
	log.Printf("Output directory '%s' prepared.", sg.OutputDir)
	return nil
}

// copyStaticAssets copies files from the static source directory to the output directory.
func (sg *SiteGenerator) copyStaticAssets() error {
	log.Printf("Copying static assets from '%s'...", sg.StaticDir)
	return filepath.Walk(sg.StaticDir, func(srcPath string, info os.FileInfo, err error) error {
		if err != nil {
			return fmt.Errorf("error accessing path %q: %w", srcPath, err)
		}

		// Calculate the destination path relative to the output directory
		relPath, err := filepath.Rel(sg.StaticDir, srcPath)
		if err != nil {
			return fmt.Errorf("could not get relative path for %q: %w", srcPath, err)
		}
		destPath := filepath.Join(sg.OutputDir, relPath)

		if info.IsDir() {
			// Create corresponding directory in output
			if err := os.MkdirAll(destPath, info.Mode()); err != nil {
				return fmt.Errorf("could not create directory %q: %w", destPath, err)
			}
			return nil // Don't copy the directory entry itself
		}

		// Copy the file
		log.Printf("Copying '%s' to '%s'", srcPath, destPath)
		return copyFile(srcPath, destPath)
	})
}

// copyFile copies a single file from src to dst.
func copyFile(src, dst string) error {
    sourceFile, err := os.Open(src)
    if err != nil {
        return fmt.Errorf("could not open source file %q: %w", src, err)
    }
    defer sourceFile.Close()

    destFile, err := os.Create(dst)
    if err != nil {
        return fmt.Errorf("could not create destination file %q: %w", dst, err)
    }
    defer destFile.Close()

    _, err = io.Copy(destFile, sourceFile)
    if err != nil {
        return fmt.Errorf("could not copy data from %q to %q: %w", src, dst, err)
    }

    // Preserve file permissions (optional, but good practice)
    info, err := os.Stat(src)
    if err == nil {
        err = os.Chmod(dst, info.Mode())
        if err != nil {
             log.Printf("Warning: Could not set permissions on %q: %v", dst, err)
        }
    } else {
         log.Printf("Warning: Could not stat source file %q: %v", src, err)
    }


    return nil
}


// organizeChapters groups chapters by novel name and sets up navigation.
func (sg *SiteGenerator) organizeChapters() []*models.Novel {
	chaptersByNovel := make(map[string][]*models.Chapter)

	// Group chapters by novel name
	for _, ch := range sg.Chapters {
		chaptersByNovel[ch.NovelName] = append(chaptersByNovel[ch.NovelName], ch)
	}

	novels := make([]*models.Novel, 0, len(chaptersByNovel))

	novelNames := make([]string, 0, len(chaptersByNovel))
	for name := range chaptersByNovel {
		novelNames = append(novelNames, name)
	}
	sort.Strings(novelNames) // Sort novels alphabetically by name for consistent index

	for _, novelName := range novelNames {
		chapters := chaptersByNovel[novelName]
		// Chapters should already be sorted by chapter_number from the DB query
		// If not, sort here: sort.Slice(chapters, func(i, j int) bool { return chapters[i].ChapterNumber < chapters[j].ChapterNumber })

		novelSlug := utils.Slugify(novelName)
		novel := &models.Novel{
			Name:     novelName,
			Slug:     novelSlug,
			Chapters: chapters,
		}

		// Set filenames and Next/Prev links
		for i, ch := range chapters {
			ch.NovelSlug = novelSlug
			// Include Volume Number in the filename format
			ch.FilenameHTML = fmt.Sprintf("v%d-c%d.html", ch.VolumeNumber, ch.ChapterNumber)
			ch.FilenameBulma = fmt.Sprintf("v%d-c%d-styled.html", ch.VolumeNumber, ch.ChapterNumber)

			if i > 0 {
				ch.PrevChapter = chapters[i-1]
			}
			if i < len(chapters)-1 {
				ch.NextChapter = chapters[i+1]
			}
		}
		novels = append(novels, novel)
	}

	return novels
}

// generateIndexPage creates the main index.html file.
func (sg *SiteGenerator) generateIndexPage(novels []*models.Novel) error {
	indexPath := filepath.Join(sg.OutputDir, "index.html")
	log.Printf("Generating index page: %s", indexPath)

	data := models.IndexPageData{
		Novels:        novels,
		IsBulmaStyled: false,
		SiteBasePath:  "",
	}

	// --- Execute template to buffer ---
	var buf bytes.Buffer // Create an in-memory buffer
	log.Printf("Attempting to execute template 'index.html' into buffer")
	err := sg.Templates["index"].ExecuteTemplate(&buf, "_base.html", data) // Execute _base.html
	if err != nil {
		log.Printf("!!! ERROR executing template 'index.html' into buffer: %v", err)
		// If executing to buffer fails, return the error immediately
		return fmt.Errorf("could not execute index template to buffer: %w", err)
	}
	log.Printf("Successfully executed template 'index.html' into buffer")

	// --- Log buffer content (snippet) ---
	outputHTML := buf.String()       // Get the string from the buffer
	outputSnippet := outputHTML
	if len(outputSnippet) > 200 {    // Limit log size
		outputSnippet = outputSnippet[:200] + "..."
	}
	// Log the actual generated content snippet
	log.Printf("  >>> Generated Index HTML (Snippet):\n---\n%s\n---", outputSnippet)
	if len(outputHTML) < 150 {       // Check if it seems too short (basic HTML structure should be > 150 chars)
		 log.Printf("  >>> WARNING: Generated Index HTML seems very short (length %d), base template likely missing?", len(outputHTML))
	}

	// --- Write buffer to file ---
	log.Printf("  Attempting os.WriteFile: %s", indexPath)
	err = os.WriteFile(indexPath, buf.Bytes(), 0644) // Use WriteFile to write buffer content
	if err != nil {
		log.Printf("!!! ERROR writing buffer to file '%s': %v", indexPath, err)
		return fmt.Errorf("could not write index file '%s': %w", indexPath, err)
	}
	log.Printf("  Successfully wrote buffer to file: %s", indexPath)

	return nil
}


func (sg *SiteGenerator) generateChapterPages(novels []*models.Novel) error {
	log.Println("--- Entering generateChapterPages ---") // Log entry into the function
	for novelIndex, novel := range novels {
		log.Printf("Processing Novel %d/%d: %s (Slug: %s)", novelIndex+1, len(novels), novel.Name, novel.Slug) // Log which novel
		novelDir := filepath.Join(sg.OutputDir, novel.Slug)

		// --- START DEBUG LOGGING ---
		log.Printf("  Attempting to create novel directory: %s", novelDir)
		// --- END DEBUG LOGGING ---
		if err := os.MkdirAll(novelDir, 0755); err != nil {
			log.Printf("!!! ERROR creating directory for novel '%s': %v", novel.Name, err) // Log error
			return fmt.Errorf("could not create directory for novel '%s': %w", novel.Name, err)
		}
		log.Printf("  Successfully created/ensured novel directory: %s", novelDir) // Log success


		log.Printf("  Generating %d chapters for novel: %s", len(novel.Chapters), novel.Name)
		for chapterIndex, chapter := range novel.Chapters {
			// --- START DEBUG LOGGING ---
			// Check for nil chapter right away
			if chapter == nil {
				log.Printf("!!! ERROR: Chapter at index %d for novel '%s' is nil. Skipping.", chapterIndex, novel.Name)
				continue
			}
			log.Printf("  Processing Chapter %d/%d: Vol %d Ch %d (DB ID: %d)",
				chapterIndex+1, len(novel.Chapters), chapter.VolumeNumber, chapter.ChapterNumber, chapter.ID) // Log which chapter
			// --- END DEBUG LOGGING ---

			// Generate Plain HTML version
			err := sg.renderChapter(novelDir, chapter, false) // isBulmaStyled = false
			if err != nil {
				log.Printf("!!! Error generating plain HTML for chapter V%d C%d (%s): %v", chapter.VolumeNumber, chapter.ChapterNumber, novel.Name, err)
				// Decide whether to continue or return error
				// return err // Stop on first error
				continue // Log and continue with next chapter
			}

			// Generate Bulma Styled HTML version
			err = sg.renderChapter(novelDir, chapter, true) // isBulmaStyled = true
			if err != nil {
				log.Printf("!!! Error generating Bulma HTML for chapter V%d C%d (%s): %v", chapter.VolumeNumber, chapter.ChapterNumber, novel.Name, err)
				// return err
				continue
			}
		}
		log.Printf("Finished generating chapters for novel: %s", novel.Name) // Log finish for novel
	}
	log.Println("--- Exiting generateChapterPages ---") // Log exit from the function
	return nil
}

// renderChapter writes a single chapter file (either plain or styled).
func (sg *SiteGenerator) renderChapter(novelDir string, chapter *models.Chapter, isBulmaStyled bool) error {
	log.Printf("    --- Entering renderChapter (DB ID: %d, Styled: %t) ---", chapter.ID, isBulmaStyled)

	var filename string
	styleType := "Plain HTML"
	if isBulmaStyled {
		log.Printf("      Inside 'if isBulmaStyled', about to access chapter.FilenameBulma (DB ID: %d)", chapter.ID)
		filename = chapter.FilenameBulma
		log.Printf("      Successfully accessed chapter.FilenameBulma. Value: '%s' (DB ID: %d)", filename, chapter.ID)
		styleType = "Bulma Styled"
	} else {
		filename = chapter.FilenameHTML
	}

	if filename == "" {
		log.Printf("!!! ERROR: Filename is empty after assignment for chapter DB ID %d (Styled: %t)", chapter.ID, isBulmaStyled)
		return fmt.Errorf("generated empty filename for chapter DB ID %d", chapter.ID)
	}
	log.Printf("    Determined filename: %s", filename)

	filePath := filepath.Join(novelDir, filename)
	logPath := filepath.Join(chapter.NovelSlug, filename)
	log.Printf("      Generating chapter file (%s): %s", styleType, logPath)

	data := models.ChapterPageData{
		NovelName:     chapter.NovelName,
		NovelSlug:     chapter.NovelSlug,
		Current:       chapter,
		IsBulmaStyled: isBulmaStyled,
		SiteBasePath:  "../",
	}

	// --- Execute template to buffer ---
	var buf bytes.Buffer // Create an in-memory buffer
	log.Printf("      Attempting to execute template 'chapter' into buffer for %s", logPath)
	err := sg.Templates["chapter"].ExecuteTemplate(&buf, "_base.html", data) // Execute _base.html
	if err != nil {
		log.Printf("!!! ERROR executing template 'chapter' into buffer for '%s': %v", logPath, err)
		// If executing to buffer fails, return the error immediately
		return fmt.Errorf("could not execute chapter template to buffer for '%s': %w", logPath, err)
	}
	log.Printf("      Successfully executed template 'chapter.html' into buffer for %s", logPath)

	// --- Log buffer content (snippet) ---
	outputHTML := buf.String()       // Get the string from the buffer
	outputSnippet := outputHTML
	if len(outputSnippet) > 200 {    // Limit log size
		outputSnippet = outputSnippet[:200] + "..."
	}
	// Log the actual generated content snippet
	log.Printf("        >>> Generated Chapter HTML (Snippet):\n---\n%s\n---", outputSnippet)
	if len(outputHTML) < 150 {       // Check if it seems too short (basic HTML structure should be > 150 chars)
		 log.Printf("        >>> WARNING: Generated Chapter HTML seems very short (length %d), base template likely missing?", len(outputHTML))
	}


	// --- Write buffer to file ---
	log.Printf("        Attempting os.WriteFile: %s", filePath)
	err = os.WriteFile(filePath, buf.Bytes(), 0644) // Use WriteFile to write buffer content
	if err != nil {
		log.Printf("!!! ERROR writing buffer to file '%s': %v", filePath, err)
		return fmt.Errorf("could not write chapter file '%s': %w", logPath, err)
	}
	log.Printf("        Successfully wrote buffer to file: %s", filePath)
	log.Printf("    --- Exiting renderChapter (DB ID: %d, Styled: %t) ---", chapter.ID, isBulmaStyled)


	return nil
}
