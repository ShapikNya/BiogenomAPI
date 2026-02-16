# BiogenomAPI

**Версия:** 1.0 — первый релиз

BiogenomAPI — ASP.NET Core Web API по принципам **Clean Architecture**, для анализа изображений и определения объектов и материалов на них. Проект использует:

- Entity Framework Core + PostgreSQL
- MediatR + CQRS + DI
- AutoMapper
- Swagger
- Docker + Docker Compose с Nginx (reverse proxy)

---

## Структура проекта (`Biogenom.sln`)
- Biogenom.Application — бизнес-логика, CQRS команды/запросы, валидаторы
- Biogenom.Domain — сущности и интерфейсы
- Biogenom.Persistence — доступ к данным, EF Core, миграции
- Biogenom.WebApi — контроллеры, Swagger, запуск API

---

## Основные API-эндпоинты

### 1. Получение объектов на изображении
```http
POST https://localhost/api/image/process
{
  "ImageUrl": "https://test.ru/example.jpg"
}
```
**Пример промпта:**
```text
Определи все значимые объекты на изображении.
Ответь строго в формате JSON без текста вне JSON.
JSON должен содержать массив "objects", каждый объект которого имеет поле:
- "name": название объекта

Пример ответа:
{
  "objects": [
    { "name": "красное яблоко" },
    { "name": "зелёный лист" }
  ]
}
```

<img width="379" height="180" alt="image" src="https://github.com/user-attachments/assets/e772a8ea-5188-4f45-aab2-b88864e6c4e9" />

### 2. Определение материалов объектов
```http
POST https://localhost/api/image/{imageId}/analyze-materials
```
Body (можно оставить пустым):
```http
[
  "блокнот",
  "ручка"
]
```
**Логика:**
- Если body пустой - объекты берутся из базы
- Если body содержит объекты - старые объекты перезаписываются и материалы определяются для новых

**Пример промпта:**
```text
Определи, из каких материалов сделаны следующие объекты на изображении: {objects}.
Ответь строго в формате JSON без текста вне JSON:

{
  "materials": [
    { "object": "название объекта", "material": "материал" }
  ]
}
```

<img width="620" height="292" alt="image" src="https://github.com/user-attachments/assets/53c0f31a-f213-442d-a5fd-ab4d0e2b4358" />

3. Вспомогательные методы

```http
POST /api/image/upload
POST /api/image/delete
POST /api/image/{imageId}/analyze
```
**- Используется для тестов и локального анализа**

---

## Запуск через Docker

1) Клонируем репозиторий:
```bash
git clone https://github.com/ShapikNya/BiogenomAPI.git
```

2) Настраиваем .env:
```ini
POSTGRES_DB=TasksDB
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
DB_CONNECTION_DOCKER=Host=db;Port=5432;Database=BiogenomDB;Username=postgres;Password=postgres
ASPNETCORE_URLS=http://+:5000
ASPNETCORE_ENVIRONMENT=Docker
WEBAPI_PORT_HOST=8080
WEBAPI_PORT_CONTAINER=5000
DB_PORT_HOST=5433
DB_PORT_CONTAINER=5432
GIGACHAT_BEARER_TOKEN=<ваш токен>
```

3) Запускаем:
```bash
docker-compose up --build
```

Изображения сохраняются локально в:
```bash
BiogenomAPI\Biogenom.Backend\Biogenom.WebApi\bin\Debug\net8.0\UploadedImages
```

## Схема базы данных

### Таблица: `Images`

| Column | Type |
|--------|------|
| Id | uuid |
| FileName | varchar |
| Extension | varchar |
| FilePath | varchar |
| FileSize | bigint |
| CreatedAt | date |
| Status | varchar |


### Таблица: `DetectedObjects`

| Column | Type |
|--------|------|
| Id | uuid |
| ImageId | uuid |
| ObjectName | varchar |


### Таблица: `ObjectMaterials`

| Column | Type |
|--------|------|
| DetectedObjectId | uuid |
| MaterialName | varchar |


### ER-диаграмма 
<img width="1024" height="310" alt="image" src="https://github.com/user-attachments/assets/f38ed33e-d5fb-4d93-90d2-715045b85096" />


## Планируемое развитие

- Покрытие тестами
- Оптимизация хендлеров
- Логирование
- Более глубокая обработка исключений
- Расширение валидаторов

<img width="1421" height="832" alt="image" src="https://github.com/user-attachments/assets/1d88f43e-fcf2-4e0b-951d-d793abdb07b8" />



