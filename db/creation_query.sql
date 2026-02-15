CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE asp_net_users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_name TEXT NOT NULL,
    email TEXT UNIQUE,
    password_hash TEXT,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL
);

CREATE TABLE groups (
    id SERIAL PRIMARY KEY,
    owner_id UUID NOT NULL,
    name TEXT NOT NULL,
    is_personal BOOLEAN DEFAULT TRUE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_groups_owner FOREIGN KEY (owner_id) REFERENCES asp_net_users(id)
);

CREATE TABLE group_members (
    user_id UUID NOT NULL,
    group_id INT NOT NULL,
    role_id INT NOT NULL,
    active BOOLEAN DEFAULT TRUE,
    joined_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (user_id, group_id),

    CONSTRAINT fk_group_members_user FOREIGN KEY (user_id) REFERENCES asp_net_users(id) ON DELETE CASCADE,
    CONSTRAINT fk_group_members_group FOREIGN KEY (group_id) REFERENCES groups(id) ON DELETE CASCADE,
    CONSTRAINT fk_group_members_role FOREIGN KEY (role_id) REFERENCES roles(id)
);

CREATE TABLE group_member_history (
    id SERIAL,
    group_id INT NOT NULL,
    user_id UUID NOT NULL,
    changed_by_user_id UUID,
    role_id_before INT,
    role_id_after INT,
    active_before BOOLEAN,
    active_after BOOLEAN,
    note TEXT,
    changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (id, group_id), 

    CONSTRAINT fk_history_user FOREIGN KEY (user_id) REFERENCES asp_net_users(id),
    CONSTRAINT fk_history_group FOREIGN KEY (group_id) REFERENCES groups(id) ON DELETE CASCADE
);

CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    group_id INT,
    name TEXT NOT NULL,
    color_hex TEXT,
    is_system BOOLEAN DEFAULT FALSE,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_categories_group FOREIGN KEY (group_id) REFERENCES groups(id) ON DELETE CASCADE
);

CREATE TABLE sellers (
    id SERIAL,
    group_id INT NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (id, group_id),
    
    CONSTRAINT fk_sellers_group FOREIGN KEY (group_id) REFERENCES groups(id) ON DELETE CASCADE
);

CREATE TABLE receipts (
    id SERIAL,
    group_id INT NOT NULL,
    created_by_user_id UUID NOT NULL,
    seller_id INT, 
    total_amount DECIMAL(18, 2),
    payment_date TIMESTAMP,
    source_type TEXT,
    original_file_name TEXT,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (id, group_id),

    CONSTRAINT fk_receipts_group FOREIGN KEY (group_id) REFERENCES groups(id) ON DELETE CASCADE,
    CONSTRAINT fk_receipts_user FOREIGN KEY (created_by_user_id) REFERENCES asp_net_users(id),
    
    CONSTRAINT fk_receipts_seller FOREIGN KEY (seller_id, group_id) 
        REFERENCES sellers(id, group_id) ON DELETE SET NULL
);

CREATE TABLE product_data (
    id SERIAL,
    group_id INT NOT NULL,
    name TEXT NOT NULL,
    description TEXT,
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (id, group_id),

    CONSTRAINT fk_product_data_group FOREIGN KEY (group_id) REFERENCES groups(id) ON DELETE CASCADE
);

CREATE TABLE product_data_categories (
    product_data_id INT NOT NULL,
    category_id INT NOT NULL,
    group_id INT NOT NULL,

    PRIMARY KEY (product_data_id, category_id, group_id),

    CONSTRAINT fk_pdc_product FOREIGN KEY (product_data_id, group_id) 
        REFERENCES product_data(id, group_id) ON DELETE CASCADE,
    
    CONSTRAINT fk_pdc_category FOREIGN KEY (category_id) 
        REFERENCES categories(id) ON DELETE CASCADE
);

CREATE TABLE product_entries (
    id SERIAL,
    group_id INT NOT NULL,
    receipt_id INT NOT NULL,
    product_data_id INT NOT NULL,
    price DECIMAL(18, 2),
    quantity DECIMAL(18, 2),
    created_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (id, group_id),

    CONSTRAINT fk_entries_receipt FOREIGN KEY (receipt_id, group_id) 
        REFERENCES receipts(id, group_id) ON DELETE CASCADE,

    CONSTRAINT fk_entries_product FOREIGN KEY (product_data_id, group_id) 
        REFERENCES product_data(id, group_id) ON DELETE CASCADE
);