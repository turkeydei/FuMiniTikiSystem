-- Script để reset identity seed cho tất cả các bảng
-- Chạy script này để sửa lỗi ID tự tăng

-- Reset identity seed cho bảng Categories
DBCC CHECKIDENT ('Categories', RESEED, 0);
GO

-- Reset identity seed cho bảng Products  
DBCC CHECKIDENT ('Products', RESEED, 0);
GO

-- Reset identity seed cho bảng Customers
DBCC CHECKIDENT ('Customers', RESEED, 0);
GO

-- Reset identity seed cho bảng Orders
DBCC CHECKIDENT ('Orders', RESEED, 0);
GO

-- Reset identity seed cho bảng OrderDetails
DBCC CHECKIDENT ('OrderDetails', RESEED, 0);
GO

-- Reset identity seed cho bảng Carts
DBCC CHECKIDENT ('Carts', RESEED, 0);
GO

-- Reset identity seed cho bảng CartItems
DBCC CHECKIDENT ('CartItems', RESEED, 0);
GO

-- Reset identity seed cho bảng Payments
DBCC CHECKIDENT ('Payments', RESEED, 0);
GO

-- Reset identity seed cho bảng Reviews
DBCC CHECKIDENT ('Reviews', RESEED, 0);
GO

-- Kiểm tra identity seed hiện tại
SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    IDENT_CURRENT(TABLE_NAME) as CurrentIdentity,
    IDENT_INCR(TABLE_NAME) as IdentityIncrement,
    IDENT_SEED(TABLE_NAME) as IdentitySeed
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE COLUMNPROPERTY(object_id(TABLE_SCHEMA + '.' + TABLE_NAME), COLUMN_NAME, 'IsIdentity') = 1
ORDER BY TABLE_NAME; 