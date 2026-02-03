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
    /// Gets the select translation
    /// </summary>
    public async Task<Translation?> GetTranslationAsync(string translationId)
    {
        var translations = await GetTranslationsAsync();
        return translations.FirstOrDefault(t => t._id == translationId);
    }

    /// <summary>
    /// Gets all chapters (loads once and caches)
    /// </summary>
    public async Task<List<Chapter>> GetAllChaptersAsync()
    {
        if (_chapters == null)
        {
            _chapters = await _httpClient.GetFromJsonAsync<List<Chapter>>("chapters.json") ?? new List<Chapter>();
        }
        return _chapters;
    }

    /// <summary>
    /// Gets a specific chapter for the selected translation
    /// </summary>
    /// <param name="bookId">The book ID (e.g., "GEN", "MAT")</param>
    /// <param name="chapterNumber">The chapter number</param>
    public async Task<Chapter?> GetChapterAsync(string bookId, int chapterNumber, string translationId)
    {
        var chapters = await GetAllChaptersAsync();
        var chapterId = $"{translationId}_{bookId}_{chapterNumber:D2}";
        return chapters.FirstOrDefault(c => c._id == chapterId);
    }

    /// <summary>
    /// Gets all chapters for a specific book in the selected translation
    /// </summary>
    /// <param name="bookId">The book ID (e.g., "GEN", "MAT")</param>
    public async Task<List<Chapter>> GetBookChaptersAsync(string bookId, string translationId)
    {
        var chapters = await GetAllChaptersAsync();
        return chapters.Where(c => c._id.StartsWith($"{translationId}_{bookId}_")).ToList();
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
    public async Task<Verse?> GetVerseAsync(string bookId, int chapterNumber, int verseNumber, string translationId)
    {
        var chapter = await GetChapterAsync(bookId, chapterNumber, translationId);
        return chapter?.verses.FirstOrDefault(v => v.verseNo == verseNumber);
    }

    /// <summary>
    /// Searches for verses containing specific text in the selected translation
    /// </summary>
    /// <param name="searchText">The text to search for</param>
    public async Task<List<(Chapter chapter, Verse verse)>> SearchAsync(string searchText, string translationId)
    {
        var chapters = await GetAllChaptersAsync();
        var translationChapters = chapters.Where(c => c._id.StartsWith($"{translationId}_")).ToList();

        var results = new List<(Chapter chapter, Verse verse)>();

        foreach (var chapter in translationChapters)
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
