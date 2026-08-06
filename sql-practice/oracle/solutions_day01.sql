-- Oracle Day01 模範解答
-- exercises_day01.md の各問題に対応する。実行前に schema_oracle.sql / seed_oracle.sql を
-- 流し込んでおくこと。ROLLBACKを挟んでいる箇所は確認用データを戻すため。

-- 問1
SELECT table_name FROM user_tables ORDER BY table_name;

-- 問2
SELECT * FROM products ORDER BY price ASC FETCH FIRST 3 ROWS ONLY;

-- 問3
SELECT * FROM (
    SELECT p.*, ROW_NUMBER() OVER (ORDER BY price ASC) AS rn
    FROM products p
)
WHERE rn BETWEEN 4 AND 6;

-- 問4
INSERT INTO customers (name, email, city, created_at)
VALUES ('テスト太郎', 'sol_check_empty@example.com', '', TO_DATE('2025-07-01', 'YYYY-MM-DD'));

SELECT id, name, city FROM customers WHERE city IS NULL;

ROLLBACK;

-- 問5
SELECT 1 + 1 FROM dual;

-- 問6
SELECT id, ordered_at
FROM orders
WHERE ordered_at >= TO_DATE('2025-06-01', 'YYYY-MM-DD');

-- 問7
SELECT id, TO_CHAR(ordered_at, 'YYYY-MM-DD HH24:MI:SS') AS ordered_at_str
FROM orders;

-- 問8
SELECT name, NVL(city, '不明') AS city_nvl FROM customers;

SELECT name, COALESCE(city, '不明') AS city_coalesce FROM customers;

-- 問9
SELECT c.name, o.id AS order_id
FROM customers c
LEFT JOIN orders o ON o.customer_id = c.id
ORDER BY c.id;

SELECT c.name, o.id AS order_id
FROM customers c, orders o
WHERE o.customer_id (+) = c.id
ORDER BY c.id;

-- 問10
INSERT INTO customers (name, email, city, created_at)
VALUES ('採番確認太郎', 'sol_check_identity@example.com', '東京', TO_DATE('2025-07-02', 'YYYY-MM-DD'));

SELECT id, name FROM customers WHERE email = 'sol_check_identity@example.com';

ROLLBACK;
