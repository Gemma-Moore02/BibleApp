using System.Net.Http.Json;
using BibleApp.Models;

namespace BibleApp.Services;

public class BibleService
{
    private readonly HttpClient _httpClient;
    private List<Book>? _books;
    private List<Translation>? _translations;
    private List<Chapter>? _chapters;

    public BibleService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets all available books
    /// </summary>
    public async Task<List<Book>> GetBooksAsync()
    {
        if (_books == null)
        {
            _books = await _httpClient.GetFromJsonAsync<List<Book>>("books.json") ?? new List<Book>();
        }
        return _books;
    }

    /// <summary>
    /// Gets all available translations
    /// </summary>
    public async Task<List<Translation>> GetTranslationsAsync()
    {
        if (_translations == null)
        {
            _translations = await _httpClient.GetFromJsonAsync<List<Translation>>("translations.json") ?? new List<Translation>();
        }
        return _translations;
    }

    /// <summary>
    /// Gets the ASV translation
    /// </summary>
    public async Task<Translation?> GetASVTranslationAsync()
    {
        var translations = await GetTranslationsAsync();
        return translations.FirstOrDefault(t => t._id == "ASV");
    }

    /// <summary>
    /// Gets all chapters (loads once and caches)
    /// </summary>
    private async Task<List<Chapter>> GetAllChaptersAsync()
    {
        if (_chapters == null)
        {
            _chapters = await _httpClient.GetFromJsonAsync<List<Chapter>>("chapters.json") ?? new List<Chapter>();
        }
        return _chapters;
    }

    /// <summary>
    /// Gets a specific chapter for the ASV translation
    /// </summary>
    /// <param name="bookId">The book ID (e.g., "GEN", "MAT")</param>
    /// <param name="chapterNumber">The chapter number</param>
    public async Task<Chapter?> GetASVChapterAsync(string bookId, int chapterNumber)
    {
        var chapters = await GetAllChaptersAsync();
        var chapterId = $"ASV_{bookId}_{chapterNumber:D2}";
        return chapters.FirstOrDefault(c => c._id == chapterId);
    }

    /// <summary>
    /// Gets all chapters for a specific book in the ASV translation
    /// </summary>
    /// <param name="bookId">The book ID (e.g., "GEN", "MAT")</param>
    public async Task<List<Chapter>> GetASVBookChaptersAsync(string bookId)
    {
        var chapters = await GetAllChaptersAsync();
        return chapters.Where(c => c._id.StartsWith($"ASV_{bookId}_")).ToList();
    }

    /// <summary>
    /// Gets a book by its ID
    /// </summary>
    /// <param name="bookId">The book ID (e.g., "GEN", "MAT")</param>
    public async Task<Book?> GetBookAsync(string bookId)
    {
        var books = await GetBooksAsync();
        return books.FirstOrDefault(b => b._id == bookId);
    }

    /// <summary>
    /// Gets a specific verse from a chapter
    /// </summary>
    /// <param name="bookId">The book ID (e.g., "GEN", "MAT")</param>
    /// <param name="chapterNumber">The chapter number</param>
    /// <param name="verseNumber">The verse number</param>
    public async Task<Verse?> GetASVVerseAsync(string bookId, int chapterNumber, int verseNumber)
    {
        var chapter = await GetASVChapterAsync(bookId, chapterNumber);
        return chapter?.verses.FirstOrDefault(v => v.verseNo == verseNumber);
    }

    /// <summary>
    /// Searches for verses containing specific text in the ASV translation
    /// </summary>
    /// <param name="searchText">The text to search for</param>
    public async Task<List<(Chapter chapter, Verse verse)>> SearchASVAsync(string searchText)
    {
        var chapters = await GetAllChaptersAsync();
        var asvChapters = chapters.Where(c => c._id.StartsWith("ASV_")).ToList();

        var results = new List<(Chapter chapter, Verse verse)>();

        foreach (var chapter in asvChapters)
        {
            var matchingVerses = chapter.verses
                .Where(v => v.verseText.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            foreach (var verse in matchingVerses)
            {
                results.Add((chapter, verse));
            }
        }

        return results;
    }
}
