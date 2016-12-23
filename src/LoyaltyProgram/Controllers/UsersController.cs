using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LoyaltyProgram.Models;

namespace LoyaltyProgram.Controllers
{
   
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private IUserStore m_userStore = null;
        public UsersController(IUserStore userStore)
        {
            this.m_userStore = userStore;
        }

        // GET api/users/5
        [HttpGet("{id}",Name ="GetById")]
        public IActionResult Get(int id)
        {
            var user = this.m_userStore.GetById(id);
            if (user == null) return NotFound(id);

            return new ObjectResult(user);
        }

        // POST api/users
        [HttpPost]
        public IActionResult Post([FromBody]LoyaltyProgramUser newUser)
        {
            RegisterUser(newUser);
            return CreatedAtRoute("GetById",new { id = newUser.Id }, newUser);
        }

        private void RegisterUser(LoyaltyProgramUser newUser)
        {
            this.m_userStore.Save(newUser);
        }

        // PUT api/users/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody]LoyaltyProgramUser updateUser)
        {
            this.m_userStore.Save(updateUser);
            return new NoContentResult();
        }

        // DELETE api/users/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
