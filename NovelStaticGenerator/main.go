package main

import (
	"html/template"
	"log"
	"NovelStaticGenerator/internal/config"    // Adjust import path
	"NovelStaticGenerator/internal/database" // Adjust import path
	"NovelStaticGenerator/internal/generator" // Adjust import path
	"os"
	"path/filepath"
)

const (
	templatesDir = "templates" // Directory containing HTML templates
	staticDir    = "static"    // Directory containing static assets (CSS, JS, images)
)

func main() {
	log.Println("Starting Novel Static Site Generator...")

	// 1. Load Configuration
	cfg, err := config.LoadConfig()
	if err != nil {
		log.Fatalf("Error loading configuration: %v", err)
	}
	log.Printf("Configuration loaded. Output directory: %s", cfg.OutputDir)

	// 2. Connect to Database
	db, err := database.ConnectDB(cfg.DSN())
	if err != nil {
		log.Fatalf("Error connecting to database: %v", err)
	}
	defer db.Close() // Ensure database connection is closed when main exits

	// 3. Fetch Chapters
	chapters, err := database.FetchAllChapters(db)
	if err != nil {
		log.Fatalf("Error fetching chapters: %v", err)
	}
	if len(chapters) == 0 {
		log.Println("No chapters fetched from the database. Exiting.")
		os.Exit(0) // Exit gracefully if there's nothing to process
	}

	// 4. Parse HTML Templates
	// Use Funcs to add custom template functions if needed later
	// E.g., funcs := template.FuncMap{"customFunc": myCustomFunc}
	tpl, err := loadTemplates(templatesDir)
	if err != nil {
		log.Fatalf("Error parsing templates from '%s': %v", templatesDir, err)
	}
	
	// 5. Initialize Site Generator
	gen := generator.NewSiteGenerator(chapters, cfg.OutputDir, tpl, staticDir)

	// 6. Run Generation Process
	err = gen.GenerateSite()
	if err != nil {
		log.Fatalf("Error during site generation: %v", err)
	}

	log.Println("Novel Static Site Generator finished successfully.")
}

func loadTemplates(templatesDir string) (map[string]*template.Template, error) {
	pages := []string{"index.html", "chapter.html"} // add your page-specific templates here
	base := filepath.Join(templatesDir, "_base.html")

	tmpls := make(map[string]*template.Template)

	for _, page := range pages {
		name := page[:len(page)-len(filepath.Ext(page))] // strip .html
		pagePath := filepath.Join(templatesDir, page)

		tmpl, err := template.New(name).ParseFiles(base, pagePath)
		if err != nil {
			return nil, err
		}

		tmpls[name] = tmpl
	}

	return tmpls, nil
}