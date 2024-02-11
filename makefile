build:
	docker build -t rinha-api-2024 . --no-cache 
	docker-compose up -d

reload:
	docker-compose down
	make build