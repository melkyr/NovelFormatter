# NovelFormatter
Migrations: NovelFormatter.sql

# Podman to run the MariaDB instance (sample code)
## podman run -d --name mariadb_container -e MYSQL_ROOT_PASSWORD=rootpassword -e MYSQL_DATABASE=mydatabase -e MYSQL_USER=myuser -e MYSQL_PASSWORD=mypassword -p 3306:3306 -v /mnt/f/MariaDB:/var/lib/mysql mariadb:latest

# Podman for rabbitMQ
## podman volume create rabbitmq_data
## podman run -d --name rabbitmq -p 5672:5672 -p 15672:15672 -v /mnt/f/RabbitMQ:/var/lib/rabbitmq,volume-opt=o-uid=1000,gid=1000 -e RABBITMQ_DEFAULT_USER=guest -e RABBITMQ_DEFAULT_PASS=guest rabbitmq:3.11-management
