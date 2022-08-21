using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace api
{
    public class ToDo
    {
        public int Id;
        public string Title;
        public bool Completed;
    }    

    public static class create
    {
        static List<ToDo> _db = new List<ToDo>();
        static int _nextId = 1;

        static create()
        {            
            _db.Add(new ToDo { Id=1, Title="Hello World!", Completed=false } );
            _db.Add(new ToDo { Id=2, Title="Hallo Welt!", Completed=false } );
            _db.Add(new ToDo { Id=3, Title="Salut Monde!", Completed=false } );
            _nextId = 4;
        }
        
        [FunctionName("Get")]
        public static async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "create/{id:int?}")] HttpRequest req, 
            ILogger log, 
            int? id)
        {
            if (id == null) 
                return new OkObjectResult(_db);           

            var t = _db.Find(i => i.Id == id);            
            
            if (t == null)
                return await Task.FromResult<IActionResult>(new NotFoundResult());
            
            return await Task.FromResult<IActionResult>(new OkObjectResult(t));
        }

        [FunctionName("Post")]
        public static async Task<IActionResult> Post(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "create")] HttpRequest req, 
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ToDo data = JsonConvert.DeserializeObject<ToDo>(requestBody);
            
            if (data.Id == 0) {
                data.Id = _nextId;
                _nextId += 1;
            }
            _db.Add(data);
            
            return new OkObjectResult(data);
        }

        [FunctionName("Patch")]
        public static async Task<IActionResult> Patch(
            [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "create/{id}")] HttpRequest req, 
            ILogger log,
            int id)
        {
            var t = _db.Find(i => i.Id == id);
            
            if (t == null)
                return await Task.FromResult<IActionResult>(new NotFoundResult());

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            ToDo data = JsonConvert.DeserializeObject<ToDo>(requestBody);
            
            t.Title = data.Title ?? t.Title;
            t.Completed = data.Completed != false ? data.Completed : t.Completed;
            
            return await Task.FromResult<IActionResult>(new OkObjectResult(t));
        }

        [FunctionName("Delete")]
        public static async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "create/{id}")] HttpRequest req, 
            ILogger log,
            int id)
        {
            var t = _db.Find(i => i.Id == id);

            if (t == null)
                return await Task.FromResult<IActionResult>(new NotFoundResult());
            
            _db.Remove(t);

            return await Task.FromResult<IActionResult>(new OkObjectResult(t));
        }
    }
}