using Microsoft.AspNetCore.Mvc;

namespace backend
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var users = new[]
            {
                new { Id = 1, Name = "Alice", Email = "alice@gmail.com" },
                new { Id = 2, Name = "Bob", Email = "bob@gmail.com" },
                new { Id = 3, Name = "Charlie", Email = "charlie@gmail.com" }
            };  
            return Ok(users);
        }
    }
}
