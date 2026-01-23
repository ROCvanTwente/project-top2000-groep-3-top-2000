using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TemplateJwtProject.Data;

namespace TemplateJwtProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("drops")]
    public async Task<IActionResult> GetBiggestDrops(int year)
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_BiggestDrops",
                reader => new PositionDeltaDto(
                    GetInt(reader, "Position"),
                    GetString(reader, "Title"),
                    GetString(reader, "Artist"),
                    GetNullableInt(reader, "ReleaseYear"),
                    GetInt(reader, "Delta")
                ),
                new SqlParameter("@Year", year)
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load biggest drops", error = ex.Message });
        }
    }

    [HttpGet("rises")]
    public async Task<IActionResult> GetBiggestRises(int year)
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_BiggestRises",
                reader => new PositionDeltaDto(
                    GetInt(reader, "Position"),
                    GetString(reader, "Title"),
                    GetString(reader, "Artist"),
                    GetNullableInt(reader, "ReleaseYear"),
                    GetInt(reader, "Delta")
                ),
                new SqlParameter("@Year", year)
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load biggest rises", error = ex.Message });
        }
    }

    [HttpGet("ever-present")]
    public async Task<IActionResult> GetEverPresent()
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_EverPresent",
                reader => new EverPresentDto(
                    GetString(reader, "Title"),
                    GetString(reader, "Artist")
                )
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load ever-present songs", error = ex.Message });
        }
    }

    [HttpGet("new")]
    public async Task<IActionResult> GetNewEntries(int year)
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_NewEntries",
                reader => new PositionEntryDto(
                    GetInt(reader, "Position"),
                    GetString(reader, "Title"),
                    GetString(reader, "Artist"),
                    GetNullableInt(reader, "ReleaseYear")
                ),
                new SqlParameter("@Year", year)
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load new entries", error = ex.Message });
        }
    }

    [HttpGet("dropouts")]
    public async Task<IActionResult> GetDropouts(int year)
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_Dropouts",
                reader => new DropoutDto(
                    GetInt(reader, "PreviousPosition"),
                    GetString(reader, "Title"),
                    GetString(reader, "Artist"),
                    GetNullableInt(reader, "ReleaseYear")
                ),
                new SqlParameter("@Year", year)
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load dropouts", error = ex.Message });
        }
    }

    [HttpGet("re-entries")]
    public async Task<IActionResult> GetReEntries(int year)
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_ReEntries",
                reader => new PositionEntryDto(
                    GetInt(reader, "Position"),
                    GetString(reader, "Title"),
                    GetString(reader, "Artist"),
                    GetNullableInt(reader, "ReleaseYear")
                ),
                new SqlParameter("@Year", year)
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load re-entries", error = ex.Message });
        }
    }

    [HttpGet("unchanged")]
    public async Task<IActionResult> GetUnchangedPositions(int year)
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_Unchanged",
                reader => new PositionEntryDto(
                    GetInt(reader, "Position"),
                    GetString(reader, "Title"),
                    GetString(reader, "Artist"),
                    GetNullableInt(reader, "ReleaseYear")
                ),
                new SqlParameter("@Year", year)
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load unchanged positions", error = ex.Message });
        }
    }

    [HttpGet("consecutive-artist-positions")]
    public async Task<IActionResult> GetConsecutiveArtistPositions(int year)
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_ConsecutiveArtistPositions",
                reader => new ConsecutiveArtistPositionDto(
                    GetString(reader, "Artist"),
                    GetInt(reader, "Position"),
                    GetString(reader, "Title"),
                    GetNullableInt(reader, "ReleaseYear"),
                    GetNullableInt(reader, "NextPosition")
                ),
                new SqlParameter("@Year", year)
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load consecutive artist positions", error = ex.Message });
        }
    }

    [HttpGet("one-timers")]
    public async Task<IActionResult> GetOneTimers()
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_OneTimers",
                reader => new OneTimerDto(
                    GetString(reader, "Artist"),
                    GetString(reader, "Title"),
                    GetNullableInt(reader, "ReleaseYear"),
                    GetInt(reader, "Position"),
                    GetInt(reader, "Year")
                )
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load one-timers", error = ex.Message });
        }
    }

    [HttpGet("top-artists")]
    public async Task<IActionResult> GetTopArtists(int year, int take = 3)
    {
        try
        {
            var result = await RunStoredProcAsync(
                "sp_Top2000_TopArtists",
                reader => new TopArtistDto(
                    GetString(reader, "Artist"),
                    GetInt(reader, "SongCount"),
                    GetDouble(reader, "AveragePosition"),
                    GetInt(reader, "BestPosition")
                ),
                new SqlParameter("@Year", year),
                new SqlParameter("@Take", take)
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Failed to load top artists", error = ex.Message });
        }
    }

    private async Task<List<T>> RunStoredProcAsync<T>(string procName, Func<DbDataReader, T> map, params SqlParameter[] parameters)
    {
        var results = new List<T>();
        var connection = _context.Database.GetDbConnection();

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }

            using var command = connection.CreateCommand();
            command.CommandText = procName;
            command.CommandType = CommandType.StoredProcedure;

            foreach (var parameter in parameters)
            {
                command.Parameters.Add(parameter);
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(map(reader));
            }
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }

        return results;
    }

    private static int GetInt(DbDataReader reader, string name)
    {
        return reader[name] is DBNull ? 0 : Convert.ToInt32(reader[name]);
    }

    private static int? GetNullableInt(DbDataReader reader, string name)
    {
        return reader[name] is DBNull ? null : Convert.ToInt32(reader[name]);
    }

    private static double GetDouble(DbDataReader reader, string name)
    {
        return reader[name] is DBNull ? 0 : Convert.ToDouble(reader[name]);
    }

    private static string GetString(DbDataReader reader, string name)
    {
        return reader[name] is DBNull ? string.Empty : reader[name].ToString() ?? string.Empty;
    }
}

public record PositionDeltaDto(int Position, string Title, string Artist, int? ReleaseYear, int Delta);

public record EverPresentDto(string Title, string Artist);

public record PositionEntryDto(int Position, string Title, string Artist, int? ReleaseYear);

public record DropoutDto(int PreviousPosition, string Title, string Artist, int? ReleaseYear);

public record ConsecutiveArtistPositionDto(string Artist, int Position, string Title, int? ReleaseYear, int? NextPosition);

public record OneTimerDto(string Artist, string Title, int? ReleaseYear, int Position, int Year);

public record TopArtistDto(string Artist, int SongCount, double AveragePosition, int BestPosition);
