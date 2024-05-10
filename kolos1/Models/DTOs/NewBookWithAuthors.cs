namespace kolos1.Models.DTOs;

public class NewBookWithAuthors
{
    public string Title { get; set; } = string.Empty;
    public IEnumerable<Author> Authors { get; set; } = new List<Author>();
}