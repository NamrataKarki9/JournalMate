namespace JournalMate.Services;

public interface IFileSaverService
{
    Task<string?> SaveFileAsync(string fileName, string content);
    Task<string?> SavePdfFileAsync(string fileName, byte[] pdfBytes);
}
