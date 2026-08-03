-- 顧客
CREATE TABLE customers (
    id          INTEGER PRIMARY KEY,
    name        TEXT    NOT NULL,
    email       TEXT    NOT NULL UNIQUE,
    city        TEXT,
    created_at  TEXT    NOT NULL   -- ISO8601形式 'YYYY-MM-DD'
);

-- 商品
CREATE TABLE products (
    id          INTEGER PRIMARY KEY,
    name        TEXT    NOT NULL,
    category    TEXT    NOT NULL,
    price       INTEGER NOT NULL   -- 円
);

-- 注文
CREATE TABLE orders (
    id          INTEGER PRIMARY KEY,
    customer_id INTEGER NOT NULL,
    ordered_at  TEXT    NOT NULL,  -- 'YYYY-MM-DD'
    status      TEXT    NOT NULL,  -- 'completed' / 'pending' / 'cancelled'
    FOREIGN KEY (customer_id) REFERENCES customers(id)
);

-- 注文明細
CREATE TABLE order_items (
    id          INTEGER PRIMARY KEY,
    order_id    INTEGER NOT NULL,
    product_id  INTEGER NOT NULL,
    quantity    INTEGER NOT NULL,
    FOREIGN KEY (order_id)   REFERENCES orders(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);
