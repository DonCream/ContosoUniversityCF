using System;
using ContosoUniversityCF.Models;

namespace ContosoUniversityCF.DAL
{
    public class UnitOfWork : IDisposable
    {
        private readonly SchoolContext _context = new SchoolContext();
        private GenericRepository<Department> _departmentRepository;
        private GenericRepository<Course> _courseRepository;

        public GenericRepository<Department> DepartmentRepository
        {
            get {
                return _departmentRepository ?? (_departmentRepository = new GenericRepository<Department>(_context));
            }
        }

        public GenericRepository<Course> CourseRepository
        {
            get { return _courseRepository ?? (_courseRepository = new GenericRepository<Course>(_context)); }
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}