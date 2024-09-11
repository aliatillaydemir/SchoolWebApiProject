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
    public class CoursesController : ApiController
    {
        private SchoolWebEntities db = new SchoolWebEntities();


        // GET: api/Courses                    kurs listeleme için
        public IQueryable<Course> GetCourses()
        {
            return db.Courses
                .Include(c => c.Teachers) // Include related teachers
                .Include(c => c.StudentCourses) // Include related student courses
                .Where(c => !(bool)c.IsDeleted); // Filter out deleted courses
        }

        // GET: api/Courses/5                   kurs detay sayfası için ve edit için bir indexteki kursu getiriyoruz.
        [ResponseType(typeof(Course))]
        public async Task<IHttpActionResult> GetCourse(int id)
        {
            Course course = await db.Courses
                .Include(c => c.Teachers) // Include related teachers
                .Include(c => c.StudentCourses) // Include related student courses
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }


        // GET: api/Corses/DeletedCors                       silinmiş kursları bulma
        [HttpGet]
        [Route("api/Corses/DeletedCors")]
        public IHttpActionResult GetDeletedCors()
        {
            var deletedCourses = db.Courses
                .Where(c => c.IsDeleted == true)
                .ToList();

            return Ok(deletedCourses);
        }


        // POST: api/Courses                     yeni kurs oluşturma
        [HttpPost]
        public IHttpActionResult CreateCourse([FromBody] Course course)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            course.CreatedDate = DateTime.Now;
            course.IsDeleted = false; // Varsayılan olarak silinmiş değil
            course.IsActive = course.IsActive ?? true; // Varsayılan olarak aktif

            db.Courses.Add(course);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = course.CourseId }, course);
        }


        // PUT: api/Courses/Restore/5                 silinmiş kursu geri getirme
        [HttpPut]
        [Route("api/Courses/Restore/{id}")]
        public IHttpActionResult RestoreCourse(int id)
        {
            var course = db.Courses.Find(id);
            if (course == null || !course.IsDeleted.GetValueOrDefault(false))
            {
                return NotFound();
            }

            course.IsDeleted = false;
            course.DeletedDate = null;

            db.SaveChanges();

            return Ok(course);
        }


        // PUT: api/Courses/5
        [HttpPost]
        [Route("api/Courses/update/{id}")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpdateCourse(int id, [FromBody] Course course)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != course.CourseId)
            {
                return BadRequest();
            }

            var existingCourse = await db.Courses.FindAsync(id);
            if (existingCourse == null || existingCourse.IsDeleted.GetValueOrDefault(false))
            {
                return NotFound();
            }

            existingCourse.CourseName = course.CourseName;
            existingCourse.IsActive = course.IsActive;
            existingCourse.UpdatedDate = DateTime.Now;

            db.Entry(existingCourse).State = System.Data.Entity.EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
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

        private bool CourseExists(int id)
        {
            return db.Courses.Count(e => e.CourseId == id) > 0;
        }


        [HttpPost]
        [ResponseType(typeof(void))]
        [Route("api/Courses/DeleteCourse/{id}")]
        public async Task<IHttpActionResult> DeleteCourse(int id)
        {
            var course = await db.Courses.FindAsync(id);
            if (course == null || course.IsDeleted.GetValueOrDefault(false))
            {
                return NotFound();
            }

            // Soft delete işlemi
            course.IsDeleted = true;
            course.DeletedDate = DateTime.Now.Date; // Sadece tarih kısmını ayarla
            db.Entry(course).State = System.Data.Entity.EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseExists(id))
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





        //    // GET: api/Courses
        //    public IQueryable<Course> GetCourses()
        //    {
        //        return db.Courses;
        //    }

        //    // GET: api/Courses/5
        //    [ResponseType(typeof(Course))]
        //    public async Task<IHttpActionResult> GetCourse(int id)
        //    {
        //        Course course = await db.Courses.FindAsync(id);
        //        if (course == null)
        //        {
        //            return NotFound();
        //        }

        //        return Ok(course);
        //    }

        //    // PUT: api/Courses/5
        //    [ResponseType(typeof(void))]
        //    public async Task<IHttpActionResult> PutCourse(int id, Course course)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        if (id != course.CourseId)
        //        {
        //            return BadRequest();
        //        }

        //        //db.Entry(course).State = EntityState.Modified;

        //        try
        //        {
        //            await db.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!CourseExists(id))
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

        //    // POST: api/Courses
        //    [ResponseType(typeof(Course))]
        //    public async Task<IHttpActionResult> PostCourse(Course course)
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }

        //        db.Courses.Add(course);
        //        await db.SaveChangesAsync();

        //        return CreatedAtRoute("DefaultApi", new { id = course.CourseId }, course);
        //    }

        // DELETE: api/Courses/5
        
        //    protected override void Dispose(bool disposing)
        //    {
        //        if (disposing)
        //        {
        //            db.Dispose();
        //        }
        //        base.Dispose(disposing);
        //    }

        //    private bool CourseExists(int id)
        //    {
        //        return db.Courses.Count(e => e.CourseId == id) > 0;
        //    }
    }
}