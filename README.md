# SchoolWeb API

Click here for [Web Application](https://github.com/aliatillaydemir/SchoolWebProject)  
Click here for project development process and summary: [Project Process & Summary](https://github.com/aliatillaydemir/SchoolWebApiProject/blob/master/Sunu.pdf)

## Introduction
SchoolWeb API is the back-end service for the SchoolWeb system, handling all data operations such as creating, reading, updating, and deleting records for students, teachers, and courses. The API follows the REST architectural style and is built using ASP.NET Web API. It supports role-based access control and integrates with Redis for caching frequently accessed data, improving performance.

## Relational Database Schema:
![Ekran görüntüsü 2024-09-06 160910](https://github.com/user-attachments/assets/7072f64d-8c71-4ee0-9983-bf390e6385db)


## API Endpoints
The following endpoints represent the available operations for managing students, teachers, and courses.

## API Endpoints

### Users
| Method | Endpoint                | Description                       |
|--------|-------------------------|-----------------------------------|
| GET    | `/api/users`          | Retrieve all users             |
| GET    | `/api/users/{id}`     | Retrieve a specific student by ID |
| POST   | `/api/users/login`          | for login              |
| POST    | `/api/users/register`     | for register        |

### Students
| Method | Endpoint                | Description                       |
|--------|-------------------------|-----------------------------------|
| GET    | `/api/students`          | Retrieve all students             |
| GET    | `/api/students/{id}`     | Retrieve a specific student by ID |
| POST   | `/api/students`          | Create a new student              |
| PUT    | `/api/students/{id}`     | Update an existing student        |
| DELETE | `/api/students/{id}`     | Hard delete a student             |

### Teachers
| Method | Endpoint                            | Description                       |
|--------|-------------------------------------|-----------------------------------|
| GET    | `/api/teachers`                     | Retrieve all teachers             |
| GET    | `/api/teachers/{id}`                | Retrieve a specific teacher by ID |
| POST   | `/api/teachers`                     | Create a new teacher              |
| PUT    | `/api/teachers/{id}`                | Update an existing teacher        |
| DELETE | `/api/teachers/{id}`                | Soft delete a teacher             |

### Courses
| Method | Endpoint                                 | Description                       |
|--------|------------------------------------------|-----------------------------------|
| GET    | `/api/courses`                           | Retrieve all courses              |
| GET    | `/api/courses/{id}`                      | Retrieve a specific course by ID  |
| POST   | `/api/courses`                           | Create a new course               |
| PUT    | `/api/courses/{id}`                      | Update an existing course         |
| DELETE | `/api/courses/{id}`                      | Soft delete a course              |
| GET    | `/api/courses/DeletedCors`               | Retrieve all soft deleted courses |
| PUT    | `/api/courses/Restore/{id}`              | Restore a soft deleted course     |

