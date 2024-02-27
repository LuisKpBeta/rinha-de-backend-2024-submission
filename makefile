build:
	docker build -t rinha-api-2024 . --no-cache 
	docker-compose up -d

reload:
	docker-compose down
	make build

run:
	dotnet run --project=src/rinha-backend-2024-q1.csproj

reset-db:
	docker exec -it rinha-backend-2024-q1_db_1 psql -h localhost -U postgres -d rinhadb -c "UPDATE customers SET balance=0; TRUNCATE transactions;"

push-docker:
	docker build -t luiscarlosb3/rinha-api-2024:1.0 . --no-cache 
	docker push luiscarlosb3/rinha-api-2024:1.0