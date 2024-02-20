build:
	docker build -t rinha-api-2024 . --no-cache 
	docker-compose up -d

reload:
	docker-compose down
	make build

reset-db:
	docker exec -it rinha-backend-2024-q1_db_1 psql -h localhost -U postgres -d rinhadb -c "UPDATE customers SET balance=0; TRUNCATE transactions;"