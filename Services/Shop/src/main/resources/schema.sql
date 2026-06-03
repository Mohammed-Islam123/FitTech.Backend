CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS products (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT,
    price DECIMAL(10, 2) NOT NULL,
    category VARCHAR(100),
    image_path VARCHAR(500),
    stock INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE IF NOT EXISTS purchases (
    id UUID PRIMARY KEY,
    member_id UUID,
    product_id UUID NOT NULL REFERENCES products(id),
    quantity INTEGER NOT NULL,
    purchased_at TIMESTAMP WITH TIME ZONE NOT NULL
);
