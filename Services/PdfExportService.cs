using JournalMaui.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Colors = QuestPDF.Helpers.Colors;

namespace JournalMate.Services;

public class PdfExportService : IPdfExportService
{
    public PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GeneratePdfAsync(List<JournalEntry> entries, string dateRangeText, int totalWords)
    {
        return await Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(11).FontColor(Colors.Black));

                    // Header
                    page.Header().Height(100).Background(Colors.Purple.Lighten3).Padding(20).Column(column =>
                    {
                        column.Item().AlignCenter().Text("JournalMate Export")
                            .FontSize(24).Bold().FontColor(Colors.Purple.Darken2);
                        
                        column.Item().PaddingTop(5).AlignCenter()
                            .Text($"Generated: {DateTime.Now:MMMM dd, yyyy h:mm tt}")
                            .FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    // Content
                    page.Content().PaddingVertical(20).Column(column =>
                    {
                        // Summary Section
                        column.Item().Background(Colors.Grey.Lighten4).Padding(15).Column(summary =>
                        {
                            summary.Item().Text("Export Summary").FontSize(16).Bold().FontColor(Colors.Purple.Darken2);
                            summary.Item().PaddingTop(8).Row(row =>
                            {
                                row.RelativeItem().Text($"Date Range: {dateRangeText}").FontSize(10);
                            });
                            summary.Item().PaddingTop(4).Row(row =>
                            {
                                row.RelativeItem().Text($"Total Entries: {entries.Count}").FontSize(10);
                                row.RelativeItem().Text($"Total Words: {totalWords}").FontSize(10);
                            });
                        });

                        column.Item().PaddingTop(20);

                        // Entries
                        foreach (var entry in entries)
                        {
                            DateTime.TryParse(entry.DateKey, out var entryDate);
                            
                            column.Item().PaddingBottom(20).Column(entryColumn =>
                            {
                                // Entry Header
                                entryColumn.Item().BorderBottom(2).BorderColor(Colors.Purple.Lighten2)
                                    .PaddingBottom(8).Row(row =>
                                {
                                    row.RelativeItem().Column(col =>
                                    {
                                        col.Item().Text(entryDate.ToString("dddd, MMMM dd, yyyy"))
                                            .FontSize(14).Bold().FontColor(Colors.Purple.Darken2);
                                        
                                        if (!string.IsNullOrEmpty(entry.Title))
                                        {
                                            col.Item().Text(entry.Title)
                                                .FontSize(12).SemiBold().FontColor(Colors.Grey.Darken2);
                                        }
                                    });
                                    
                                    var moodEmoji = MoodData.GetMoodEmoji(entry.PrimaryMood);
                                    if (!string.IsNullOrEmpty(moodEmoji))
                                    {
                                        row.ConstantItem(60).AlignRight().Text(moodEmoji).FontSize(20);
                                    }
                                });

                                // Entry Metadata
                                entryColumn.Item().PaddingTop(8).Row(row =>
                                {
                                    if (!string.IsNullOrEmpty(entry.PrimaryMood))
                                    {
                                        var moodText = $"Mood: {entry.PrimaryMood}";
                                        if (!string.IsNullOrEmpty(entry.SecondaryMood1))
                                            moodText += $", {entry.SecondaryMood1}";
                                        if (!string.IsNullOrEmpty(entry.SecondaryMood2))
                                            moodText += $", {entry.SecondaryMood2}";
                                        
                                        row.AutoItem().PaddingRight(15)
                                            .Text(moodText)
                                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                                    }

                                    if (!string.IsNullOrEmpty(entry.Category))
                                    {
                                        row.AutoItem().PaddingRight(15)
                                            .Text($"Category: {entry.Category}")
                                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                                    }

                                    row.AutoItem().Text($"{entry.WordCount} words")
                                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                                });

                                // Tags
                                if (entry.GetTagsList().Any())
                                {
                                    entryColumn.Item().PaddingTop(5).Row(row =>
                                    {
                                        row.AutoItem().Text("Tags: ")
                                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                                        
                                        row.AutoItem().Text(string.Join(", ", entry.GetTagsList().Select(t => $"#{t}")))
                                            .FontSize(9).FontColor(Colors.Purple.Medium);
                                    });
                                }

                                // Entry Content
                                var cleanContent = StripHtmlTags(entry.Content);
                                if (!string.IsNullOrWhiteSpace(cleanContent))
                                {
                                    entryColumn.Item().PaddingTop(12)
                                        .Text(cleanContent)
                                        .FontSize(10).LineHeight(1.6f).FontColor(Colors.Grey.Darken3);
                                }
                            });
                        }
                    });

                    // Footer
                    page.Footer().Height(30).AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium));
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                        text.Span(" - JournalMate");
                    });
                });
            });

            return document.GeneratePdf();
        });
    }

    private string StripHtmlTags(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        text = System.Net.WebUtility.HtmlDecode(text);
        return text.Trim();
    }
}