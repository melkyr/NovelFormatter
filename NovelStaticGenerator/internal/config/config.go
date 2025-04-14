package config

import (
	"errors"
	"flag"
	"fmt"
	"os"
)

// Config holds application configuration.
type Config struct {
	DBUser     string
	DBPassword string
	DBHost     string
	DBPort     string
	DBName     string
	OutputDir  string
}

// LoadConfig loads configuration from environment variables or command-line flags.
// Flags take precedence over environment variables.
func LoadConfig() (*Config, error) {
	cfg := &Config{}

	// Define flags
	flag.StringVar(&cfg.DBUser, "dbuser", os.Getenv("DB_USER"), "Database username (env: DB_USER)")
	flag.StringVar(&cfg.DBPassword, "dbpass", os.Getenv("DB_PASSWORD"), "Database password (env: DB_PASSWORD)")
	flag.StringVar(&cfg.DBHost, "dbhost", os.Getenv("DB_HOST"), "Database host (env: DB_HOST)")
	flag.StringVar(&cfg.DBPort, "dbport", os.Getenv("DB_PORT"), "Database port (env: DB_PORT)")
	flag.StringVar(&cfg.DBName, "dbname", os.Getenv("DB_NAME"), "Database name (env: DB_NAME)")
	flag.StringVar(&cfg.OutputDir, "output", os.Getenv("OUTPUT_DIR"), "Output directory for static site (env: OUTPUT_DIR)")

	flag.Parse()

	// Basic validation
	if cfg.DBUser == "" || cfg.DBHost == "" || cfg.DBPort == "" || cfg.DBName == "" {
		return nil, errors.New("database credentials (user, host, port, name) are required")
	}
	if cfg.OutputDir == "" {
		return nil, errors.New("output directory is required")
	}
	// DB Password can be empty, but often required. Add check if necessary.

	return cfg, nil
}

// DSN generates the Data Source Name string for connecting to the database.
func (c *Config) DSN() string {
	// username:password@protocol(address)/dbname?param=value
	// Ensure parseTime=true is important for scanning into time.Time correctly if needed,
	// although not strictly required for the current model. It's good practice.
	return fmt.Sprintf("%s:%s@tcp(%s:%s)/%s?parseTime=true",
		c.DBUser, c.DBPassword, c.DBHost, c.DBPort, c.DBName)
}