using JournalMaui.Models;

namespace JournalMate.Services;

public interface IPdfExportService
{
    Task<byte[]> GeneratePdfAsync(List<JournalEntry> entries, string dateRangeText, int totalWords);
}