﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SchoolWebApiProject.Models;

namespace SchoolWebApiProject.Controllers
{
    public class UsersController : ApiController
    {
        private SchoolWebEntities db = new SchoolWebEntities();

        // POST: api/Users/Login
        [HttpPost]
        [Route("api/Users/Login")]
        public async Task<IHttpActionResult> Login([FromBody] User model)
        {
            if (model == null || string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
            {
                return BadRequest("Username and password are required.");
            }

            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            return Ok(new { user.UserId, user.Username, user.RoleId });
        }

        // POST: api/Users/Register
        [HttpPost]
        [Route("api/Users/Register")]
        public async Task<IHttpActionResult> Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password) || user.RoleId < 0 || user.RoleId>=3)
            {
                return BadRequest("All fields are required and RoleId must be greater than 0.");  //hiç gelmeceyecek güvenlik için.
            }

            if (db.Users.Any(u => u.Username == user.Username))
            {
                return Conflict(); // 409 Conflict - Username already exists
            }

            // Add the new user to the database
            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Ok(); // 200 OK - Registration successful
        }
    


    // GET: api/Users
    public IQueryable<User> GetUsers()
        {
            return db.Users;
        }

        // GET: api/Users/5
        [ResponseType(typeof(User))]
        public async Task<IHttpActionResult> GetUser(int id)
        {
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

    //    // PUT: api/Users/5
    //    [ResponseType(typeof(void))]
    //    public async Task<IHttpActionResult> PutUser(int id, User user)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        if (id != user.UserId)
    //        {
    //            return BadRequest();
    //        }

    //        //db.Entry(user).State = EntityState.Modified;

    //        try
    //        {
    //            await db.SaveChangesAsync();
    //        }
    //        catch (DbUpdateConcurrencyException)
    //        {
    //            if (!UserExists(id))
    //            {
    //                return NotFound();
    //            }
    //            else
    //            {
    //                throw;
    //            }
    //        }

    //        return StatusCode(HttpStatusCode.NoContent);
    //    }

    //    // POST: api/Users
    //    [ResponseType(typeof(User))]
    //    public async Task<IHttpActionResult> PostUser(User user)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        db.Users.Add(user);
    //        await db.SaveChangesAsync();

    //        return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
    //    }

    //    // DELETE: api/Users/5
    //    [ResponseType(typeof(User))]
    //    public async Task<IHttpActionResult> DeleteUser(int id)
    //    {
    //        User user = await db.Users.FindAsync(id);
    //        if (user == null)
    //        {
    //            return NotFound();
    //        }

    //        db.Users.Remove(user);
    //        await db.SaveChangesAsync();

    //        return Ok(user);
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        if (disposing)
    //        {
    //            db.Dispose();
    //        }
    //        base.Dispose(disposing);
    //    }

    //    private bool UserExists(int id)
    //    {
    //        return db.Users.Count(e => e.UserId == id) > 0;
    //    }
    
    
    }
}