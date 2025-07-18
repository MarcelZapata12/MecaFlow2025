-- 1. Crear tabla Marcas
CREATE TABLE Marcas (
    MarcaId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL
);

-- 3. Agregar columna MarcaId a Vehiculos (si no existe)
ALTER TABLE Vehiculos
ADD MarcaId INT;

-- 4. Establecer relación de clave foránea
ALTER TABLE Vehiculos
ADD CONSTRAINT FK_Vehiculos_Marcas
FOREIGN KEY (MarcaId) REFERENCES Marcas(MarcaId);


-- 5. Eliminar Marca de la tabla Vehiculos
ALTER TABLE Vehiculos DROP COLUMN Marca;	

-- Crear la tabla Modelos
CREATE TABLE Modelos (
    ModeloId INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL,
    MarcaId INT NOT NULL
);

-- Crear clave foránea hacia Marcas
ALTER TABLE Modelos
ADD CONSTRAINT FK_Modelos_Marcas
FOREIGN KEY (MarcaId) REFERENCES Marcas(MarcaId);


ALTER TABLE Vehiculos
ADD ModeloId INT;

ALTER TABLE Vehiculos
ADD CONSTRAINT FK_Vehiculos_Modelos
FOREIGN KEY (ModeloId) REFERENCES Modelos(ModeloId);


-- Reinicia para que el próximo valor sea 1
DBCC CHECKIDENT ('Marcas', RESEED, 0);

-- Inserción de las 40 marcas principales
INSERT INTO Marcas (Nombre) VALUES
('Toyota'),
('Hyundai'),
('Nissan'),
('Kia'),
('Suzuki'),
('Honda'),
('Mazda'),
('Ford'),
('Subaru'),
('Volkswagen'),
('Peugeot'),
('Citroën'),
('Fiat'),
('Jeep'),
('Chevrolet'),
('Cadillac'),
('GMC'),
('RAM'),
('BYD'),
('SAIC'),
('BMW'),
('Mercedes-Benz'),
('Renault'),
('Tesla'),
('Geely'),
('Chery'),
('Mitsubishi'),
('Volvo'),
('Audi'),
('Porsche'),
('Land Rover'),
('Jaguar'),
('Lexus'),
('Acura'),
('Infiniti'),
('Mini'),
('Alfa Romeo'),
('Lincoln'),
('Genesis');


--Inyeccion de Modelos 
DECLARE @toyotaId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Toyota');
DECLARE @hyundaiId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Hyundai');
DECLARE @nissanId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Nissan');
DECLARE @kiaId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Kia');
DECLARE @suzukiId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Suzuki');
DECLARE @hondaId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Honda');
DECLARE @mazdaId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Mazda');
DECLARE @fordId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Ford');
DECLARE @subaruId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Subaru');
DECLARE @volkswagenId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Volkswagen');
DECLARE @peugeotId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Peugeot');
DECLARE @citroënId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Citroën');
DECLARE @fiatId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Fiat');
DECLARE @jeepId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Jeep');
DECLARE @chevroletId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Chevrolet');
DECLARE @cadillacId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Cadillac');
DECLARE @gmcId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='GMC');
DECLARE @ramId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='RAM');
DECLARE @bydId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='BYD');
DECLARE @saicId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='SAIC');
DECLARE @bmwId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='BMW');
DECLARE @mercedesbenzId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Mercedes-Benz');
DECLARE @renaultId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Renault');
DECLARE @teslaId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Tesla');
DECLARE @geelyId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Geely');
DECLARE @cheryId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Chery');
DECLARE @mitsubishiId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Mitsubishi');
DECLARE @volvoId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Volvo');
DECLARE @audiId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Audi');
DECLARE @porscheId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Porsche');
DECLARE @landroverId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Land Rover');
DECLARE @jaguarId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Jaguar');
DECLARE @lexusId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Lexus');
DECLARE @acuraId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Acura');
DECLARE @infinitiId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Infiniti');
DECLARE @miniId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Mini');
DECLARE @alfaromeoId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Alfa Romeo');
DECLARE @lincolnId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Lincoln');
DECLARE @genesisId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Genesis');
DECLARE @holdenId INT = (SELECT MarcaId FROM Marcas WHERE Nombre='Holden');

INSERT INTO Modelos (Nombre, MarcaId) VALUES
('Camry', @toyotaId),
('Corolla', @toyotaId),
('Prius', @toyotaId),
('RAV4', @toyotaId),
('Highlander', @toyotaId),
('Tacoma', @toyotaId),
('Tundra', @toyotaId),
('bZ4X', @toyotaId),
('4Runner', @toyotaId),
('Sienna', @toyotaId),
('Land Cruiser', @toyotaId),
('Crown', @toyotaId),
('GR86', @toyotaId),
('GR Corolla', @toyotaId),
('GR Supra', @toyotaId),
('Prius Prime', @toyotaId),
('Highlander Hybrid', @toyotaId),
('RAV4 Hybrid', @toyotaId),
('Tacoma Hybrid', @toyotaId),
('Tundra Hybrid', @toyotaId),
('Venza', @toyotaId),
('Accent', @hyundaiId),
('Elantra', @hyundaiId),
('Sonata', @hyundaiId),
('Kona', @hyundaiId),
('Tucson', @hyundaiId),
('Santa Fe', @hyundaiId),
('Palisade', @hyundaiId),
('Venue', @hyundaiId),
('Veloster', @hyundaiId),
('Ioniq 5', @hyundaiId),
('Ioniq 6', @hyundaiId),
('Kona Electric', @hyundaiId),
('Tucson Hybrid', @hyundaiId),
('Rogue',        @nissanId),  
('Altima',       @nissanId),  
('Sentra',       @nissanId),  
('Frontier',     @nissanId),  
('Pathfinder',   @nissanId), 
('Kicks',        @nissanId),  
('Leaf',         @nissanId),  
('ARiYA',        @nissanId),  
('Z',            @nissanId),  
('Armada',       @nissanId),
('Sportage',   @kiaId),  -- SUV compacto, mejor vendido a nivel global :contentReference[oaicite:1]{index=1}
('Sorento',    @kiaId),  -- SUV mediano muy popular :contentReference[oaicite:2]{index=2}
('Telluride',  @kiaId),  -- SUV grande de tres filas, récords de ventas :contentReference[oaicite:3]{index=3}
('Seltos',     @kiaId),  -- SUV subcompacto de fuerte demanda :contentReference[oaicite:4]{index=4}
('Forte',      @kiaId),  -- Sedán compacto con altas ventas :contentReference[oaicite:5]{index=5}
('K5',         @kiaId),  -- Sedán mediano, remarcable en ventas recientes :contentReference[oaicite:6]{index=6}
('Carnival',   @kiaId),  -- Minivan con récord anual de ventas en 2024 :contentReference[oaicite:7]{index=7}
('EV6',        @kiaId),  -- SUV eléctrico con récord de ventas :contentReference[oaicite:8]{index=8}
('EV9',        @kiaId),  -- Nuevo SUV eléctrico premiado, ventas crecientes :contentReference[oaicite:9]{index=9}
('Soul',       @kiaId),
('Swift',      @suzukiId), -- modelo compacto líder y esperado para 2024 :contentReference[oaicite:1]{index=1}
('Swift Sport',@suzukiId), -- variante deportiva del Swift :contentReference[oaicite:2]{index=2}
('Baleno',     @suzukiId), -- hatchback popular globalmente :contentReference[oaicite:3]{index=3}
('Celerio',    @suzukiId), -- city car muy vendido :contentReference[oaicite:4]{index=4}
('Ignis',      @suzukiId), -- micro SUV urbano :contentReference[oaicite:5]{index=5}
('Vitara',     @suzukiId), -- SUV mediano/híbrido :contentReference[oaicite:6]{index=6}
('Grand Vitara', @suzukiId), -- SUV familiar popular :contentReference[oaicite:7]{index=7}
('S-Cross',     @suzukiId), -- crossover global :contentReference[oaicite:8]{index=8}
('Jimny',      @suzukiId), -- SUV compacto icónico :contentReference[oaicite:9]{index=9}
('Fronx',      @suzukiId),
('Civic',         @hondaId), -- compacto versátil y premiado :contentReference[oaicite:11]{index=11}
('Accord',        @hondaId), -- mediano premiado :contentReference[oaicite:12]{index=12}
('CR-V',          @hondaId), -- SUV compacto, modelo popular :contentReference[oaicite:13]{index=13}
('CR-V Hybrid',   @hondaId), -- variante híbrida del CR-V :contentReference[oaicite:14]{index=14}
('HR-V',          @hondaId), -- subcompacto SUV :contentReference[oaicite:15]{index=15}
('Pilot',         @hondaId), -- SUV familiar :contentReference[oaicite:16]{index=16}
('Odyssey',       @hondaId), -- minivan familiar :contentReference[oaicite:17]{index=17}
('Passport',      @hondaId), -- SUV mediano :contentReference[oaicite:18]{index=18}
('Ridgeline',     @hondaId), -- pickup crossover :contentReference[oaicite:19]{index=19}
('Prologue',      @hondaId),
('CX-5',     @mazdaId),  -- El SUV compacto más vendido de Mazda :contentReference[oaicite:1]{index=1}
('MX-5 Miata', @mazdaId),-- Roadster deportivo icónico :contentReference[oaicite:2]{index=2}
('CX-30',    @mazdaId),  -- Subcompacto con alta demanda :contentReference[oaicite:3]{index=3}
('CX-60',    @mazdaId),  -- SUV mediano reciente :contentReference[oaicite:4]{index=4}
('Mazda3',   @mazdaId),  -- Compacto popular :contentReference[oaicite:5]{index=5}
('Mazda6',   @mazdaId),  -- Sedán mediano premiado :contentReference[oaicite:6]{index=6}
('CX-50',    @mazdaId),  -- SUV confiable para 2025 :contentReference[oaicite:7]{index=7}
('CX-90',    @mazdaId),  -- Gran SUV para 2024–25 :contentReference[oaicite:8]{index=8}
('CX-9',     @mazdaId),  -- SUV familiar grande :contentReference[oaicite:9]{index=9}
('CX-3',     @mazdaId),
('Mustang',        @fordId), -- Ícono deportivo :contentReference[oaicite:11]{index=11}
('F-150',          @fordId), -- Pickup más vendida de Ford :contentReference[oaicite:12]{index=12}
('Bronco',         @fordId), -- SUV legendario :contentReference[oaicite:13]{index=13}
('Escape',         @fordId), -- SUV compacto popular :contentReference[oaicite:14]{index=14}
('Explorer',       @fordId), -- SUV familiar grande :contentReference[oaicite:15]{index=15}
('Ranger',         @fordId), -- Pickup mediana :contentReference[oaicite:16]{index=16}
('Transit',        @fordId), -- Van comercial dominante :contentReference[oaicite:17]{index=17}
('F-250 Super Duty',@fordId),-- Pickup pesada :contentReference[oaicite:18]{index=18}
('Super Duty F-350',@fordId),-- Variante de carga pesada :contentReference[oaicite:19]{index=19}
('Fiesta',         @fordId),
('Outback',     @subaruId),
('Forester',    @subaruId),
('Crosstrek',   @subaruId),
('Impreza',     @subaruId),
('Ascent',      @subaruId),
('Legacy',      @subaruId),
('BRZ',         @subaruId),
('WRX',         @subaruId),
('Solterra',    @subaruId), 
('Baja',        @subaruId),
('Golf',       @volkswagenId),
('Tiguan',     @volkswagenId),
('Passat',     @volkswagenId),
('Jetta',      @volkswagenId),
('Polo',       @volkswagenId),
('Touareg',    @volkswagenId),
('Arteon',     @volkswagenId),
('Atlas',      @volkswagenId),
('Taos',       @volkswagenId),
('ID.4',       @volkswagenId),
('208',         @peugeotId),
('2008',        @peugeotId),
('3008',        @peugeotId),
('5008',        @peugeotId),
('308',         @peugeotId),
('508',         @peugeotId),
('Rifter',      @peugeotId),
('e-208',       @peugeotId),
('e-2008',      @peugeotId),
('Partner',     @peugeotId),
('C3',          @citroënId),
('C3 Aircross', @citroënId),
('C4',          @citroënId),
('C5 Aircross', @citroënId),
('C4 Cactus',   @citroënId),
('C5 X',        @citroënId),
('Berlingo',    @citroënId),
('Spacetourer', @citroënId),
('e-C4',        @citroënId),
('Ami',         @citroënId),
('500',         @fiatId),
('Panda',       @fiatId),
('Punto',       @fiatId),
('Tipo',        @fiatId),
('500X',        @fiatId),
('500e',        @fiatId),
('Panda Cross', @fiatId),
('Doblo',       @fiatId),
('Cronos',      @fiatId),
('Strada',      @fiatId),
('Renegade',    @jeepId),
('Compass',     @jeepId),
('Cherokee',    @jeepId),
('Grand Cherokee', @jeepId),
('Wrangler',    @jeepId),
('Gladiator',   @jeepId),
('Cherokee 4xe',@jeepId),
('Grand Cherokee 4xe', @jeepId),
('Avenger',     @jeepId),
('Commander',   @jeepId),
('Spark',       @chevroletId),
('Bolt EV',     @chevroletId),
('Malibu',      @chevroletId),
('Cruze',       @chevroletId),
('Equinox',     @chevroletId),
('Blazer',      @chevroletId),
('Trailblazer', @chevroletId),
('Traverse',    @chevroletId),
('Tahoe',       @chevroletId),
('Silverado',   @chevroletId),
('Escalade',           @cadillacId),
('XT5',                @cadillacId),
('XT6',                @cadillacId),
('CT4',                @cadillacId),
('CT5',                @cadillacId),
('CT4-V',              @cadillacId),
('CT5-V',              @cadillacId),
('LYRIQ',              @cadillacId),
('XT4',                @cadillacId),
('XT5 Sport',          @cadillacId),
('Sierra 1500',        @gmcId),
('Sierra HD',          @gmcId),
('Yukon',              @gmcId),
('Yukon XL',           @gmcId),
('Terrain',            @gmcId),
('Acadia',             @gmcId),
('Hummer EV',          @gmcId),
('Canyon',             @gmcId),
('Hummer EV SUV',      @gmcId),
('Jimmy',              @gmcId), 
('1500',               @ramId),
('2500',               @ramId),
('3500',               @ramId),
('ProMaster',          @ramId),
('ProMaster City',     @ramId),
('RamTRX',             @ramId),
('1500 Classic',       @ramId),
('Chassis Cab',        @ramId),
('ProMaster Electric', @ramId),
('Ram 700',            @ramId),
('Atto 3',             @bydId),
('Tang',               @bydId),
('Dolphin',            @bydId),
('Seal',               @bydId),
('Yuan Plus',          @bydId),
('Song Pro',           @bydId),
('Han',                @bydId),
('Yuan',               @bydId),
('Dolphin R',          @bydId),
('Seal U',             @bydId),
('MG ZS',              @saicId),
('MG HS',              @saicId),
('MG Hector',          @saicId),
('MG5',                @saicId),
('MG6',                @saicId),
('MG3',                @saicId),
('MG4 EV',             @saicId),
('MG ZS EV',           @saicId),
('MG4 XPower',         @saicId),
('MG RX8',             @saicId),
('3 Series',           @bmwId),
('5 Series',           @bmwId),
('X3',                 @bmwId),
('X5',                 @bmwId),
('7 Series',           @bmwId),
('i4',                 @bmwId),
('iX',                 @bmwId),
('X1',                 @bmwId),
('M3',                 @bmwId),
('M5',                 @bmwId),
('C-Class',        @mercedesbenzId),
('E-Class',        @mercedesbenzId),
('S-Class',        @mercedesbenzId),
('GLC',            @mercedesbenzId),
('GLE',            @mercedesbenzId),
('GLS',            @mercedesbenzId),
('A-Class',        @mercedesbenzId),
('CLA',            @mercedesbenzId),
('GLA',            @mercedesbenzId),
('EQC',            @mercedesbenzId),
('Clio',           @renaultId),
('Captur',         @renaultId),
('Megane',         @renaultId),
('Kadjar',         @renaultId),
('Scenic',         @renaultId),
('Arkana',         @renaultId),
('Koleos',         @renaultId),
('Twingo',         @renaultId),
('Zoe',            @renaultId),
('Traffic',        @renaultId),
('Model 3',        @teslaId),
('Model Y',        @teslaId),
('Model S',        @teslaId),
('Model X',        @teslaId),
('Cybertruck',     @teslaId),
('Roadster',       @teslaId),
('Semi',           @teslaId),
('Highland',       @teslaId),   -- nueva actualización
('Model 2 Prototype', @teslaId),
('Model Y Performance', @teslaId),
('Geometry A',      @geelyId),
('Geometry C',      @geelyId),
('Atlas',           @geelyId),
('Coolray',         @geelyId),
('Panda',           @geelyId),
('Emgrand',         @geelyId),
('Okavango',        @geelyId),
('Monjaro',         @geelyId),
('FENGSHEN L',      @geelyId),
('Preface',         @geelyId),
('Tiggo 3',          @cheryId),
('Tiggo 4',          @cheryId),
('Tiggo 5',          @cheryId),
('Tiggo 7',          @cheryId),
('Tiggo 8',          @cheryId),
('Arrizo 5',         @cheryId),
('Arrizo 6',         @cheryId),
('Arrizo 8',         @cheryId),
('QQ',               @cheryId),
('Omoda 5',          @cheryId),
('Outlander',       @mitsubishiId),
('Eclipse Cross',   @mitsubishiId),
('ASX / RVR',       @mitsubishiId),
('Outlander PHEV',  @mitsubishiId),
('Mirage',          @mitsubishiId),
('Pajero / Montero',@mitsubishiId),
('L200 / Triton',   @mitsubishiId),
('L200 EV',         @mitsubishiId),
('Xpander',         @mitsubishiId),
('Lancer',          @mitsubishiId),
('XC60',            @volvoId),
('XC90',            @volvoId),
('XC40',            @volvoId),
('S60',             @volvoId),
('S90',             @volvoId),
('V60',             @volvoId),
('V90',             @volvoId),
('C40 Recharge',    @volvoId),
('EX30',            @volvoId),
('Polestar 2',      @volvoId),
('A3',             @audiId),
('A4',             @audiId),
('A6',             @audiId),
('A8',             @audiId),
('Q3',             @audiId),
('Q5',             @audiId),
('Q7',             @audiId),
('Q8',             @audiId),
('e-tron',         @audiId),
('RS6 Avant',      @audiId),
('911',            @porscheId),
('Cayenne',        @porscheId),
('Macan',          @porscheId),
('Panamera',       @porscheId),
('Taycan',         @porscheId),
('718 Cayman',     @porscheId),
('718 Boxster',    @porscheId),
('Taycan Cross Turismo', @porscheId),
('Cayman GT4',     @porscheId),
('Macan EV',       @porscheId),
('Discovery',      @landroverId),
('Defender',       @landroverId),
('Range Rover',    @landroverId),
('Range Rover Sport', @landroverId),
('Range Rover Velar', @landroverId),
('Discovery Sport',  @landroverId),
('Defender 90',      @landroverId),
('Range Rover Evoque', @landroverId),
('Range Rover SV',     @landroverId),
('Defender 110',      @landroverId),
('XE',             @jaguarId),
('XF',             @jaguarId),
('XJ',             @jaguarId),
('F-PACE',         @jaguarId),
('E-PACE',         @jaguarId),
('I-PACE',         @jaguarId),
('F-TYPE',         @jaguarId),
('XEL',            @jaguarId),
('XFL',            @jaguarId),
('I-PACE SVR',     @jaguarId),
('IS',             @lexusId),
('ES',             @lexusId),
('GS',             @lexusId),
('LS',             @lexusId),
('NX',             @lexusId),
('RX',             @lexusId),
('GX',             @lexusId),
('LX',             @lexusId),
('UX',             @lexusId),
('RZ',             @lexusId),
('Integra',        @acuraId),
('TLX',            @acuraId),
('ILX',            @acuraId),
('RLX',            @acuraId),
('RDX',            @acuraId),
('MDX',            @acuraId),
('CDX',            @acuraId),
('ZDX',            @acuraId),
('NSX',            @acuraId),
('Prologue',       @acuraId),
('Q50',               @infinitiId),
('Q60',               @infinitiId),
('Q70',               @infinitiId),
('QX50',              @infinitiId),
('QX55',              @infinitiId),
('QX60',              @infinitiId),
('QX80',              @infinitiId),
('QX4',               @infinitiId),
('QX30',              @infinitiId),
('Q30',               @infinitiId),
('Cooper',            @miniId),
('Convertible',       @miniId),
('Clubman',           @miniId),
('Countryman',        @miniId),
('Paceman',           @miniId),
('John Cooper Works', @miniId),
('Electric',          @miniId),
('Cooper S',          @miniId),
('Cooper JCW GP',     @miniId),
('Classic',           @miniId),
('Giulia',            @alfaromeoId),
('Stelvio',           @alfaromeoId),
('Tonale',            @alfaromeoId),
('Giulietta',         @alfaromeoId),
('4C',                @alfaromeoId),
('GTV',               @alfaromeoId),
('Giulia GTA',        @alfaromeoId),
('Stelvio Quadrifoglio', @alfaromeoId),
('MiTo',              @alfaromeoId),
('Giulietta Veloce',  @alfaromeoId),
('Corsair',           @lincolnId),
('Nautilus',          @lincolnId),
('Aviator',           @lincolnId),
('Navigator',         @lincolnId),
('MKZ',               @lincolnId),
('MKC',               @lincolnId),
('MKS',               @lincolnId),
('MKT',               @lincolnId),
('Zephyr',            @lincolnId),
('Continental',       @lincolnId),
('G70',               @genesisId),
('G80',               @genesisId),
('G90',               @genesisId),
('GV70',              @genesisId),
('GV80',              @genesisId),
('GV60',              @genesisId),
('GV90',              @genesisId),
('X Concept',         @genesisId),
('Mint Concept',      @genesisId),
('Essentia Concept',  @genesisId);



select * from dbo.Modelos

select * from dbo.TareasVehiculo
