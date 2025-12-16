namespace BibleApp.Models;

public class Chapter
{
    public string _id { get; set; } = string.Empty;
    public string translationName { get; set; } = string.Empty;
    public string bookName { get; set; } = string.Empty;
    public int chapterNo { get; set; }
    public List<Verse> verses { get; set; } = new();
}
