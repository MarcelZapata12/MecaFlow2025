CREATE TABLE [dbo].[Pagos] (
    [PagoId]     INT           IDENTITY (1, 1) NOT NULL,
    [FacturaId]  INT           NOT NULL,
    [FechaPago]  DATE          NOT NULL,
    [MetodoPago] NVARCHAR (50) NULL,
    PRIMARY KEY CLUSTERED ([PagoId] ASC),
    CHECK ([MetodoPago]='Tarjeta' OR [MetodoPago]='Efectivo'),
    FOREIGN KEY ([FacturaId]) REFERENCES [dbo].[Facturas] ([FacturaId])
);

