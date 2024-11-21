using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata;
using Minio;

namespace RestAPI.Minio;
public class MinioInitializer{
    public void MinioInit(){
        try
        {
            var minio = new MinioClient()
                                .WithEndpoint("minio")
                                .Build();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
 
    }
}