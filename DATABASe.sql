USE master
GO 

IF EXISTS(SELECT name FROM sys.databases WHERE name = N'FashionShop')
BEGIN 
    ALTER DATABASE FashionShop SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FashionShop;
END

CREATE DATABASE FashionShop 
GO 

USE FashionShop
GO 

SET ANSI_NULLS ON
GO 
SET QUOTED_IDENTIFIER ON
GO 

CREATE TABLE [dbo].[User] (
    [user_id] [int] IDENTITY(1,1) NOT NULL,
    [first_name] [nvarchar](255) NULL,
    [last_name] [nvarchar](255) NULL,
    [email] [nvarchar](255) NOT NULL,
    [username] [nvarchar](20) NOT NULL,
    [password] [nvarchar](255) NOT NULL,
    [address] [nvarchar](255) NULL,
    [phone_number] [nvarchar](20) NULL,
    [permission] [int] NOT NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([user_id] ASC)
) ON [PRIMARY]
GO 

CREATE TABLE [dbo].[Category] (
    [category_id] [int] IDENTITY(1,1) NOT NULL,
    [name] [nvarchar](255) NOT NULL,
    [image] [nvarchar](255) NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([category_id] ASC)
) ON [PRIMARY]
GO 

CREATE TABLE [dbo].[Product] (
    [product_id] [int] IDENTITY(1,1) NOT NULL,
    [SKU] [nvarchar](100) NOT NULL,
    [description] [nvarchar](max) NULL,
    [price] [int] NULL,
    [stock] [int] NOT NULL,
    [category_id] [int] NULL,
    [image] [nvarchar](50) NULL,
    [name] [nvarchar](max) NOT NULL,
    [gallery] [nvarchar](max) NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([product_id] ASC),
    FOREIGN KEY ([category_id]) REFERENCES [dbo].[Category]([category_id])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO 

CREATE TABLE [dbo].[Color] (
    [color_id] [int] IDENTITY(1,1) NOT NULL,
    [product_id] [int] NOT NULL,
    [color_name] [nvarchar](50) NOT NULL,
    [color_hex] [nvarchar](7) NOT NULL,
    [image_url] [nvarchar](255) NOT NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([color_id] ASC),
    FOREIGN KEY ([product_id]) REFERENCES [dbo].[Product]([product_id])
) ON [PRIMARY]
GO 

CREATE TABLE [dbo].[Payment] (
    [payment_id] [int] NOT NULL,
    [name] [nvarchar](50) NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([payment_id] ASC)
) ON [PRIMARY]
GO 

CREATE TABLE [dbo].[Order] (
    [order_id] [int] IDENTITY(1,1) NOT NULL,
    [order_date] [date] NOT NULL,
    [total_price] [int] NULL,
    [user_id] [int] NULL,
    [order_status] [int] NULL,
    [address] [nvarchar](max) NULL,
    [payment_id] [int] NULL,
    [phone] [nvarchar](50) NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([order_id] ASC),
    FOREIGN KEY ([user_id]) REFERENCES [dbo].[User]([user_id]),
    FOREIGN KEY ([payment_id]) REFERENCES [dbo].[Payment]([payment_id])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO 

CREATE TABLE [dbo].[CartItem] (
    [cart_item_id] [int] IDENTITY(1,1) NOT NULL,
    [user_id] [int] NOT NULL,
    [product_id] [int] NOT NULL,
    [color_id] [int] NOT NULL,
    [quantity] [int] NOT NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([cart_item_id] ASC),
    FOREIGN KEY ([user_id]) REFERENCES [dbo].[User]([user_id]),
    FOREIGN KEY ([product_id]) REFERENCES [dbo].[Product]([product_id]),
    FOREIGN KEY ([color_id]) REFERENCES [dbo].[Color]([color_id])
) ON [PRIMARY]
GO 

CREATE TABLE [dbo].[Order_item] (
    [order_item_id] [int] IDENTITY(1,1) NOT NULL,
    [quantity] [int] NOT NULL,
    [price] [decimal](18, 2) NOT NULL,
    [product_id] [int] NULL,
    [order_id] [int] NULL,
    [color_id] [int] NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([order_item_id] ASC),
    FOREIGN KEY ([order_id]) REFERENCES [dbo].[Order]([order_id]),
    FOREIGN KEY ([product_id]) REFERENCES [dbo].[Product]([product_id]),
    FOREIGN KEY ([color_id]) REFERENCES [dbo].[Color]([color_id])
) ON [PRIMARY]
GO 

CREATE TABLE [dbo].[Wishlist] (
    [wishlist_id] [int] IDENTITY(1,1) NOT NULL,
    [user_id] [int] NULL,
    [product_id] [int] NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([wishlist_id] ASC),
    FOREIGN KEY ([user_id]) REFERENCES [dbo].[User]([user_id]),
    FOREIGN KEY ([product_id]) REFERENCES [dbo].[Product]([product_id])
) ON [PRIMARY]
GO 

CREATE TABLE [dbo].[Blog] (
    [id] [int] IDENTITY(1,1) NOT NULL,
    [title] [nvarchar](255) NULL,
    [contentBlog] [nvarchar](max) NULL,
    [thumbnail] [nvarchar](50) NULL,
    [date] [nvarchar](50) NULL,
    [author] [nvarchar](50) NULL,
    [RowState] [nvarchar](20) NOT NULL DEFAULT 'Unchanged',
    PRIMARY KEY CLUSTERED ([id] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Blog] ON 

INSERT [dbo].[Blog] ([id], [title], [contentBlog], [thumbnail], [date], [author]) VALUES (1, N'3 phong cách thời trang Cardigan cực kỳ trendy', N'Áo Cardigan là item rất dễ phối đồ, theo đó bạn đều có thể dễ dàng để biến tấu phong cách từ nữ tính nhẹ nhàng, trẻ trung cá tính cho đến độc đáo, sành điệu… Cho dù đó có là bất kỳ phong cách nào cũng không thể làm khó được áo Cardigan.', N'Thum1a.jpg', N'2022/10/12', N'Tung Tam')
INSERT [dbo].[Blog] ([id], [title], [contentBlog], [thumbnail], [date], [author]) VALUES (2, N'5 Items xuống phố mùa thu đông thoải mái, trẻ trung và thanh lịch', N'Mùa thu đông đang ngày càng cận kề kéo theo đó là những cơn gió đầu mùa. Lúc này cho ta cảm giác man mát và se lạnh khi vào buổi sáng và chiều tối, cùng với đó là nắng nóng vào buổi trưa. Vì vậy rất khó để bạn có thể lựa chọn được mình những items phù hợp với thời tiết mà lại thời trang.

Đi đầu cho xu hướng thời trang mới ưu tiên về sự thoải mái ZOFAL gửi đến bạn 5 items xuống phố vô cùng trẻ trung, bắt mắt và thời thường!', N'Thum2a.jpg', N'2023/09/15', N'DM Hieu')
SET IDENTITY_INSERT [dbo].[Blog] OFF
GO
SET IDENTITY_INSERT [dbo].[CartItem] ON 

INSERT [dbo].[CartItem] ([cart_item_id], [user_id], [product_id], [color_id], [quantity]) VALUES (2, 1, 1, 3, 1)
INSERT [dbo].[CartItem] ([cart_item_id], [user_id], [product_id], [color_id], [quantity]) VALUES (3, 1, 1, 2, 1)
INSERT [dbo].[CartItem] ([cart_item_id], [user_id], [product_id], [color_id], [quantity]) VALUES (4, 1, 1, 2, 1)
SET IDENTITY_INSERT [dbo].[CartItem] OFF
GO
SET IDENTITY_INSERT [dbo].[Category] ON 

INSERT [dbo].[Category] ([category_id], [name], [image]) VALUES (1, N'Đồ bộ', N'DB01a.jpg')
INSERT [dbo].[Category] ([category_id], [name], [image]) VALUES (2, N'Áo thun', N'AT01a.jpg')
INSERT [dbo].[Category] ([category_id], [name], [image]) VALUES (3, N'Áo khoác', N'AK01a.jpg')
INSERT [dbo].[Category] ([category_id], [name], [image]) VALUES (4, N'Quần âu', N'QA01a.jpg')
INSERT [dbo].[Category] ([category_id], [name], [image]) VALUES (5, N'Đầm/Váy', N'DV01a.jpg')
INSERT [dbo].[Category] ([category_id], [name], [image]) VALUES (6, N'Quần nữ', N'QN01a.jpg')
SET IDENTITY_INSERT [dbo].[Category] OFF
GO
SET IDENTITY_INSERT [dbo].[Color] ON 

INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (1, 1, N'Da', N'#e2b999', N'DB01a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (2, 1, N'Trắng', N'#c8c8c6', N'DB01b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (3, 1, N'Đen', N'#1a1b1f', N'DB01c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (4, 3, N'Xám', N'#b4b5ba', N'DB02a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (7, 3, N'Đen', N'#1a1b1f', N'DB02b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (8, 3, N'Trắng', N'#c8c8c6', N'DB02c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (9, 4, N'Xanh dương', N'#193282', N'DB03a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (10, 4, N'Trắng', N'#c8c8c6', N'DB03b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (11, 4, N'Đen', N'#1a1b1f', N'DB03c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (12, 5, N'Đen', N'#1a1b1f', N'DB04a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (13, 5, N'Đỏ', N'#5c0317', N'DB04b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (15, 6, N'Đen', N'#1a1b1c', N'DB05a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (16, 6, N'Xanh lá', N'#8e8d78', N'DB05b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (18, 7, N'Trắng', N'#c8c8c6', N'AT01a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (19, 7, N'Đen', N'#1a1b1f', N'AT01b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (20, 7, N'Đỏ', N'#5c0317', N'AT01c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (22, 8, N'Đen', N'#1a1b1f', N'AT02a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (24, 8, N'Trắng', N'#c8c8c6', N'AT02b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (25, 8, N'Nâu', N'#947a69', N'AT02c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (27, 9, N'Đen', N'#1a1b1f', N'AT03a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (28, 9, N'Trắng', N'#c8c8c6', N'AT03b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (29, 9, N'Kem', N'#efe6dd', N'AT03c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (30, 10, N'Đen', N'#1a1b1f', N'AT04a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (31, 10, N'Đỏ', N'#5c0317', N'AT04b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (32, 10, N'Trắng', N'#c8c8c6', N'AT04c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (33, 11, N'Xanh dương', N'#193282', N'AT05a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (34, 11, N'Trắng', N'#c8c8c6', N'AT05b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (35, 11, N'Đen', N'#1a1b1f', N'AT05c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (36, 12, N'Đen', N'#1a1b1f', N'AK01a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (37, 12, N'Đỏ', N'#5c0317', N'AK01b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (38, 12, N'Xanh dương', N'#193282', N'AK01c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (39, 13, N'Đen', N'#1a1b1f', N'AK02a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (40, 13, N'Trắng', N'#c8c8c6', N'AK02b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (41, 13, N'Đỏ', N'#5c0317', N'AK02c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (42, 14, N'Đen', N'#1a1b1f', N'AK03a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (43, 14, N'Xanh dương', N'#193282', N'AK03b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (44, 14, N'Kem', N'#efe6dd', N'AK03c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (45, 15, N'Đen', N'#1a1b1f', N'AK04a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (46, 15, N'Trắng', N'#c8c8c6', N'AK04b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (47, 15, N'Xám', N'#b4b5ba', N'AK04c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (48, 16, N'Đen', N'#1a1b1f', N'AK05a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (49, 16, N'Xám', N'#b4b5ba', N'AK05b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (50, 16, N'Kem', N'#efe6dd', N'AK05c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (51, 17, N'Đen', N'#1a1b1f', N'QA01a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (52, 17, N'Trắng', N'#c8c8c6', N'QA01b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (53, 17, N'Xanh dương', N'#193282', N'QA01c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (54, 18, N'Đen', N'#1a1b1f', N'QA02a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (55, 18, N'Trắng', N'#c8c8c6', N'QA02b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (56, 18, N'Xám', N'#b4b5ba', N'QA02c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (57, 19, N'Đen', N'#1a1b1f', N'QA03a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (58, 19, N'Trắng', N'#c8c8c6', N'QA03b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (59, 19, N'Nâu', N'#947a69', N'QA03c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (60, 20, N'Đen', N'#1a1b1f', N'QA04a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (61, 20, N'Kem', N'#efe6dd', N'QA04b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (62, 20, N'Xám', N'#b4b5ba', N'QA04c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (63, 21, N'Đen', N'#1a1b1f', N'QA05a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (64, 21, N'Trắng', N'#c8c8c6', N'QA05b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (66, 21, N'Nâu', N'#947a69', N'QA05c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (67, 22, N'Trắng', N'#c8c8c6', N'DV01a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (68, 22, N'Đen', N'#1a1b1f', N'DV01b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (69, 22, N'Xanh dương', N'#193282', N'DV01c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (70, 23, N'Xanh lá', N'#8e8d78', N'DV02a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (71, 23, N'Hồng', N'#e8c5c9', N'DV02b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (72, 23, N'Trắng', N'#c8c8c6', N'DV02c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (73, 24, N'Xanh dương', N'#193282', N'DV03a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (74, 24, N'Hồng', N'#e8c5c9', N'DV03b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (75, 24, N'Trắng', N'#c8c8c6', N'DV03c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (76, 25, N'Đen', N'#1a1b1f', N'DV04a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (77, 25, N'Xanh dương', N'#193282', N'DV04b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (78, 25, N'Trắng', N'#c8c8c6', N'DV04c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (79, 26, N'Hồng', N'#e8c5c9', N'DV05a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (80, 26, N'Kem', N'#efe6dd', N'DV05b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (81, 26, N'Trắng', N'#c8c8c6', N'DV05c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (82, 27, N'Đen', N'#1a1b1f', N'QN01a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (83, 27, N'Xanh dương', N'#193282', N'QN01b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (84, 27, N'Trắng', N'#c8c8c6', N'QN01c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (86, 28, N'Đen', N'#1a1b1f', N'QN02a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (87, 28, N'Hồng', N'#e8c5c9', N'QN02b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (88, 28, N'Trắng', N'#c8c8c6', N'QN02c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (90, 29, N'Đen', N'#1a1b1f', N'QN03a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (91, 29, N'Trắng', N'#c8c8c6', N'QN03b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (92, 29, N'Hồng', N'#e8c5c9', N'QN03c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (93, 30, N'Đen', N'#1a1b1f', N'QN04a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (94, 30, N'Trắng', N'#c8c8c6', N'QN04b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (95, 30, N'Hồng', N'#e8c5c9', N'QN04c.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (96, 31, N'Đen', N'#1a1b1f', N'QN05a.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (97, 31, N'Trắng', N'#c8c8c6', N'QN05b.jpg')
INSERT [dbo].[Color] ([color_id], [product_id], [color_name], [color_hex], [image_url]) VALUES (98, 31, N'Nâu', N'#947a69', N'QN05c.jpg')
SET IDENTITY_INSERT [dbo].[Color] OFF

GO
INSERT [dbo].[Payment] ([payment_id], [name]) VALUES (1, N'COD')
INSERT [dbo].[Payment] ([payment_id], [name]) VALUES (2, N'Credit/Debit Card')
INSERT [dbo].[Payment] ([payment_id], [name]) VALUES (3, N'Payment Gateway')
INSERT [dbo].[Payment] ([payment_id], [name]) VALUES (4, N'Digital Wallet')
GO
SET IDENTITY_INSERT [dbo].[Product] ON 

INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (1, N'DB01', N'Bộ đồ nam cao cấp KAPA chất liệu mì chéo được sản xuất tại nhà máy của KAPA với quy trình nghiệm ngặt. Các sản phẩm đều được những người thợ lâu năm trong nghề làm ra một cách cẩn thận và chắc chắn. Chất lượng vải đầu vào được kiểm nghiệm kĩ càng bằng văn bản và giấy tờ nên sản phẩm đầu ra có cam kết chuẩn chất lượng như mô tả!!', 129000, 200, 1, N'DB01a.jpg', N'Bộ quần áo thun nam cộc tay chất liệu polyester cao cấp hàn quốc, Set quần áo nam form dáng trẻ trung', N'DB01a.jpg,DB01b.jpg,DB01c.jpg')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (3, N'DB02', N'Bộ Thể Thao Nam phối trắng cổ tròn tay ngắn vải thun lạnh thoáng mát co giãn chuẩn form

* Chi tiết sản phẩm Áo Thể Thao Nam Coolpass: 

    + Chất Poly thể thao chuyên dụng co giãn, mềm mại, thấm mồ hôi cực tốt, ưu điểm mỏng - nhẹ - thoáng.

    + Thiết kế phối line ở ngực và mạng sườn tạo điểm nhấn thú vị và mới lạ.

    + Form tính toán tỉ mỉ phù hợp với người châu Á, thiết kế cổ tròn, lai thẳng xẻ sườn, logo phản quang đặt tại lai áo và tay áo.

    + Thiết kế khỏe khoắn, trẻ trung, có thể mặc chơi thể thao, tập gym, mặc nhà, đi chơi,...

* Màu sắc và kích cỡ Áo Thể Thao Nam Coolpass: 

    + Áo có 3 màu (đen, xám, be

  Thông số bảng size mang tính chất tương đối. Hãy Chat với chúng tôi, khi quý khách có nhu cầu tư vấn size chuẩn theo độ tuổi, thể trạng, sở thích,...

* Hướng dẫn sử dụng và bảo quản Áo Thể Thao Nam Coolpass: 

    + Bạn có thể giặt vải bằng tay hoặc máy giặt đều được.

    + Không dùng nước quá nóng để giặt.

    + Giặt ở nhiệt độ bình thường, với đồ có màu tương tự.', 159000, 99, 1, N'DB02a.jpg', N'Bộ Thể Thao Nam JULIDO cổ tròn tay ngắn vải waffle thoáng mát co giãn chuẩn form', N'DB02a.jpg,DB02b.jpg,DB02c.jpg')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (4, N'DB03', N'Thông tin sản phẩm
- Hàng Full tag, mác cực sang chảnh (xem video trên ảnh sản phẩm).
- Chất liệu: thun cotton 100%, vải dày, vải mềm, vải mịn, thoáng mát, không xù lông (không nhàu)
- Đường may tỉ mỉ, chắc chắn
- Công dụng: mặc ở nhà, mặc đi chơi, khi vận động thể thao, đi du lịch,...
- Thiết kế hiện đại, trẻ trung, năng động. Dễ phối đồ
- 5 size XS,S, M,L,XL,XXL
SIZE XS < 30KG
SIZE: S < 35KG
SIZE: M  35 - 55KG
SIZE: L 55 - 68KG
SIZE: XL > 68KG
', 139000, 158, 1, N'DB03a.jpg', N'BỘ THỂ THAO NAM DONT STOP TRÀ SỮA, ĐỒ BỘ QUẦN ÁO MÙA HÈ CỘC TAY VẢI ĐẸP HOT TREND 2023 - DUBAI FASHION', N'DB03a.jpg,DB03b.jpg,DB03c.jpg')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (5, N'DB04', N'Bộ Quần Áo Nam Jor Đan In Chữ Paris Siêu Đẹp - Bộ Quần Áo Cộc  Jor Đan Mùa Hè In Chữ Cao Cấp

ĐIỂM NỔI BẬT CỦA SẢN PHẨM:
- Chất Cotton mịn thoáng mát co dãn 2 chiều, thoáng mát, hút ẩm tốt, mềm mịn, dày dặn, thoải mái khi vận động.
- Hàng may kỹ chắc chắn - Thiết kế đơn giãn thanh lịch trẻ đẹp phù hợp mọi lứa tuổi
- Dễ dàng kết hợp với quần ngắn, quần dài... cho bạn trông thật bảnh bao khi dạo phố, đi chơi, học tập, làm việc hay mặc thường ngày ở nhà.

SHOP CAM KẾT
✔ Mang đến cho khách hàng những sản phẩm với chất lượng tốt nhất trong tầm giá.
✔ Chính sách bảo  hành tốt nhất ( Hỗ trợ đổi size, sản phẩm lỗi)
✔ Shop Cam Kết Chất Lượng và Mẫu Mã Giống hình ảnh 100%
✔ Mẫu Mã Đa Dạng ,Cập Nhật Liên Tục, Chất liệu hàng đầu, giá cả hợp lý.
✔ Nhận hàng không ưng hoặc lỗi khách hàng có thể hoàn hàng và được hoàn tiền 100%', 124000, 169, 1, N'DB04a.jpg', N'Bộ Quần Áo Nam Jor Đan In Chữ Paris Siêu Đẹp - Bộ Quần Áo Cộc Jor Đan Mùa Hè In Chữ Cao Cấp', N'DB04a.jpg,DB04b.jpg')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (6, N'DB05', N'Bộ Đồ Nam Tay Ngắn Áo Kiểu Sơ Mi Basic Quần Short Có Túi Kiểu Dáng Trẻ Trung Thời Trang MixxStore QA NAM 071V1

- Chất liệu: Vải kaki mềm mịn 

- Kiểu dáng đơn giản, năng động nam nữ mặc đều đẹp..

- Xuất xứ: Việt Nam

- Bảng size:

+ Size XS: cho bạn có cân nặng từ 40 - 45kg tùy chiều cao 

+ Size S: cho bạn có cân nặng từ dưới 45 - 50kg tùy chiều cao 

+ Size M: cho bạn có cân nặng từ 50 - 55kg tùy chiều cao 

+ Size L: cho bạn có cân nặng từ 55 - 60kg tùy chiều cao 

+ Size XL: cho bạn có cân nặng từ 60 - 65kg tùy chiều cao

🔰 VỚI MIXXSTORE.NO1:

🎗Dịch vụ nhanh chóng

🎗Cập nhập mẫu mã liên tục với giá tốt nhất

🎗Sản phẩm luôn kèm theo video shop quay. ', 99000, 122, 1, N'DB05a.jpg', N'Bộ Đồ Nam Tay Ngắn Áo Kiểu Sơ Mi Basic Quần Short Có Túi Kiểu Dáng Trẻ Trung Thời Trang MixxStore QA NAM 071V1', N'DB05a.jpg,DB05b.jpg')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (7, N'AT01
', N'Áo được thiết kế kiểu dáng áo thun basic, form rộng, tay áo ngắn, cổ tròn đơn giản.
', 90000, 119, 2, N'AT01a.jpg
', N'Áo thun nam nữ unisex Dickies basic in ngực, chất cotton 100% chính hãng - Helistore
', N'AT01a.jpg
,AT01b.jpg
,AT01c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (8, N'AT02
', N'Vải waffle co giãn với độ dày vừa phải, thoáng mát, không xù lông, độ bền màu cao.
', 180000, 200, 2, N'AT02a.jpg
', N'Áo thun oversize nam nữ cao cấp vải waffle hàng hiệu form rộng PUNDO ATPD112
', N'AT02a.jpg
,AT02b.jpg
,AT02c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (9, N'AT03
', N'Form áo được thiết kế theo tiêu chuẩn tương đối của người Việt Nam. Form oversize nên không cần nhảy size lớn hơn
', 200000, 100, 2, N'AT03a.jpg
', N'Áo Thun Unisex Local Brand Lourents Signature Tee - TEE1
', N'AT03a.jpg
,AT03b.jpg
,AT03c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (10, N'AT04
', N'Chất vải cotton mềm mịn, giúp giữ form áo sau nhiều lần giặt và thấm hút mồ hôi ở mức độ trung bình
', 190000, 158, 2, N'AT04a.jpg
', N'Áo Thun Gimme Tee Oversize - Chữ GM In Lụa Unisex Cotton - 270gsm - GMT36
', N'AT04a.jpg
,AT04b.jpg
,AT04c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (11, N'AT05
', N'Được chăm chút từ chất liệu, form dáng, đường may, hình in cho đến khâu đóng gói và hậu mãi, chiếc áo cao cấp này sẽ làm hài lòng cả những vị khách khó tính nhất
', 170000, 169, 2, N'AT05a.jpg
', N'Áo thun Local Brand Lavi Studio/ Shark
', N'AT05a.jpg
,AT05b.jpg
,AT05c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (12, N'AK01
', N'Chất liệu: Vải dù Symbolic Premium poli dày dặn, có lớp lót dù
', 230000, 130, 3, N'AK01a.jpg
', N'Áo Khoác Bomber Pilot Oversized Jacket Symbolic
', N'AK01a.jpg
,AK01b.jpg
,AK01c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (13, N'AK02
', N'Áo khoác Jacket Uranus mang kiểu dáng unisex, form rộng dễ mặc, dễ dàng kết hợp với các sản phẩm quần jean, quần jogger,... 
', 285000, 148, 3, N'AK02a.jpg
', N'Áo Khoác Gió Local Brand Uranus City Cycle oversize nam nữ form rộng chống nước
', N'AK02a.jpg
,AK02b.jpg
,AK02c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (14, N'AK03
', N'Áo khoác nỉ bông mịn bông dày mặc mùa đông ấm áp, áo có các chi tiết  thêu sắc nét, tag tay tag sườn, tag cổ, có bo chun ở tay, nón to trùm đầu
', 300000, 100, 3, N'AK03a.jpg
', N'Áo Khoác Gió Today Viền chỉ tay form unisex nam nữ cực ngầu
', N'AK03a.jpg
,AK03b.jpg
,AK03c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (15, N'AK04
', N'Chất liệu: Nhung Thun 100% cao cấp, bề mặt vải mịn, không xù, không gião
', 250000, 120, 3, N'AK04a.jpg
', N'Áo Khoác Bomber Varsity Gooan Infinity Form Rộng
', N'AK04a.jpg
,AK04b.jpg
,AK04c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (16, N'AK05
', N'Chất liệu da, dù, nỉ dạ basic Jacket Classy
', 279000, 200, 3, N'AK05a.jpg
', N'Áo khoác bomber croptop chống nước DAVIES local brand nam, nữ
', N'AK05a.jpg
,AK05b.jpg
,AK05c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (17, N'QA01
', N'Quần tây JA0101 được căn chỉnh form dáng chuẩn xác hơn với phần hông rộng, đũng sâu và ống ôm suông dần
', 149000, 140, 4, N'QA01a.jpg
', N'Quần tây nam hàn quốc JBagy dáng baggy vải co giãn dày dặn dáng suông ống rộng, màu đen, kem JA0101
', N'QA01a.jpg
,QA01b.jpg
,QA01c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (18, N'QA02
', N'Quần tây nam ống suông cạp chun có túi thật,Quần âu nam vải cotton hàn co dãn nhẹ thoáng khí HK1970
', 287000, 70, 4, N'QA02a.jpg
', N'Quần tây nam ống suông cạp chun có túi thật, Quần âu nam vải cotton hàn co dãn nhẹ thoáng khí Q087
', N'QA02a.jpg
,QA02b.jpg
,QA02c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (19, N'QA03
', N'Thiết kế : quần tây nam mẫu trơn, đường may gấu quần là đường may chìm
', 300000, 260, 4, N'QA03a.jpg
', N'Quần âu nam cạp 3 lớp cao cấp 360Boutique quần tây dài form slimcrop vải dày dặn chống nhăn-QACOL420
', N'QA03a.jpg
,QA03b.jpg
,QA03c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (20, N'QA04
', N'Chất Tuyết Hàn cao cấp dày dặn, form Suông trẻ trung, thanh lịch, tôn dáng QAR12CT
', 150000, 187, 4, N'QA04a.jpg
', N'Quần âu nam suông WHY NOT thiết kế cạp nửa chun đằng sau, chất Tuyết Hàn cao cấp dày dặn, form Suông trẻ trung QAR12CT
', N'QA04a.jpg
,QA04b.jpg
,QA04c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (21, N'QA05
', N'Quần âu nam ống rộng JONATO,  quần nam ống suông phong cách hàn quốc trẻ trung năng động
', 129000, 200, 4, N'QA05a.jpg
', N'Quần âu nam ống rộng JONATO, quần nam ống suông phong cách hàn quốc trẻ trung năng động
', N'QA05a.jpg
,QA05b.jpg
,QA05c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (22, N'DV01
', N'Sản phẩm 100% giống mô tả. Hình ảnh sản phẩm là ảnh thật do shop tự chụp và giữ bản quyền hình ảnh
', 79000, 122, 5, N'DV01a.jpg
', N'Váy cúp ngực, đầm 2 dây Lou dress xixeoshop - v458
', N'DV01a.jpg
,DV01b.jpg
,DV01c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (23, N'DV02
', N'Chất liệu vải nhẹ, mềm mại cùng đường may tỉ mỉ tạo nên chiếc đầm không chỉ đẹp mắt mà còn rất thoải mái khi mặc.
', 119000, 69, 5, N'DV02a.jpg
', N'Đầm suông dáng dài, Váy nữ công sở xinh nhẹ nhàng nữ tính ANN SARA
', N'DV02a.jpg
,DV02b.jpg
,DV02c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (24, N'DV03
', N'Sản phẩm cao cấp được shop chọn tỉ mỉ. Chất vải, đường may và form dáng đều được may kĩ lưỡng, xứng đáng với giá tiền
', 110000, 110, 5, N'DV03a.jpg
', N'Váy Đầm Dài Hai Dây Xếp Ly Thiết Kế Màu Trơn Váy Chữ A Chiết Eo Tôn Dáng Đầu Xuân Hè 2024
', N'DV03a.jpg
,DV03b.jpg
,DV03c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (25, N'DV04', N'Thiết kế đơn giản dễ mặc và sự kết hợp của chất vải co giản, thấm hút mồ hôi tốt.
', 60000, 190, 5, N'DV04a.jpg
', N'Đầm Cổ Vest Dự Tiệc Vạt Chéo Xếp Ly Cao Cấp
', N'DV04a.jpg
,DV04b.jpg
,DV04c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (26, N'DV05
', N'Hàng Thiết Kế Chất Liệu Vải Cao Cấp Đính Nơ Cổ, Đuôi Váy Xếp Ly Đẹp Sang Trọng 
', 89000, 210, 5, N'DV05a.jpg
', N'Váy Trắng Tiểu Thư KANGSOOSTORE , Hàng Thiết Kế Đính Nơ Cổ, Đuôi Váy Xếp Ly
', N'DV05a.jpg
,DV05b.jpg
,DV05c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (27, N'QN01
', N'Chi tiết bấu ly lật cạp bản to làm điểm nhấn chính thanh lịch phù hợp công sở, dạo phố, sản phẩm mặc tôn dáng dễ mix match các items
', 200000, 117, 6, N'QN01a.jpg
', N'Quần Ống Rộng CCHAT Ly Cạp Cao Chất Tuytsi Cao Cấp Sang Trọng
', N'QN01a.jpg
,QN01b.jpg
,QN01c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (28, N'QN02
', N'Quần kaki ống suông nữ rộng cạp chun dây thắt phong cách Avocado 2 túi hộp phong cách trẻ trung năng động
', 180000, 60, 6, N'QN02a.jpg
', N'Quần kaki ống suông nữ rộng cạp chun dây thắt phong cách Avocado 2 túi hộp phong cách trẻ trung năng động
', N'QN02a.jpg
,QN02b.jpg
,QN02c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (29, N'QN03
', N'Nhẹ thoáng mát cạp chun co giãn thoải mai'' không nhăn không xù không nhão
', 270000, 126, 6, N'QN03a.jpg
', N'Quần Ống Rộng Nữ Vải Đũi Mát Loại Đẹp Siêu Hót
', N'QN03a.jpg
,QN03b.jpg
,QN03c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (30, N'QN04
', N'Ương đối, phù hợp với 80-90% khách hàng. Các bạn iu có thể nhắn tin cho EMPTI chiều cao & cân nặng 
', 125000, 89, 6, N'QN04a.jpg
', N'QUẦN DÙ DÂY RÚT PARACHUTE TRƠN 10 MÀU
', N'QN04a.jpg
,QN04b.jpg
,QN04c.jpg
')
INSERT [dbo].[Product] ([product_id], [SKU], [description], [price], [stock], [category_id], [image], [name], [gallery]) VALUES (31, N'QN05
', N'Quần jean ống rộng nữ , quần ống suông nữ màu đen trắng kem kaki jean form basic chất bò trẻ trung năng động
', 119000, 131, 6, N'QN05a.jpg
', N'Quần jean ống rộng nữ , quần ống suông nữ
', N'QN05a.jpg
,QN05b.jpg
,QN05c.jpg
')
SET IDENTITY_INSERT [dbo].[Product] OFF
GO
SET IDENTITY_INSERT [dbo].[User] ON 

INSERT [dbo].[User] ([user_id], [first_name], [last_name], [email], [username], [password], [address], [phone_number], [permission]) VALUES (1, N'Nguyễn Trung ', N'Truong', N'trungtruong3110@gmail.com', N'admin', N'e10adc3949ba59abbe56e057f20f883e', N'Đô Lương,Nghệ An', N'0123456789', 1)
INSERT [dbo].[User] ([user_id], [first_name], [last_name], [email], [username], [password], [address], [phone_number], [permission]) VALUES (2, N'Nguyễn Trung ', N'Truong', N'trungtruong3110@gmail.com', N'truongnt', N'e10adc3949ba59abbe56e057f20f883e', N'Đô Lương,Nghệ An', N'0123456789', 0)
INSERT [dbo].[User] ([user_id], [first_name], [last_name], [email], [username], [password], [address], [phone_number], [permission]) VALUES (3, N'Nguyễn Văn', N'A', N'an@gmail.com', N'anv', N'e10adc3949ba59abbe56e057f20f883e', N'Thị trấn Chờ,Yên Phong, Bắc Ninh', N'0743591540', 0)
SET IDENTITY_INSERT [dbo].[User] OFF
GO
SET IDENTITY_INSERT [dbo].[Wishlist] ON 

INSERT [dbo].[Wishlist] ([wishlist_id], [user_id], [product_id]) VALUES (9, 2, 3)
INSERT [dbo].[Wishlist] ([wishlist_id], [user_id], [product_id]) VALUES (11, 2, 9)
INSERT [dbo].[Wishlist] ([wishlist_id], [user_id], [product_id]) VALUES (12, 2, 6)
INSERT [dbo].[Wishlist] ([wishlist_id], [user_id], [product_id]) VALUES (13, 2, 12)
INSERT [dbo].[Wishlist] ([wishlist_id], [user_id], [product_id]) VALUES (14, 2, 26)
INSERT [dbo].[Wishlist] ([wishlist_id], [user_id], [product_id]) VALUES (15, 2, 19)
INSERT [dbo].[Wishlist] ([wishlist_id], [user_id], [product_id]) VALUES (16, 2, 1)
SET IDENTITY_INSERT [dbo].[Wishlist] OFF
GO
