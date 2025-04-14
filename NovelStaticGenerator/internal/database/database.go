package database

import (
	"database/sql"
	"fmt"
	"log"
	"NovelStaticGenerator/internal/models" // Adjust import path if needed

	_ "github.com/go-sql-driver/mysql" // MySQL driver
)

// ConnectDB establishes a connection to the database using the provided DSN.
func ConnectDB(dsn string) (*sql.DB, error) {
	db, err := sql.Open("mysql", dsn)
	if err != nil {
		return nil, fmt.Errorf("failed to open database connection: %w", err)
	}

	// Verify the connection is active
	err = db.Ping()
	if err != nil {
		db.Close() // Close the connection if ping fails
		return nil, fmt.Errorf("failed to ping database: %w", err)
	}

	log.Println("Database connection established successfully.")
	return db, nil
}

// FetchAllChapters retrieves all chapters from the database, ordered by novel name and chapter number.
func FetchAllChapters(db *sql.DB) ([]*models.Chapter, error) {
	// Adjust the query if your column names are different or if you add a title column
	query := `
        SELECT c.chapter_id, n.name AS novel_name, v.volume_number, c.chapter_number, c.content_html, c.content_bulma
        FROM chapters c
        INNER JOIN novels n ON n.novel_id = c.novel_id  -- Ensure correct join column names
        INNER JOIN volumes v ON v.volume_id = c.volume_id -- Ensure correct join column names
        ORDER BY novel_name, v.volume_number, c.chapter_number
    `

	rows, err := db.Query(query)
	if err != nil {
		return nil, fmt.Errorf("failed to execute chapter query: %w", err)
	}
	defer rows.Close() // Ensure rows are closed even if scanning fails

	chapters := []*models.Chapter{} // Use slice of pointers

	for rows.Next() {
		chapter := &models.Chapter{} // Create a new Chapter struct for each row
		// Add &chapter.Title if you have a title column
		err := rows.Scan(
			&chapter.ID,
			&chapter.NovelName,
			&chapter.VolumeNumber,
			&chapter.ChapterNumber,
			&chapter.ContentHTML,  // Scan directly into template.HTML
			&chapter.ContentBulma, // Scan directly into template.HTML
		)
		if err != nil {
			// Consider logging the error and skipping the row vs failing entirely
			log.Printf("Warning: Failed to scan chapter row: %v", err)
			continue // Skip this row and proceed with others
			// OR: return nil, fmt.Errorf("failed to scan chapter row: %w", err) // Fail hard
		}
		chapters = append(chapters, chapter)
	}

	// Check for errors encountered during iteration
	if err = rows.Err(); err != nil {
		return nil, fmt.Errorf("error encountered during row iteration: %w", err)
	}

	if len(chapters) == 0 {
		log.Println("Warning: No chapters found in the database.")
	} else {
		log.Printf("Fetched %d chapters from the database.", len(chapters))
	}

	return chapters, nil
}