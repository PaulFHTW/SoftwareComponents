namespace DAL.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? Content { get; set; }
        public DateTime UploadDate { get; set; }
        
        public Document(int id, string title, string? content, DateTime uploadDate)
        {
            Id = id;
            Title = title;
            Content = content;
            UploadDate = uploadDate;
        }

        public Document() 
        {
            Id = 0;
            Title = "";
            UploadDate = DateTime.Now;
        }
    }
}
