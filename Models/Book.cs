namespace BibleApp.Models;

public class Book
{
    public string _id { get; set; } = string.Empty;
    public string bookName { get; set; } = string.Empty;
    public int bookOrder { get; set; }
    public int chapterCount { get; set; }
}
