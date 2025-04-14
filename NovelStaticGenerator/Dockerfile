# --- Stage 1: Build ---
# Use an official Go image as the builder. Choose a specific version.
# Alpine version is smaller.
FROM golang:1.22-alpine AS builder

# Set the Current Working Directory inside the container
WORKDIR /app

# Copy go module files
COPY go.mod go.sum ./

# Download dependencies. This leverages Docker cache if mod files don't change.
RUN go mod download

# Copy the source code into the container
COPY . .

# Build the Go app
# - CGO_ENABLED=0: Build without Cgo (needed for static linking, works well with scratch/alpine)
# - GOOS=linux: Target Linux OS
# - Installs dependencies required for the build within the builder stage
# - -ldflags="-w -s": Strip debug information and symbols to reduce binary size
# - -o ./out/novel-generator: Output the binary to a specific location
RUN apk add --no-cache gcc musl-dev && \
    CGO_ENABLED=0 GOOS=linux go build \
    -ldflags="-w -s" \
    -o ./out/novel-generator ./main.go

# --- Stage 2: Runtime ---
# Use a minimal base image like Alpine for the final image
FROM alpine:latest

# Set the Current Working Directory inside the container
WORKDIR /app

# Copy the static assets (templates and static files) needed at runtime
COPY --from=builder /app/templates ./templates
COPY --from=builder /app/static ./static

# Copy the compiled binary from the builder stage
COPY --from=builder /app/out/novel-generator .

# (Optional) Create a non-root user to run the application
# RUN addgroup -S appgroup && adduser -S appuser -G appgroup
# USER appuser

# Define environment variables for configuration (defaults can be set here, but better set at runtime)
# These MUST be provided when running the container unless defaults are acceptable (not recommended for secrets)
ENV DB_USER=""
ENV DB_PASSWORD=""
ENV DB_HOST=""
ENV DB_PORT="3306"
ENV DB_NAME=""
ENV OUTPUT_DIR="/app/output_site" # Default output inside the container

# Expose ports (Not strictly needed for this generator, but good practice if it were a server)
# EXPOSE 8080

# Command to run the application
# Use ENTRYPOINT for the main executable
ENTRYPOINT ["./novel-generator"]
# CMD can provide default flags/args if needed, e.g., CMD ["-output", "/app/output_site"]
# If using ENTRYPOINT, flags need to be passed after the image name during `docker run`