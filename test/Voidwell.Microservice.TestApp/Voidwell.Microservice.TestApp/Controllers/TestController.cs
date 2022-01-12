using Microsoft.AspNetCore.Mvc;
using Voidwell.Microservice.TestApp.Services;

namespace Voidwell.Microservice.TestApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ITestAuthenticatedClient _testClient;
        private readonly ILogger<TestController> _logger;

        public TestController(ITestAuthenticatedClient testClient, ILogger<TestController> logger)
        {
            _testClient = testClient;
            _logger = logger;
        }

        [HttpGet("{characterName}")]
        public async Task<ActionResult> Get(string characterName)
        {
            var data = await _testClient.TestAsync(characterName);

            return Ok(data);
        }
    }
}
