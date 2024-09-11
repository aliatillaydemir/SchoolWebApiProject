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
    public class StudentsController : ApiController
    {
        private SchoolWebEntities db = new SchoolWebEntities();


        // GET: api/students               kurs listeleme için
        [HttpGet]
        public IQueryable<Student> GetStudents()
        {
            return db.Students
                .Where(s => !(bool)s.IsDeleted) // Silinmiş öğrencileri filtrele
                .Include(s => s.StudentCourses)
                .Include(s => s.StudentCourses.Select(sc => sc.Course))
                .Include(s => s.StudentCourses.Select(sc => sc.Teacher));
        }

        // GET: api/students/5
        [HttpGet]
        [Route("api/students/{id}")]
        public async Task<IHttpActionResult> GetStudentById(int id)
        {
            var student = await db.Students
                .Include(s => s.StudentCourses.Select(sc => sc.Course))
                .Include(s => s.StudentCourses.Select(sc => sc.Teacher))
                .SingleOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return NotFound();
            }

            return Ok(student);
        }


        // GET: api/students/create
        [HttpGet]
        [Route("api/students/create")]
        public IHttpActionResult GetCreateForm()
        {
            var courses = db.Courses
                .Where(c => c.IsActive == true)
                .Select(c => new { c.CourseId, c.CourseName })
                .ToList();

            var teachers = db.Teachers
                .Where(t => t.IsActive == true)
                .Select(t => new { t.TeacherId, t.TeacherName })
                .ToList();

            return Ok(new { CourseList = courses, TeacherList = teachers });
        }


        // POST: api/students/create
        [HttpPost]
        [Route("api/students/create")]
        public IHttpActionResult CreateStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Create student
            student.IsDeleted = false;
            student.CreatedDate = DateTime.Now;
            db.Students.Add(student);
            db.SaveChanges();

            // Add student courses
            foreach (var studentCourse in student.StudentCourses)
            {
                if (studentCourse.CourseId.HasValue && studentCourse.TeacherId.HasValue)
                {
                    var course = db.Courses.Find(studentCourse.CourseId.Value);
                    var teacher = db.Teachers.Find(studentCourse.TeacherId.Value);

                    if (course != null && !course.IsDeleted == true && teacher != null && !teacher.IsDeleted == true)
                    {
                        studentCourse.CreatedDate = DateTime.Now;
                        db.StudentCourses.Add(studentCourse);
                    }
                }
            }

            db.SaveChanges();

            return Ok(student);
        }


        // GET: api/students/GetTeachersCours           //dropdown için AJAX student ders-hoca seçimi için
        [HttpGet]
        [Route("api/students/GetTeachersCours")]
        public IHttpActionResult GetTeachersCours(int courseId)
        {
            var teachers = db.Teachers
                              .Where(t => t.CourseId == courseId && !(bool)t.IsDeleted) // Only non-deleted teachers
                              .Select(t => new
                              {
                                  Value = t.TeacherId,
                                  Text = t.TeacherName
                              })
                              .ToList();

            return Ok(teachers);
        }



        // GET: api/students/edit/{id}
        [HttpGet]
        [Route("api/students/edit/{id}")]
        public IHttpActionResult GetEditDetails(int id)
        {
            var student = db.Students
                            .Include(s => s.StudentCourses.Select(sc => sc.Course))
                            .Include(s => s.StudentCourses.Select(sc => sc.Teacher))
                            .FirstOrDefault(s => s.StudentId == id);

            if (student == null)
            {
                return NotFound();
            }

            // Get courses that are not assigned to the student and are not deleted
            var assignedCourseIds = student.StudentCourses.Select(sc => sc.CourseId).ToList();
            var courses = db.Courses
                .Where(c => !assignedCourseIds.Contains(c.CourseId) && !(bool)c.IsDeleted)
                .Select(c => new { c.CourseId, c.CourseName })
                .ToList();

            // Get teachers who are not deleted
            var teachers = db.Teachers
                .Where(t => !(bool)t.IsDeleted)
                .Select(t => new { t.TeacherId, t.TeacherName })
                .ToList();

            return Ok(new
            {
                Student = student,
                CourseList = courses,
                TeacherList = teachers
            });
        }



        //// POST: api/students/edit
        //[HttpPost]
        //[Route("api/students/edit")]
        //public async Task<IHttpActionResult> PostEditStudent([FromBody] Student student)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var existingStudent = db.Students
        //        .Include(s => s.StudentCourses)
        //        .FirstOrDefault(s => s.StudentId == student.StudentId);

        //    if (existingStudent == null)
        //    {
        //        return NotFound();
        //    }

        //    // Update student details
        //    existingStudent.StudentName = student.StudentName;
        //    existingStudent.IsActive = student.IsActive;
        //    existingStudent.UpdatedDate = DateTime.Now;

        //    // Preserve CreatedDate
        //    db.Entry(existingStudent).Property(s => s.CreatedDate).IsModified = false;

        //    // Add course if selected
        //    foreach (var studentCourse in student.StudentCourses)
        //    {
        //        var existingCourse = existingStudent.StudentCourses
        //            .FirstOrDefault(sc => sc.CourseId == studentCourse.CourseId);
        //        if (existingCourse == null)
        //        {
        //            studentCourse.CreatedDate = DateTime.Now;
        //            db.StudentCourses.Add(studentCourse);
        //        }
        //    }

        //    await db.SaveChangesAsync();

        //    return Ok(student);
        //}



        //// POST: api/students/edit
        //[HttpPost]
        //[Route("api/students/edit")]
        //public async Task<IHttpActionResult> PostEditStudent([FromBody] Student student)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var existingStudent = db.Students
        //        .Include(s => s.StudentCourses) // İçindeki ders ve öğretmenler
        //        .FirstOrDefault(s => s.StudentId == student.StudentId);

        //    if (existingStudent == null)
        //    {
        //        return NotFound();
        //    }

        //    // Update student details
        //    existingStudent.StudentName = student.StudentName;
        //    existingStudent.IsActive = student.IsActive;
        //    existingStudent.UpdatedDate = DateTime.Now;

        //    // Preserve CreatedDate
        //    db.Entry(existingStudent).Property(s => s.CreatedDate).IsModified = false;

        //    // Add new courses and teachers
        //    foreach (var studentCourse in student.StudentCourses)
        //    {
        //        var existingStudentCourse = existingStudent.StudentCourses
        //            .FirstOrDefault(sc => sc.CourseId == studentCourse.CourseId && sc.TeacherId == studentCourse.TeacherId);

        //        if (existingStudentCourse == null)
        //        {
        //            // Add new course and teacher
        //            studentCourse.CreatedDate = DateTime.Now;
        //            existingStudent.StudentCourses.Add(studentCourse);
        //        }
        //    }

        //    await db.SaveChangesAsync();

        //    return Ok(student);
        //}


        // POST: api/students/edit
        [HttpPost]
        [Route("api/students/edit")]
        public async Task<IHttpActionResult> PostEditStudent([FromBody] Student student)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingStudent = db.Students
                .Include(s => s.StudentCourses) // İçindeki ders ve öğretmenler
                .FirstOrDefault(s => s.StudentId == student.StudentId);

            if (existingStudent == null)
            {
                return NotFound();
            }

            // Update student details
            existingStudent.StudentName = student.StudentName;
            existingStudent.IsActive = student.IsActive;
            existingStudent.UpdatedDate = DateTime.Now;

            // Preserve CreatedDate
            db.Entry(existingStudent).Property(s => s.CreatedDate).IsModified = false;

            // Add new courses and teachers
            foreach (var studentCourse in student.StudentCourses)
            {
                var existingStudentCourse = existingStudent.StudentCourses
                    .FirstOrDefault(sc => sc.CourseId == studentCourse.CourseId && sc.TeacherId == studentCourse.TeacherId);

                if (existingStudentCourse == null)
                {
                    // Add new course and teacher
                    studentCourse.CreatedDate = DateTime.Now;
                    studentCourse.IsDeleted = false; // Varsayılan olarak false ayarla
                    existingStudent.StudentCourses.Add(studentCourse);
                }
            }

            await db.SaveChangesAsync();

            return Ok(student);
        }




        // DELETE: api/studentcourses/remove/{studentId}/{courseId}
        [HttpDelete]
        [Route("api/studentcourses/remove/{studentId}/{courseId}")]
        public async Task<IHttpActionResult> RemoveCourse(int studentId, int courseId)
        {
            var studentCourse = await db.StudentCourses
                .FirstOrDefaultAsync(sc => sc.StudentId == studentId && sc.CourseId == courseId);

            if (studentCourse == null)
            {
                return NotFound(); // Öğrenci-Kurs ilişkisi bulunamadı
            }

            db.StudentCourses.Remove(studentCourse);
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent); // 204 No Content döner, başarılı silme işlemi anlamında
        }


        [HttpPost]
        [ResponseType(typeof(void))]
        [Route("api/Students/DeleteStudent/{id}")]
        public async Task<IHttpActionResult> DeleteStudent(int id)
        {
            var student = await db.Students.FindAsync(id);
            if (student == null || student.IsDeleted.GetValueOrDefault(false))
            {
                return NotFound();
            }

            // Soft delete işlemi
            student.IsDeleted = true;
            student.DeletedDate = DateTime.Now.Date; // Sadece tarih kısmını ayarla
            db.Entry(student).State = System.Data.Entity.EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
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

        private bool StudentExists(int id)
        {
            return db.Students.Count(e => e.StudentId == id) > 0;
        }

        //// GET: api/Students
        //public IQueryable<Student> GetStudents()
        //{
        //    return db.Students;
        //}


        //// GET: api/Students/5
        //[ResponseType(typeof(Student))]
        //public async Task<IHttpActionResult> GetStudent(int id)
        //{
        //    Student student = await db.Students.FindAsync(id);
        //    if (student == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(student);
        //}

        //// PUT: api/Students/5
        //[ResponseType(typeof(void))]
        //public async Task<IHttpActionResult> PutStudent(int id, Student student)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != student.StudentId)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(student).State = System.Data.Entity.EntityState.Modified;

        //    try
        //    {
        //        await db.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!StudentExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //// POST: api/Students
        //[ResponseType(typeof(Student))]
        //public async Task<IHttpActionResult> PostStudent(Student student)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Students.Add(student);
        //    await db.SaveChangesAsync();

        //    return CreatedAtRoute("DefaultApi", new { id = student.StudentId }, student);
        //}

        //// DELETE: api/Students/5
        //[ResponseType(typeof(Student))]
        //public async Task<IHttpActionResult> DeleteStudent(int id)
        //{
        //    Student student = await db.Students.FindAsync(id);
        //    if (student == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Students.Remove(student);
        //    await db.SaveChangesAsync();

        //    return Ok(student);
        //}

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}


    }
}