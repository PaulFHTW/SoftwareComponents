using System.Net;
using DAL.Entities;
using Newtonsoft.Json;
using NUnit.Framework;

namespace IntegrationTests;

[TestFixture]
public class SearchIndexTests
{
    [Test]
    public async Task IntegrationTest()
    {
        UploadDocument();
        await SearchDocument();
        DeleteDocument();
    }
    
    private void UploadDocument()
    {
        using var httpClient = new HttpClient();
        using var fileContent = new ByteArrayContent(File.ReadAllBytes("IntegrationTests/Paul_Bogza_Proposal.pdf"));
        using var multipartContent = new MultipartFormDataContent();
        multipartContent.Add(fileContent, "file", "Paul_Bogza_Proposal.pdf");
        var postResponse = httpClient.PostAsync("http://localhost:8081/documents", multipartContent).Result;
        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var getResponse = httpClient.GetAsync("http://localhost:8081/documents").Result;
        var files = JsonConvert.DeserializeObject<List<Document>>(getResponse.Content.ReadAsStringAsync().Result);
        var file = files!.FirstOrDefault(f => f.Title == "Paul_Bogza_Proposal.pdf" && f.Id == int.Parse(postResponse.Content.ReadAsStringAsync().Result));
        Assert.That(file, Is.Not.Null);

        Console.WriteLine($"Document uploaded successfully: {file!.Title}");
    }
    
    private async Task SearchDocument()
    {
        await Task.Delay(20000);
        
        var searchTerm = "Proposal";
        using var httpClient = new HttpClient();
        var getResponse = httpClient.GetAsync($"http://localhost:8081/documents/search?q={searchTerm}").Result;
        var files = JsonConvert.DeserializeObject<List<Document>>(getResponse.Content.ReadAsStringAsync().Result);
        var file = files!.FirstOrDefault(f => f.Title == "Paul_Bogza_Proposal.pdf");
        
        Assert.That(file, Is.Not.Null);
        Assert.That(file!.Title.Contains(searchTerm) || file!.Content!.Contains(searchTerm));
        
        Console.WriteLine($"Document found: {file.Title}");
    }
    
    private void DeleteDocument()
    {
        using var httpClient = new HttpClient();
        var getResponse = httpClient.GetAsync("http://localhost:8081/documents").Result;
        var files = JsonConvert.DeserializeObject<List<Document>>(getResponse.Content.ReadAsStringAsync().Result);
        var file = files!.FirstOrDefault(f => f.Title == "Paul_Bogza_Proposal.pdf");
        Assert.That(file, Is.Not.Null);
        
        var deleteResponse = httpClient.DeleteAsync($"http://localhost:8081/documents?id={file!.Id}").Result;
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        getResponse = httpClient.GetAsync("http://localhost:8081/documents").Result;
        files = JsonConvert.DeserializeObject<List<Document>>(getResponse.Content.ReadAsStringAsync().Result);
        file = files!.FirstOrDefault(f => f.Title == "Paul_Bogza_Proposal.pdf");
        Assert.That(file, Is.Null);
        
        Console.WriteLine($"Document deleted successfully");
    }
}