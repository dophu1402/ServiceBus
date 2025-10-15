using EventBusManager.Abstractions;
using Microsoft.AspNetCore.Mvc;
using PackageDemo.IntegrationEvents.Events;

namespace PackageDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {


        private readonly IEventBus _eventBus;

        public TestController(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        [HttpGet()]
        public IActionResult Get()
        {
            _eventBus.Publish(new TestEvent() { Message = "Test Message"});
            return Ok();
        }
    }
}
