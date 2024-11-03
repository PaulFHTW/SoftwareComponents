namespace DAL.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime UploadDate { get; set; }
        public string Path { get; set; }
        
        public Document(int id, string title, DateTime uploadDate, string path)
        {
            Id = id;
            Title = title;
            UploadDate = uploadDate;
            Path = path;
        }
    }
}
