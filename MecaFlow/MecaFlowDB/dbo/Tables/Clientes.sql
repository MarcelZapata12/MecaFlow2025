CREATE TABLE [dbo].[Clientes] (
    [ClienteId]     INT            IDENTITY (1, 1) NOT NULL,
    [Nombre]        NVARCHAR (100) NOT NULL,
    [Correo]        NVARCHAR (100) NULL,
    [Telefono]      NVARCHAR (20)  NULL,
    [Direccion]     NVARCHAR (200) NULL,
    [FechaRegistro] DATETIME       DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([ClienteId] ASC)
);

