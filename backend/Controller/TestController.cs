using Microsoft.AspNetCore.Mvc;
using Resturant_Backend.Common.Exceptions;

namespace Resturant_Backend.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {


        [HttpGet("get")]
        public async Task<IActionResult> Get(int id)
        {




            return this.SuccessMessage("dd");

        }

    }
}


