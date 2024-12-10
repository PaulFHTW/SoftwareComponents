namespace DAL.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public DateTime UploadDate { get; set; }
        public string Path { get; set; }
        
        public Document(int id, string title, string? content, DateTime uploadDate, string path)
        {
            Id = id;
            Title = title;
            Content = content;
            UploadDate = uploadDate;
            Path = path;
        }

        public Document() 
        {
            Id = 0;
            Title = "";
            UploadDate = DateTime.Now;
            Path = "";
        }
    }
}
