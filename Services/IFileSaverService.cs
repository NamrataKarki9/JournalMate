namespace JournalMate.Services;

public interface IFileSaverService
{
    Task<string?> SaveFileAsync(string fileName, string content);
}
