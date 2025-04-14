package utils

import (
	"regexp"
	"strings"
	"unicode"
    "unicode/utf8" // Import for rune counting

	"golang.org/x/text/runes"
	"golang.org/x/text/transform"
	"golang.org/x/text/unicode/norm"
)

var (
	nonAlphanumericRegex = regexp.MustCompile(`[^a-z0-9_-]+`)
	multiDashRegex       = regexp.MustCompile(`[-_]{2,}`)
    maxSlugLength        = 50 // Define the maximum length
)

// Slugify creates a URL-friendly "slug" from a given string, truncated to maxSlugLength.
func Slugify(s string) string {
	// ... (keep steps 1-5: normalize, lowercase, replace spaces, clean, single dash) ...
    t := transform.Chain(norm.NFD, runes.Remove(runes.In(unicode.Mn)), norm.NFC)
	normalized, _, _ := transform.String(t, s)
	lower := strings.ToLower(normalized)
    // Allow spaces temporarily to handle multi-word truncation better
	withHyphens := strings.ReplaceAll(lower, " ", "-") // Initial space replace
    cleaned := nonAlphanumericRegex.ReplaceAllString(withHyphens, "")
    singleDashed := multiDashRegex.ReplaceAllString(cleaned, "-")


	// 6. Trim leading and trailing hyphens/underscores FIRST
	trimmed := strings.Trim(singleDashed, "-_")

    // 7. Truncate if necessary - IMPORTANT: Work with runes for UTF-8 safety
    if utf8.RuneCountInString(trimmed) > maxSlugLength {
        // Convert to rune slice for safe slicing
        runes := []rune(trimmed)
        // Slice up to maxSlugLength runes
        trimmedRunes := runes[:maxSlugLength]
        // Convert back to string
        trimmed = string(trimmedRunes)
        // Trim again in case truncation left a trailing hyphen
        trimmed = strings.Trim(trimmed, "-_")
    }


	// Ensure it's not empty after truncation/trimming
	if trimmed == "" {
		return "untitled" // Or some default
	}

	return trimmed
}