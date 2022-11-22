﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Utg.LegalService.Dal.SqlContext;

#nullable disable

namespace Utg.LegalService.Dal.Migrations
{
    [DbContext(typeof(UtgContext))]
    partial class UtgContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("ProductVersion", "6.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Utg.LegalService.Common.Models.Domain.Task", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("AuthorFullName")
                        .HasColumnType("text");

                    b.Property<int>("AuthorUserProfileId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DeadlineDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<DateTime>("LastChangeDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("PerformerFullName")
                        .HasColumnType("text");

                    b.Property<int?>("PerformerUserProfileId")
                        .HasColumnType("integer");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Tasks", "public");
                });

            modelBuilder.Entity("Utg.LegalService.Common.Models.Domain.TaskAttachment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid>("FileId")
                        .HasColumnType("uuid");

                    b.Property<string>("FileName")
                        .HasColumnType("text");

                    b.Property<long>("FileSizeInBytes")
                        .HasColumnType("bigint");

                    b.Property<int>("TaskId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserProfileId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TaskId");

                    b.ToTable("TaskAttachments", "public");
                });

            modelBuilder.Entity("Utg.LegalService.Common.Models.Domain.TaskComment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TaskId")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.Property<int>("UserProfileId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TaskId");

                    b.ToTable("TaskComments", "public");
                });

            modelBuilder.Entity("Utg.LegalService.Common.Models.Domain.UserProfileAgregate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("CompanyId")
                        .HasColumnType("integer");

                    b.Property<string>("CompanyName")
                        .HasColumnType("text");

                    b.Property<int?>("DepartmentId")
                        .HasColumnType("integer");

                    b.Property<string>("DepartmentName")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DismissalDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FullName")
                        .HasColumnType("text");

                    b.Property<int?>("HeadUserProfileId")
                        .HasColumnType("integer");

                    b.Property<bool>("IsRemoved")
                        .HasColumnType("boolean");

                    b.Property<int?>("ManagerPositionId")
                        .HasColumnType("integer");

                    b.Property<int?>("PositionId")
                        .HasColumnType("integer");

                    b.Property<string>("PositionName")
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("TabN")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<int>("UserProfileId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("UserProfileAgregates", "public");
                });

            modelBuilder.Entity("Utg.LegalService.Common.Models.Domain.TaskAttachment", b =>
                {
                    b.HasOne("Utg.LegalService.Common.Models.Domain.Task", "Task")
                        .WithMany("TaskAttachments")
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Task");
                });

            modelBuilder.Entity("Utg.LegalService.Common.Models.Domain.TaskComment", b =>
                {
                    b.HasOne("Utg.LegalService.Common.Models.Domain.Task", "Task")
                        .WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Task");
                });

            modelBuilder.Entity("Utg.LegalService.Common.Models.Domain.Task", b =>
                {
                    b.Navigation("TaskAttachments");
                });
#pragma warning restore 612, 618
        }
    }
}
