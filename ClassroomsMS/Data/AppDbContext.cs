using ClassroomsMS.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClassroomsMS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<ClassroomStudent> ClassroomStudents => Set<ClassroomStudent>();
    public DbSet<TeacherUser> TeacherUsers => Set<TeacherUser>();
    public DbSet<StudentUser> StudentUsers => Set<StudentUser>();
    public DbSet<Notice> Notices => Set<Notice>();
    public DbSet<NoticeImage> NoticeImages => Set<NoticeImage>();
    public DbSet<NoticeComment> NoticeComments => Set<NoticeComment>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();
    public DbSet<TaskFile> TaskFiles => Set<TaskFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Assignment maps to "Tasks" table (as per spec)
        modelBuilder.Entity<Assignment>().ToTable("Tasks");

        // Unique index on Classroom.Code
        modelBuilder.Entity<Classroom>()
            .HasIndex(c => c.Code)
            .IsUnique();

        // User -> TeacherUser (1-1)
        modelBuilder.Entity<TeacherUser>()
            .HasOne(t => t.User)
            .WithOne(u => u.TeacherUser)
            .HasForeignKey<TeacherUser>(t => t.UserId);

        // User -> StudentUser (1-1)
        modelBuilder.Entity<StudentUser>()
            .HasOne(s => s.User)
            .WithOne(u => u.StudentUser)
            .HasForeignKey<StudentUser>(s => s.UserId);

        // Classroom -> TeacherUser
        modelBuilder.Entity<Classroom>()
            .HasOne(c => c.Teacher)
            .WithMany(t => t.Classrooms)
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Classroom -> Class
        modelBuilder.Entity<Classroom>()
            .HasOne(c => c.Class)
            .WithMany(cl => cl.Classrooms)
            .HasForeignKey(c => c.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // StudentUser -> Class (nullable)
        modelBuilder.Entity<StudentUser>()
            .HasOne(s => s.Class)
            .WithMany(cl => cl.Students)
            .HasForeignKey(s => s.ClassId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // ClassroomStudent -> Classroom
        modelBuilder.Entity<ClassroomStudent>()
            .HasOne(cs => cs.Classroom)
            .WithMany(c => c.ClassroomStudents)
            .HasForeignKey(cs => cs.ClassroomId)
            .OnDelete(DeleteBehavior.Cascade);

        // ClassroomStudent -> StudentUser
        modelBuilder.Entity<ClassroomStudent>()
            .HasOne(cs => cs.StudentUser)
            .WithMany(s => s.ClassroomStudents)
            .HasForeignKey(cs => cs.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Notice -> Classroom
        modelBuilder.Entity<Notice>()
            .HasOne(n => n.Classroom)
            .WithMany(c => c.Notices)
            .HasForeignKey(n => n.ClassroomId)
            .OnDelete(DeleteBehavior.Cascade);

        // NoticeComment -> User (restrict to avoid cycles)
        modelBuilder.Entity<NoticeComment>()
            .HasOne(nc => nc.User)
            .WithMany()
            .HasForeignKey(nc => nc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Assignment -> Classroom
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Classroom)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.ClassroomId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaskFile -> StudentUser
        modelBuilder.Entity<TaskFile>()
            .HasOne(tf => tf.StudentUser)
            .WithMany(s => s.TaskFiles)
            .HasForeignKey(tf => tf.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // TaskFile -> Assignment
        modelBuilder.Entity<TaskFile>()
            .HasOne(tf => tf.Assignment)
            .WithMany(a => a.TaskFiles)
            .HasForeignKey(tf => tf.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
