﻿namespace DAL.Entities
{
    public class Document
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime UploadDate { get; set; }
        public string Path { get; set; }
    }
}
