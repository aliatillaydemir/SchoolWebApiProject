using System;
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
    public class TeachersController : ApiController
    {
        private SchoolWebEntities db = new SchoolWebEntities();


        // GET: api/teachers                      //index için
        public IQueryable<Teacher> GetTeachers()
        {
            return db.Teachers
                .Where(t => !(bool)t.IsDeleted) // Silinmemiş öğretmenleri filtrele
                .Include(t => t.Course); // İlişkili kursları dahil et
        }


        // GET: api/teachers/{id}                      
        // details için
        [ResponseType(typeof(Teacher))]
        public async Task<IHttpActionResult> GetTeacher(int id)
        {
            Teacher teacher = await db.Teachers
                .Include(t => t.Course) // İlişkili kursları dahil et
                .FirstOrDefaultAsync(t => t.TeacherId == id && !(bool)t.IsDeleted); // Silinmemiş öğretmeni bul

            if (teacher == null)
            {
                return NotFound();
            }

            return Ok(teacher);
        }

        [HttpPost]
        [ResponseType(typeof(Teacher))]
        public IHttpActionResult PostTeacher([FromBody] Teacher teacher)
        {
            if (!ModelState.IsValid)
            {
                // Log validation errors or check them
                return BadRequest(ModelState);
            }

            // Set default values for optional fields
            teacher.CreatedDate = DateTime.Now;
            teacher.IsDeleted = false; // Default to false
            if (!teacher.IsActive.HasValue)
            {
                teacher.IsActive = true; // Default to true
            }

            // Check if the provided CourseId exists
            if (!db.Courses.Any(c => c.CourseId == teacher.CourseId))
            {
                return BadRequest("The selected course does not exist.");
            }

            try
            {
                db.Teachers.Add(teacher);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                // Log the exception message
                return InternalServerError(ex);
            }

            // Return the created teacher with appropriate route
            return CreatedAtRoute("DefaultApi", new { id = teacher.TeacherId }, teacher);
        }




        // GET: api/Courses/Active                 //teacher eklerken dersin aktif olup olmadığını kontrolünü yapıyoruz.
        [HttpGet]
        [Route("api/Courses/Active")]
        public IQueryable<Course> GetActiveCourses()
        {
            return db.Courses
                .Where(c => !(bool)c.IsDeleted); // Silinmemiş kursları filtrele
        }

        [HttpGet]
        [ResponseType(typeof(Course))]
        [Route("api/Courses/{id}")]
        public async Task<IHttpActionResult> GetCourse(int id)
        {
            Course course = await db.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id && !(bool)c.IsDeleted);
            if (course == null)
            {
                return NotFound();
            }
            return Ok(course);
        }

        // GET: api/Teacher/5                    //Edit sayfasının web appde get üzerinden görüntülenebilmesi için
        [HttpGet]
        [Route("api/Teacher/{id}")]
        [ResponseType(typeof(Teacher))]
        public async Task<IHttpActionResult> GetTeacherById(int id)
        {
            var teacher = await db.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            return Ok(teacher);
        }


        //// PUT: api/Teachers/5                 //edit teacher güncellemek için
        //[HttpPut]
        //[Route("api/Teachers/{id}")]
        //[ResponseType(typeof(Teacher))]
        //public async Task<IHttpActionResult> PutTeacher(int id, Teacher teacher)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != teacher.TeacherId)
        //    {
        //        return BadRequest("Teacher ID mismatch");
        //    }

        //    var existingTeacher = await db.Teachers.FindAsync(id);
        //    if (existingTeacher == null)
        //    {
        //        return NotFound();
        //    }

        //    existingTeacher.TeacherName = teacher.TeacherName;
        //    existingTeacher.CourseId = teacher.CourseId;
        //    existingTeacher.IsActive = teacher.IsActive.HasValue ? teacher.IsActive.Value : existingTeacher.IsActive;
        //    existingTeacher.IsDeleted = teacher.IsDeleted.HasValue ? teacher.IsDeleted.Value : existingTeacher.IsDeleted;
        //    existingTeacher.CreatedDate = teacher.CreatedDate.HasValue ? teacher.CreatedDate.Value : existingTeacher.CreatedDate;
        //    existingTeacher.UpdatedDate = DateTime.Now; // Update the UpdatedDate

        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!TeacherExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return Ok(existingTeacher);
        //}

        //private bool TeacherExists(int id)
        //{
        //    return db.Teachers.Count(e => e.TeacherId == id) > 0;
        //}


        [HttpPut]
        [Route("api/Teachers/{id}")]
        [ResponseType(typeof(Teacher))]
        public async Task<IHttpActionResult> PutTeacher(int id, Teacher teacher)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != teacher.TeacherId)
            {
                return BadRequest("Teacher ID mismatch");
            }

            var existingTeacher = await db.Teachers.FindAsync(id);
            if (existingTeacher == null)
            {
                return NotFound();
            }

            // Yalnızca isim güncellenebilir ve ders sabit tutulur
            existingTeacher.TeacherName = teacher.TeacherName;
            existingTeacher.UpdatedDate = DateTime.Now; // Update the UpdatedDate

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(existingTeacher);
        }

        private bool TeacherExists(int id)
        {
            return db.Teachers.Count(e => e.TeacherId == id) > 0;
        }



        [HttpPost]
        [ResponseType(typeof(void))]
        [Route("api/Teachers/DeleteTeacher/{id}")]
        public async Task<IHttpActionResult> DeleteTeacher(int id)
        {
            var teacher = await db.Teachers.FindAsync(id);
            if (teacher == null || teacher.IsDeleted.GetValueOrDefault(false))
            {
                return NotFound();
            }

            // Soft delete işlemi
            teacher.IsDeleted = true;
            teacher.DeletedDate = DateTime.Now.Date; // Sadece tarih kısmını ayarla
            db.Entry(teacher).State = System.Data.Entity.EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }


        //    // GET: api/Teachers
        //    public IQueryable<Teacher> GetTeachers()
        //{
        //    return db.Teachers;
        //}


        //// GET: api/Teachers/5
        //[ResponseType(typeof(Teacher))]
        //public async Task<IHttpActionResult> GetTeacher(int id)
        //{
        //    Teacher teacher = await db.Teachers.FindAsync(id);
        //    if (teacher == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(teacher);
        //}

        //// PUT: api/Teachers/5
        //[ResponseType(typeof(void))]
        //    public async Task<IHttpActionResult> PutTeacher(int id, Teacher teacher)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        if (id != teacher.TeacherId)
        //        {
        //            return BadRequest();
        //        }

        //        db.Entry(teacher).State = System.Data.Entity.EntityState.Modified;

        //        try
        //        {
        //            await db.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TeacherExists(id))
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

        //// POST: api/Teachers
        //[ResponseType(typeof(Teacher))]
        //public async Task<IHttpActionResult> PostTeacher(Teacher teacher)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Teachers.Add(teacher);
        //    await db.SaveChangesAsync();

        //    return CreatedAtRoute("DefaultApi", new { id = teacher.TeacherId }, teacher);
        //}

        //// DELETE: api/Teachers/5
        //[ResponseType(typeof(Teacher))]
        //public async Task<IHttpActionResult> DeleteTeacher(int id)
        //{
        //    Teacher teacher = await db.Teachers.FindAsync(id);
        //    if (teacher == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Teachers.Remove(teacher);
        //    await db.SaveChangesAsync();

        //    return Ok(teacher);
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        //private bool TeacherExists(int id)
        //{
        //    return db.Teachers.Count(e => e.TeacherId == id) > 0;
        //}
    }
}