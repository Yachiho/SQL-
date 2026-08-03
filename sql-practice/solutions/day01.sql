-- 問1
SELECT name, city FROM customers;

-- 問2
SELECT * FROM customers WHERE city = '東京';

-- 問3
SELECT * FROM products WHERE price >= 3000 ORDER BY price DESC;

-- 問4
SELECT name FROM products WHERE category = '書籍';

-- 問5
SELECT id, ordered_at FROM orders WHERE status = 'completed' ORDER BY ordered_at ASC;

-- 問6
SELECT * FROM products ORDER BY price ASC LIMIT 3;

-- 問7
SELECT * FROM customers WHERE email LIKE '%example.com%';

-- 問8
SELECT * FROM customers WHERE created_at >= '2025-06-01';

-- 問9
SELECT * FROM products WHERE category IN ('家電', '食品');

-- 問10
SELECT * FROM products WHERE price BETWEEN 2000 AND 4000;
