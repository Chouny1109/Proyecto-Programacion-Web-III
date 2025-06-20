USE ProyectoPizarra;
GO

-- ADD Trazos(GrupoTrazoId)
ALTER TABLE Trazos
ADD GrupoTrazoId UNIQUEIDENTIFIER NULL;

-- Tabla Mensaje
CREATE TABLE Mensaje (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PizarraId UNIQUEIDENTIFIER NOT NULL,
    UsuarioId NVARCHAR(450) NOT NULL,
    NombreUsuario NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(MAX) NOT NULL,
    FechaPublicacion DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Mensaje_Pizarra FOREIGN KEY (PizarraId)
        REFERENCES Pizarra(Id) ON DELETE CASCADE,

    CONSTRAINT FK_Mensaje_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Mensaje_Pizarra_Usuario_Fecha
ON Mensaje (PizarraId, UsuarioId, FechaPublicacion);
GO

-- Tabla MensajeVisto
CREATE TABLE MensajeVisto (
    MensajeId INT NOT NULL,
    UsuarioId NVARCHAR(450) NOT NULL,
    FechaVisto DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_MensajeVisto PRIMARY KEY NONCLUSTERED (MensajeId, UsuarioId),

    CONSTRAINT FK_MensajeVisto_Mensaje FOREIGN KEY (MensajeId)
        REFERENCES Mensaje(Id) ON DELETE CASCADE,

    CONSTRAINT FK_MensajeVisto_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES AspNetUsers(Id) ON DELETE NO ACTION
);
GO

-- Tabla RolEnPizarra
CREATE TABLE RolEnPizarra (
    Id INT PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO RolEnPizarra (Id, Nombre)
VALUES 
    (1, 'Escritura'),
    (2, 'Lectura'),
    (3, 'Admin');
GO

-- UPDATE InvitacionPizarra(RolId)                                
ALTER TABLE dbo.InvitacionPizarra DROP CONSTRAINT DF__InvitacionP__Rol__628FA481;  -- <-----------------   aca tiraba error y actualice esto 628FA481

ALTER TABLE dbo.InvitacionPizarra DROP COLUMN Rol;

ALTER TABLE dbo.InvitacionPizarra ADD RolId INT NOT NULL;

ALTER TABLE dbo.InvitacionPizarra
ADD CONSTRAINT FK_InvitacionPizarra_RolEnPizarra
FOREIGN KEY (RolId) REFERENCES dbo.RolEnPizarra(Id);
GO

-- UPDATE PizarraUsuarios(RolId)
ALTER TABLE dbo.PizarraUsuarios DROP COLUMN Rol;

ALTER TABLE dbo.PizarraUsuarios ADD RolId INT NOT NULL;

ALTER TABLE dbo.PizarraUsuarios
ADD CONSTRAINT FK_PizarraUsuarios_RolEnPizarra
FOREIGN KEY (RolId) REFERENCES dbo.RolEnPizarra(Id);
GO

-- Tabla Notificacion
CREATE TABLE Notificacion (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    Titulo NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL
);

-- Tabla NotificacionUsuario
CREATE TABLE NotificacionUsuario (
    Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    NotificacionId UNIQUEIDENTIFIER NOT NULL,
    UsuarioId NVARCHAR(450) NOT NULL,
    FueVista BIT NOT NULL DEFAULT 0, 

    CONSTRAINT FK_NotificacionUsuario_Notificacion FOREIGN KEY (NotificacionId)
        REFERENCES Notificacion(Id),

    CONSTRAINT FK_NotificacionUsuario_Usuario FOREIGN KEY (UsuarioId)
        REFERENCES AspNetUsers(Id),

    CONSTRAINT UQ_Notificacion_Usuario UNIQUE (NotificacionId, UsuarioId)
);
