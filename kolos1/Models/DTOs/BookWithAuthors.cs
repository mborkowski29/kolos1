namespace kolos1.Models.DTOs;

public class BookWithAuthors
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<Author> Authors { get; set; } = null!;
}

