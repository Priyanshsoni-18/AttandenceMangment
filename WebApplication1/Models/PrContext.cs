using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MVC_Read_Excel.Models;

public partial class PrContext : DbContext
{
    public PrContext()
    {
    }

    public PrContext(DbContextOptions<PrContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AttAttendanceLog> AttAttendanceLogs { get; set; }

    public virtual DbSet<AttAttendanceRegister> AttAttendanceRegisters { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=OTC18;Database=pr;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AttAttendanceLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__att_Atte__3214EC07C179607E");

            entity.ToTable("att_AttendanceLog");

            entity.Property(e => e.AttDate)
                .HasColumnType("date")
                .HasColumnName("attDate");
            entity.Property(e => e.Createdby)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Createddate)
                .HasColumnType("datetime")
                .HasColumnName("createddate");
            entity.Property(e => e.LogTime).HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<AttAttendanceRegister>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Att_Atte__3214EC07C2D1A4C0");

            entity.ToTable("Att_AttendanceRegister");

            entity.Property(e => e.AttDate).HasColumnType("datetime");
            entity.Property(e => e.EarlyOutMinute).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.LateInMinute).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.OverTimeMinute).HasColumnType("numeric(18, 0)");
            entity.Property(e => e.ProcessBy)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.ProcessDate).HasColumnType("datetime");
            entity.Property(e => e.UserId)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WorkHour).HasColumnType("numeric(18, 0)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
