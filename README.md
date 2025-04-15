NovelFormatter

A multi-language pipeline for processing, formatting, and publishing novels. This system is composed of two C# applications and one Go application, working together with RabbitMQ and MariaDB for processing and storage.
Components
🧠 NovelExtractor (C#)

    Takes a .txt file containing the full novel.

    Splits it into volumes and chapters using a configurable regex.

    Sends each chapter as a message to a RabbitMQ queue for downstream processing.

🖋️ NovelPublisher (C#)

    Consumes chapters from the RabbitMQ queue.

    Applies character-based transformations and formatting rules to generate:

        Raw HTML

        Clean HTML with basic inline styling

        Bulma CSS-enhanced HTML

    Inserts all versions into a MariaDB database.

🛠️ NovelStaticGenerator (Go)

    Queries the MariaDB database.

    Extracts novels and their chapters.

    Generates a static table of contents linking to all chapters.

    Allows readers to choose between raw, plain, or styled HTML versions.

Database Migrations

Run the schema setup with:

NovelFormatter.sql

Running Services with Podman
📦 MariaDB Instance

podman run -d \
  --name mariadb_container \
  -e MYSQL_ROOT_PASSWORD=rootpassword \
  -e MYSQL_DATABASE=mydatabase \
  -e MYSQL_USER=myuser \
  -e MYSQL_PASSWORD=mypassword \
  -p 3306:3306 \
  -v /mnt/f/MariaDB:/var/lib/mysql \
  mariadb:latest

📨 RabbitMQ Instance

podman volume create rabbitmq_data

podman run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -v /mnt/f/RabbitMQ:/var/lib/rabbitmq,volume-opt=o-uid=1000,gid=1000 \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  rabbitmq:3.11-management# NovelFormatter

A multi-language pipeline for processing, formatting, and publishing novels. This system is composed of two C# applications and one Go application, working together with RabbitMQ and MariaDB for processing and storage.

## Components

### 🧠 NovelExtractor (C#)
- Takes a `.txt` file containing the full novel.
- Splits it into volumes and chapters using a configurable regex.
- Sends each chapter as a message to a RabbitMQ queue for downstream processing.

### 🖋️ NovelPublisher (C#)
- Consumes chapters from the RabbitMQ queue.
- Applies character-based transformations and formatting rules to generate:
  - Raw HTML
  - Clean HTML with basic inline styling
  - Bulma CSS-enhanced HTML
- Inserts all versions into a MariaDB database.

### 🛠️ NovelStaticGenerator (Go)
- Queries the MariaDB database.
- Extracts novels and their chapters.
- Generates a static table of contents linking to all chapters.
- Allows readers to choose between raw, plain, or styled HTML versions.

---

## Database Migrations

Run the schema setup with:

NovelFormatter.sql


---

## Running Services with Podman

### 📦 MariaDB Instance

```bash
podman run -d \
  --name mariadb_container \
  -e MYSQL_ROOT_PASSWORD=rootpassword \
  -e MYSQL_DATABASE=mydatabase \
  -e MYSQL_USER=myuser \
  -e MYSQL_PASSWORD=mypassword \
  -p 3306:3306 \
  -v /mnt/f/MariaDB:/var/lib/mysql \
  mariadb:latest

podman volume create rabbitmq_data

podman run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -v /mnt/f/RabbitMQ:/var/lib/rabbitmq,volume-opt=o-uid=1000,gid=1000 \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  rabbitmq:3.11-management
