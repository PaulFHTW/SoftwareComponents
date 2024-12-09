using DAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DAL.Controllers;

public interface IDocumentController
{
    public Task<IEnumerable<Document>> GetAsync();
    public Task<Document> GetAsyncById(int id);
    public Task<IActionResult> PostAsync(Document item);
    public Task<IActionResult> PutAsync(int id, Document item);
    public Task<IActionResult> DeleteAsync(int id);
}