IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [IsSystemRole] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(max) NOT NULL,
        [LastName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [LastLoginDate] datetime2 NULL,
        [RefreshToken] nvarchar(max) NULL,
        [RefreshTokenExpiryTime] datetime2 NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [Menus] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(max) NOT NULL,
        [Icon] nvarchar(max) NULL,
        [Url] nvarchar(max) NULL,
        [ParentId] int NULL,
        [Order] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Menus] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Menus_Menus_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [Menus] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Code] nvarchar(max) NOT NULL,
        [Group] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [Action] nvarchar(max) NOT NULL,
        [EntityName] nvarchar(max) NOT NULL,
        [EntityId] nvarchar(max) NOT NULL,
        [OldValues] nvarchar(max) NOT NULL,
        [NewValues] nvarchar(max) NOT NULL,
        [IpAddress] nvarchar(max) NULL,
        [UserAgent] nvarchar(max) NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AuditLogs_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] int NOT NULL IDENTITY,
        [Token] nvarchar(max) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Expires] datetime2 NOT NULL,
        [Created] datetime2 NOT NULL,
        [CreatedByIp] nvarchar(max) NOT NULL,
        [Revoked] datetime2 NULL,
        [RevokedByIp] nvarchar(max) NULL,
        [ReplacedByToken] nvarchar(max) NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [RoleMenus] (
        [RoleId] nvarchar(450) NOT NULL,
        [MenuId] int NOT NULL,
        CONSTRAINT [PK_RoleMenus] PRIMARY KEY ([RoleId], [MenuId]),
        CONSTRAINT [FK_RoleMenus_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RoleMenus_Menus_MenuId] FOREIGN KEY ([MenuId]) REFERENCES [Menus] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE TABLE [RolePermissions] (
        [RoleId] nvarchar(450) NOT NULL,
        [PermissionId] int NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
        CONSTRAINT [FK_RolePermissions_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'CreatedDate', N'Description', N'IsSystemRole', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] ON;
    EXEC(N'INSERT INTO [AspNetRoles] ([Id], [ConcurrencyStamp], [CreatedDate], [Description], [IsSystemRole], [Name], [NormalizedName])
    VALUES (N''3100a53a-8f03-48ac-887d-4fe91ae2ca2a'', NULL, ''2026-01-21T07:48:52.7530590Z'', N''Regular User'', CAST(1 AS bit), N''User'', N''USER''),
    (N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8'', NULL, ''2026-01-21T07:48:52.7530452Z'', N''System Administrator'', CAST(1 AS bit), N''Admin'', N''ADMIN'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'CreatedDate', N'Description', N'IsSystemRole', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[AspNetRoles]'))
        SET IDENTITY_INSERT [AspNetRoles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedDate', N'Icon', N'IsActive', N'Order', N'ParentId', N'Title', N'Url') AND [object_id] = OBJECT_ID(N'[Menus]'))
        SET IDENTITY_INSERT [Menus] ON;
    EXEC(N'INSERT INTO [Menus] ([Id], [CreatedDate], [Icon], [IsActive], [Order], [ParentId], [Title], [Url])
    VALUES (1, ''2026-01-21T07:48:52.7533452Z'', N''dashboard'', CAST(1 AS bit), 1, NULL, N''Dashboard'', N''/dashboard''),
    (2, ''2026-01-21T07:48:52.7533545Z'', N''admin_panel_settings'', CAST(1 AS bit), 2, NULL, N''Administration'', N''#''),
    (8, ''2026-01-21T07:48:52.7533635Z'', N''person'', CAST(1 AS bit), 3, NULL, N''Profile'', N''/profile''),
    (9, ''2026-01-21T07:48:52.7533636Z'', N''settings'', CAST(1 AS bit), 4, NULL, N''Settings'', N''/settings'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedDate', N'Icon', N'IsActive', N'Order', N'ParentId', N'Title', N'Url') AND [object_id] = OBJECT_ID(N'[Menus]'))
        SET IDENTITY_INSERT [Menus] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedDate', N'Description', N'Group', N'Name') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] ON;
    EXEC(N'INSERT INTO [Permissions] ([Id], [Code], [CreatedDate], [Description], [Group], [Name])
    VALUES (1, N''users.view'', ''2026-01-21T07:48:52.7525252Z'', N''Can view users'', N''Users'', N''View Users''),
    (2, N''users.create'', ''2026-01-21T07:48:52.7525822Z'', N''Can create users'', N''Users'', N''Create Users''),
    (3, N''users.edit'', ''2026-01-21T07:48:52.7525824Z'', N''Can edit users'', N''Users'', N''Edit Users''),
    (4, N''users.delete'', ''2026-01-21T07:48:52.7525825Z'', N''Can delete users'', N''Users'', N''Delete Users''),
    (5, N''roles.view'', ''2026-01-21T07:48:52.7525826Z'', N''Can view roles'', N''Roles'', N''View Roles''),
    (6, N''roles.create'', ''2026-01-21T07:48:52.7525830Z'', N''Can create roles'', N''Roles'', N''Create Roles''),
    (7, N''roles.edit'', ''2026-01-21T07:48:52.7525831Z'', N''Can edit roles'', N''Roles'', N''Edit Roles''),
    (8, N''roles.delete'', ''2026-01-21T07:48:52.7525832Z'', N''Can delete roles'', N''Roles'', N''Delete Roles''),
    (9, N''permissions.view'', ''2026-01-21T07:48:52.7525833Z'', N''Can view permissions'', N''Permissions'', N''View Permissions''),
    (10, N''permissions.manage'', ''2026-01-21T07:48:52.7525835Z'', N''Can manage permissions'', N''Permissions'', N''Manage Permissions''),
    (11, N''menus.view'', ''2026-01-21T07:48:52.7525836Z'', N''Can view menus'', N''Menus'', N''View Menus''),
    (12, N''menus.manage'', ''2026-01-21T07:48:52.7525837Z'', N''Can manage menus'', N''Menus'', N''Manage Menus''),
    (13, N''audit.view'', ''2026-01-21T07:48:52.7525838Z'', N''Can view audit logs'', N''Audit'', N''View Audit Logs'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedDate', N'Description', N'Group', N'Name') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedDate', N'Icon', N'IsActive', N'Order', N'ParentId', N'Title', N'Url') AND [object_id] = OBJECT_ID(N'[Menus]'))
        SET IDENTITY_INSERT [Menus] ON;
    EXEC(N'INSERT INTO [Menus] ([Id], [CreatedDate], [Icon], [IsActive], [Order], [ParentId], [Title], [Url])
    VALUES (3, ''2026-01-21T07:48:52.7533624Z'', N''people'', CAST(1 AS bit), 1, 2, N''Users'', N''/admin/users''),
    (4, ''2026-01-21T07:48:52.7533626Z'', N''assignment_ind'', CAST(1 AS bit), 2, 2, N''Roles'', N''/admin/roles''),
    (5, ''2026-01-21T07:48:52.7533628Z'', N''security'', CAST(1 AS bit), 3, 2, N''Permissions'', N''/admin/permissions''),
    (6, ''2026-01-21T07:48:52.7533631Z'', N''menu'', CAST(1 AS bit), 4, 2, N''Menus'', N''/admin/menus''),
    (7, ''2026-01-21T07:48:52.7533633Z'', N''history'', CAST(1 AS bit), 5, 2, N''Audit Logs'', N''/admin/audit-logs'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedDate', N'Icon', N'IsActive', N'Order', N'ParentId', N'Title', N'Url') AND [object_id] = OBJECT_ID(N'[Menus]'))
        SET IDENTITY_INSERT [Menus] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MenuId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RoleMenus]'))
        SET IDENTITY_INSERT [RoleMenus] ON;
    EXEC(N'INSERT INTO [RoleMenus] ([MenuId], [RoleId])
    VALUES (1, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (2, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (8, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (9, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MenuId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RoleMenus]'))
        SET IDENTITY_INSERT [RoleMenus] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
        SET IDENTITY_INSERT [RolePermissions] ON;
    EXEC(N'INSERT INTO [RolePermissions] ([PermissionId], [RoleId])
    VALUES (1, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (2, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (3, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (4, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (5, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (6, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (7, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (8, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (9, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (10, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (11, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (12, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (13, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
        SET IDENTITY_INSERT [RolePermissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MenuId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RoleMenus]'))
        SET IDENTITY_INSERT [RoleMenus] ON;
    EXEC(N'INSERT INTO [RoleMenus] ([MenuId], [RoleId])
    VALUES (3, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (4, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (5, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (6, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8''),
    (7, N''59e429e4-ddd3-49ed-b397-9fff4d13f4f8'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MenuId', N'RoleId') AND [object_id] = OBJECT_ID(N'[RoleMenus]'))
        SET IDENTITY_INSERT [RoleMenus] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Menus_ParentId] ON [Menus] ([ParentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RoleMenus_MenuId] ON [RoleMenus] ([MenuId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260121074853_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260121074853_InitialCreate', N'9.0.0');
END;

COMMIT;
GO

