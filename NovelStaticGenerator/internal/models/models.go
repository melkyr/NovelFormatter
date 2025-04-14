package models

import "html/template" // Import html/template

// Chapter represents a single chapter fetched from the database.
// Note: Adjust field types (e.g., int vs int64) based on your DB schema's exact integer sizes.
type Chapter struct {
	ID            int    `db:"id"`             // Database primary key
	NovelName     string `db:"novel_name"`     // Name of the novel
	ChapterNumber int    `db:"chapter_number"` // Sequence number of the chapter
	VolumeNumber  int    `db:"volume_number"`
	// Title         string `db:"title"` // Optional: Add if you have a chapter title column
	ContentHTML  template.HTML `db:"content_html"`  // Pre-rendered plain HTML (Use template.HTML to prevent escaping)
	ContentBulma template.HTML `db:"content_bulma"` // Pre-rendered Bulma HTML (Use template.HTML)

	// --- Fields added for generation logic ---
	NovelSlug     string   // URL-friendly version of NovelName
	FilenameHTML  string   // Output filename for plain HTML version
	FilenameBulma string   // Output filename for Bulma version
	PrevChapter   *Chapter // Pointer to the previous chapter (nil if none)
	NextChapter   *Chapter // Pointer to the next chapter (nil if none)
}

// Novel represents a collection of chapters for a single novel.
type Novel struct {
	Name     string
	Slug     string
	Chapters []*Chapter // Sorted list of chapters
}

// IndexPageData holds data needed for the main index.html template.
type IndexPageData struct {
	Novels        []*Novel
	IsBulmaStyled bool // <<<--- ADD THIS FIELD
    // SiteBasePath isn't strictly needed here, but adding it
    // for consistency with _base.html might prevent future issues
    // if base template uses it elsewhere. Let's add it for safety.
    SiteBasePath string // <<<--- ADD THIS FIELD (Optional but recommended)

}

// ChapterPageData holds data needed for the chapter.html template.
type ChapterPageData struct {
	NovelName     string
	NovelSlug     string
	Current       *Chapter
	IsBulmaStyled bool // Field already exists here
	SiteBasePath  string // Field already exists here

}
