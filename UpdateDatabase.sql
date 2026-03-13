-- تحديث جدول المستخدمين لإضافة الصلاحيات وتغيير نوع كلمة المرور
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Role')
BEGIN
    ALTER TABLE Users ADD Role NVARCHAR(20) DEFAULT 'User';
END
GO

-- تحديث كلمة المرور لتكون مشفرة (SHA256 ينتج 64 حرفاً)
ALTER TABLE Users ALTER COLUMN Password NVARCHAR(64);
GO

-- إنشاء جدول سجل الحركات
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('AuditLogs') AND type = 'U')
BEGIN
    CREATE TABLE AuditLogs (
        LogID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT,
        Action NVARCHAR(MAX),
        ActionDate DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (UserID) REFERENCES Users(UserID)
    );
END
GO

-- تحديث جدول طلبات الخدمة لربطها بـ UserID وإضافة المرفقات
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ServiceRequests') AND name = 'CreatedBy')
BEGIN
    ALTER TABLE ServiceRequests ADD CreatedBy INT;
    ALTER TABLE ServiceRequests ADD AttachmentPath NVARCHAR(MAX);
END
GO

-- إضافة مستخدم مدير افتراضي (كلمة المرور المشفرة لـ 'admin123' بـ SHA256)
-- ملاحظة: ستحتاج لتحديث كلمات المرور الحالية يدوياً أو عبر التطبيق
IF NOT EXISTS (SELECT * FROM Users WHERE UserName = 'admin')
BEGIN
    INSERT INTO Users (UserName, Password, Role) 
    VALUES ('admin', '240be518fabd2724ddb6f0403f3d5d2f3c22c51000b918d55852244d8e131031', 'Admin');
END
GO
