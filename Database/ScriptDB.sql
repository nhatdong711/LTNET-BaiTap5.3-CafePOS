/* =========================================================
   BÀI TẬP 5.3 - HỆ THỐNG QUẢN LÝ QUÁN CAFE & BÁN HÀNG (POS)
   Script tạo CSDL CafePOSDB + dữ liệu mẫu
   Chạy toàn bộ file này trong SQL Server Management Studio
   ========================================================= */
USE master;
GO
IF DB_ID(N'CafePOSDB') IS NOT NULL
BEGIN
    ALTER DATABASE CafePOSDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CafePOSDB;
END
GO
CREATE DATABASE CafePOSDB;
GO
USE CafePOSDB;
GO

-- 1. Bàn ăn
CREATE TABLE BanAn (
    MaBan     INT IDENTITY(1,1) PRIMARY KEY,
    TenBan    NVARCHAR(50) NOT NULL,
    TrangThai NVARCHAR(20) NOT NULL DEFAULT N'Trống'
        CONSTRAINT CK_BanAn_TrangThai CHECK (TrangThai IN (N'Trống', N'Có khách'))
);

-- 2. Danh mục món
CREATE TABLE DanhMuc (
    MaDanhMuc  INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL
);

-- 3. Thực đơn món
CREATE TABLE MonAn (
    MaMon     INT IDENTITY(1,1) PRIMARY KEY,
    TenMon    NVARCHAR(100) NOT NULL,
    MaDanhMuc INT NOT NULL FOREIGN KEY REFERENCES DanhMuc(MaDanhMuc),
    DonGia    DECIMAL(18,2) NOT NULL CONSTRAINT CK_MonAn_DonGia CHECK (DonGia >= 0)
);

-- 4. Hóa đơn
CREATE TABLE HoaDon (
    MaHoaDon  INT IDENTITY(1,1) PRIMARY KEY,
    MaBan     INT NOT NULL FOREIGN KEY REFERENCES BanAn(MaBan),
    GioVao    DATETIME NOT NULL DEFAULT GETDATE(),
    GioRa     DATETIME NULL,
    TongTien  DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai BIT NOT NULL DEFAULT 0      -- 0: Chưa thanh toán, 1: Đã thanh toán
);

-- 5. Chi tiết hóa đơn
CREATE TABLE ChiTietHoaDon (
    MaHoaDon INT NOT NULL FOREIGN KEY REFERENCES HoaDon(MaHoaDon),
    MaMon    INT NOT NULL FOREIGN KEY REFERENCES MonAn(MaMon),
    SoLuong  INT NOT NULL CHECK (SoLuong > 0),
    DonGia   DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (MaHoaDon, MaMon)
);
GO

/* ---------------- DỮ LIỆU MẪU ---------------- */
INSERT INTO BanAn (TenBan) VALUES
(N'Bàn 01'), (N'Bàn 02'), (N'Bàn 03'), (N'Bàn 04'), (N'Bàn 05'), (N'Bàn 06'),
(N'Bàn 07'), (N'Bàn 08'), (N'Bàn 09'), (N'Bàn 10'), (N'Bàn 11'), (N'Bàn 12');

INSERT INTO DanhMuc (TenDanhMuc) VALUES (N'Cà phê'), (N'Trà sữa'), (N'Sinh tố - Nước ép'), (N'Đồ ăn nhẹ');

INSERT INTO MonAn (TenMon, MaDanhMuc, DonGia) VALUES
(N'Cà phê đen đá', 1, 25000), (N'Cà phê sữa đá', 1, 29000), (N'Bạc xỉu', 1, 32000), (N'Cappuccino', 1, 45000),
(N'Trà sữa trân châu', 2, 35000), (N'Trà sữa matcha', 2, 39000), (N'Trà đào cam sả', 2, 42000),
(N'Sinh tố bơ', 3, 45000), (N'Nước ép cam', 3, 35000), (N'Nước ép dưa hấu', 3, 30000),
(N'Hướng dương', 4, 15000), (N'Bánh tiramisu', 4, 40000), (N'Khoai tây chiên', 4, 30000);

-- Bàn 02 đang có khách (hóa đơn chưa thanh toán)
INSERT INTO HoaDon (MaBan, GioVao, TrangThai) VALUES (2, DATEADD(MINUTE, -40, GETDATE()), 0);
INSERT INTO ChiTietHoaDon VALUES (1, 2, 2, 29000), (1, 11, 1, 15000);
UPDATE BanAn SET TrangThai = N'Có khách' WHERE MaBan = 2;

-- Một số hóa đơn đã thanh toán (phục vụ báo cáo doanh thu)
INSERT INTO HoaDon (MaBan, GioVao, GioRa, TongTien, TrangThai) VALUES
(1, DATEADD(DAY, -1, GETDATE()), DATEADD(DAY, -1, DATEADD(HOUR, 1, GETDATE())), 102000, 1),
(3, DATEADD(HOUR, -5, GETDATE()), DATEADD(HOUR, -4, GETDATE()), 80000, 1);
INSERT INTO ChiTietHoaDon VALUES (2, 4, 1, 45000), (2, 7, 1, 42000), (2, 11, 1, 15000);
INSERT INTO ChiTietHoaDon VALUES (3, 5, 1, 35000), (3, 8, 1, 45000);
GO

SELECT * FROM BanAn;
SELECT * FROM MonAn;
