using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Stori.Data;

namespace Stori.Data
{
	[Table("StoriNode")]
	public class StoriNode
	{
		[Key]
		public Guid ID { get; set; }
		[Required]
		public Guid Parent { get; set; }
		public DateTime CreationDate { get; set; }
		public Guid Creator { get; set; }
		[MinLength(6)]
		[MaxLength(256)]
		[Required]
		public string Action { get; set; }
		[MinLength(6)]
		[MaxLength(2048)]
		[Required]
		public string Content { get; set; }
		public uint Votes { get; set; }

		public static string MDToHTML(string md)
		{
			return md.Replace("\n", "<br/><br/>");
		}

		public override string ToString() => Action;
	}

	public class ApplicationDbContext : IdentityDbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{	
		}

		public DbSet<Stori.Data.StoriNode> StoriNode { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<StoriNode>()
				.ToTable("StoriNode");
			base.OnModelCreating(modelBuilder);
		}
	}
}
