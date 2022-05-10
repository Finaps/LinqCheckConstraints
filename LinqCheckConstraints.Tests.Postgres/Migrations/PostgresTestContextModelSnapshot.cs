﻿// <auto-generated />
using System;
using LinqCheckConstraints.Tests.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LinqCheckConstraints.Tests.Postgres.Migrations
{
    [DbContext(typeof(PostgresTestContext))]
    partial class PostgresTestContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("LinqCheckConstraints.Tests.TestEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<byte>("Byte")
                        .HasColumnType("smallint");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTimeOffset>("DateTimeOffset")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Int")
                        .HasColumnType("integer");

                    b.Property<long>("Long")
                        .HasColumnType("bigint");

                    b.Property<short>("Sbyte")
                        .HasColumnType("smallint");

                    b.Property<short>("Short")
                        .HasColumnType("smallint");

                    b.Property<string>("String")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Uint")
                        .HasColumnType("bigint");

                    b.Property<decimal>("Ulong")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("Ushort")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("TestEntity");

                    b.HasCheckConstraint("CK_TestEntity_ByteSmallerThanFive", "(  \"Byte\" < 5)");
                });
#pragma warning restore 612, 618
        }
    }
}