-- =============================================
-- Stored Procedures for Top 2000 Statistics
-- =============================================

-- 1. Biggest Drops - Shows songs with the biggest position drops
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_BiggestDrops')
    DROP PROCEDURE sp_Top2000_BiggestDrops
GO

CREATE PROCEDURE sp_Top2000_BiggestDrops
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t1.Position,
        s.Titel AS Title,
        a.Name AS Artist,
        s.ReleaseYear,
        (t1.Position - t2.Position) AS Delta
    FROM Top2000Entries t1
    INNER JOIN Songs s ON t1.SongId = s.SongId
    INNER JOIN Artist a ON s.ArtistId = a.ArtistId
    INNER JOIN Top2000Entries t2 ON t1.SongId = t2.SongId AND t2.Year = @Year - 1
    WHERE t1.Year = @Year
        AND t1.Position > t2.Position
    ORDER BY Delta DESC;
END
GO

-- 2. Biggest Rises - Shows songs with the biggest position improvements
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_BiggestRises')
    DROP PROCEDURE sp_Top2000_BiggestRises
GO

CREATE PROCEDURE sp_Top2000_BiggestRises
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t1.Position,
        s.Titel AS Title,
        a.Name AS Artist,
        s.ReleaseYear,
        (t2.Position - t1.Position) AS Delta
    FROM Top2000Entries t1
    INNER JOIN Songs s ON t1.SongId = s.SongId
    INNER JOIN Artist a ON s.ArtistId = a.ArtistId
    INNER JOIN Top2000Entries t2 ON t1.SongId = t2.SongId AND t2.Year = @Year - 1
    WHERE t1.Year = @Year
        AND t1.Position < t2.Position
    ORDER BY Delta DESC;
END
GO

-- 3. Ever-Present Songs - Songs that appear in every edition
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_EverPresent')
    DROP PROCEDURE sp_Top2000_EverPresent
GO

CREATE PROCEDURE sp_Top2000_EverPresent
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalYears INT;
    
    SELECT @TotalYears = COUNT(DISTINCT Year) FROM Top2000Entries;
    
    SELECT 
        s.Titel AS Title,
        a.Name AS Artist
    FROM Songs s
    INNER JOIN Artist a ON s.ArtistId = a.ArtistId
    WHERE (SELECT COUNT(DISTINCT Year) FROM Top2000Entries WHERE SongId = s.SongId) = @TotalYears
    ORDER BY s.Titel;
END
GO

-- 4. New Entries - Songs appearing for the first time in a given year
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_NewEntries')
    DROP PROCEDURE sp_Top2000_NewEntries
GO

CREATE PROCEDURE sp_Top2000_NewEntries
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.Position,
        s.Titel AS Title,
        a.Name AS Artist,
        s.ReleaseYear
    FROM Top2000Entries t
    INNER JOIN Songs s ON t.SongId = s.SongId
    INNER JOIN Artist a ON s.ArtistId = a.ArtistId
    WHERE t.Year = @Year
        AND NOT EXISTS (
            SELECT 1 FROM Top2000Entries t2 
            WHERE t2.SongId = t.SongId 
            AND t2.Year < @Year
        )
    ORDER BY t.Position;
END
GO

-- 5. Dropouts - Songs that were in the list last year but not this year
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_Dropouts')
    DROP PROCEDURE sp_Top2000_Dropouts
GO

CREATE PROCEDURE sp_Top2000_Dropouts
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.Position AS PreviousPosition,
        s.Titel AS Title,
        a.Name AS Artist,
        s.ReleaseYear
    FROM Top2000Entries t
    INNER JOIN Songs s ON t.SongId = s.SongId
    INNER JOIN Artist a ON s.ArtistId = a.ArtistId
    WHERE t.Year = @Year - 1
        AND NOT EXISTS (
            SELECT 1 FROM Top2000Entries t2 
            WHERE t2.SongId = t.SongId 
            AND t2.Year = @Year
        )
    ORDER BY t.Position;
END
GO

-- 6. Re-Entries - Songs returning after being absent
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_ReEntries')
    DROP PROCEDURE sp_Top2000_ReEntries
GO

CREATE PROCEDURE sp_Top2000_ReEntries
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t.Position,
        s.Titel AS Title,
        a.Name AS Artist,
        s.ReleaseYear
    FROM Top2000Entries t
    INNER JOIN Songs s ON t.SongId = s.SongId
    INNER JOIN Artist a ON s.ArtistId = a.ArtistId
    WHERE t.Year = @Year
        AND NOT EXISTS (
            SELECT 1 FROM Top2000Entries t2 
            WHERE t2.SongId = t.SongId 
            AND t2.Year = @Year - 1
        )
        AND EXISTS (
            SELECT 1 FROM Top2000Entries t3 
            WHERE t3.SongId = t.SongId 
            AND t3.Year < @Year - 1
        )
    ORDER BY t.Position;
END
GO

-- 7. Unchanged Positions - Songs that kept the same position
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_Unchanged')
    DROP PROCEDURE sp_Top2000_Unchanged
GO

CREATE PROCEDURE sp_Top2000_Unchanged
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        t1.Position,
        s.Titel AS Title,
        a.Name AS Artist,
        s.ReleaseYear
    FROM Top2000Entries t1
    INNER JOIN Songs s ON t1.SongId = s.SongId
    INNER JOIN Artist a ON s.ArtistId = a.ArtistId
    INNER JOIN Top2000Entries t2 ON t1.SongId = t2.SongId AND t2.Year = @Year - 1
    WHERE t1.Year = @Year
        AND t1.Position = t2.Position
    ORDER BY t1.Position;
END
GO

-- 8. Consecutive Artist Positions - Artists with multiple consecutive positions
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_ConsecutiveArtistPositions')
    DROP PROCEDURE sp_Top2000_ConsecutiveArtistPositions
GO

CREATE PROCEDURE sp_Top2000_ConsecutiveArtistPositions
    @Year INT
AS
BEGIN
    SET NOCOUNT ON;
    
    WITH RankedSongs AS (
        SELECT 
            a.Name AS Artist,
            t.Position,
            s.Titel AS Title,
            s.ReleaseYear,
            LEAD(t.Position) OVER (PARTITION BY a.ArtistId ORDER BY t.Position) AS NextPosition
        FROM Top2000Entries t
        INNER JOIN Songs s ON t.SongId = s.SongId
        INNER JOIN Artist a ON s.ArtistId = a.ArtistId
        WHERE t.Year = @Year
    )
    SELECT 
        Artist,
        Position,
        Title,
        ReleaseYear,
        NextPosition
    FROM RankedSongs
    WHERE NextPosition = Position + 1
    ORDER BY Position;
END
GO

-- 9. One-Timers - Songs that only appeared once
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_OneTimers')
    DROP PROCEDURE sp_Top2000_OneTimers
GO

CREATE PROCEDURE sp_Top2000_OneTimers
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        a.Name AS Artist,
        s.Titel AS Title,
        s.ReleaseYear,
        t.Position,
        t.Year
    FROM Songs s
    INNER JOIN Artist a ON s.ArtistId = a.ArtistId
    INNER JOIN Top2000Entries t ON s.SongId = t.SongId
    WHERE (SELECT COUNT(*) FROM Top2000Entries WHERE SongId = s.SongId) = 1
    ORDER BY t.Year, t.Position;
END
GO

-- 10. Top Artists - Artists with most songs in a given year
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_Top2000_TopArtists')
    DROP PROCEDURE sp_Top2000_TopArtists
GO

CREATE PROCEDURE sp_Top2000_TopArtists
    @Year INT,
    @Take INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Take)
        a.Name AS Artist,
        COUNT(*) AS SongCount,
        AVG(CAST(t.Position AS FLOAT)) AS AveragePosition,
        MIN(t.Position) AS BestPosition
    FROM Top2000Entries t
    INNER JOIN Songs s ON t.SongId = s.SongId
    INNER JOIN Artist a ON s.ArtistId = a.ArtistId
    WHERE t.Year = @Year
    GROUP BY a.ArtistId, a.Name
    ORDER BY SongCount DESC, AveragePosition ASC;
END
GO

PRINT 'All stored procedures created successfully!'
