
-- Скрипт создания базы данных Dashboard


-- 1. Создаём базу данных (если ещё не создана)
-- CREATE DATABASE dashboard_db;

-- 2. Удаляем таблицы, если они существуют
DROP TABLE IF EXISTS sales CASCADE;
DROP TABLE IF EXISTS categories CASCADE;
DROP TABLE IF EXISTS regions CASCADE;

-- 3. Создаём таблицы
CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE regions (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL
);

CREATE TABLE sales (
    id SERIAL PRIMARY KEY,
    category_id INTEGER REFERENCES categories(id),
    region_id INTEGER REFERENCES regions(id),
    amount NUMERIC(12,2) NOT NULL,
    sale_date DATE NOT NULL
);

-- 4. Заполняем справочники
INSERT INTO categories (name) VALUES 
('Продукты питания'),
('Одежда и обувь'),
('Электроника'),
('Мебель и интерьер'),
('Книги и канцтовары'),
('Спорт и отдых');

INSERT INTO regions (name) VALUES 
('Москва'),
('Санкт-Петербург'),
('Екатеринбург'),
('Новосибирск'),
('Казань'),
('Краснодар');

-- 5. Генерируем тестовые продажи 
INSERT INTO sales (category_id, region_id, amount, sale_date)
SELECT 
    (FLOOR(random() * 6) + 1)::int,           
    (FLOOR(random() * 6) + 1)::int,           
    (random() * 450000 + 80000)::numeric(12,2),
    (CURRENT_DATE - (random() * 100)::int)
FROM generate_series(1, 400);