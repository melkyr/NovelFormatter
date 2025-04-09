# NovelFormatter
Migrations:

Podman to run the MariaDB instance (sample code)
podman run -d --name mariadb_container -e MYSQL_ROOT_PASSWORD=rootpassword -e MYSQL_DATABASE=mydatabase -e MYSQL_USER=myuser -e MYSQL_PASSWORD=mypassword -p 3306:3306 -v /mnt/f/MariaDB:/var/lib/mysql mariadb:latest
Podman for rabbitMQ
podman volume create rabbitmq_data
podman run -d  --name rabbitmq  -p 5672:5672   # AMQP port  -p 15672:15672       # Management UI  -v /mnt/f/RabbitMQ:/var/lib/rabbitmq   -e RABBITMQ_DEFAULT_USER=guest   -e RABBITMQ_DEFAULT_PASS=guest   rabbitmq:3.11-management
