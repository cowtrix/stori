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
	[Table(nameof(VoteToken))]
	public class VoteToken
	{
		[Key]
		public Guid UserID { get; set; }
		public Guid Node { get; set; }
		public string Token { get; set; }
	}

	[Table(nameof(StoriNode))]
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

		public static string MDToHTML(string str)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			// Firstly escape any explicit html to prevent injection
			str = System.Web.HttpUtility.HtmlEncode(str);
			// While this is nice, we want quotation marks to work
			str = str.Replace("&quot;", "\"");
			// Finally replace linebreaks with <br/>
			return str
				.Replace(Environment.NewLine, "</br>")
				.Replace("\n", "</br>")
				.Replace("\t", "&emsp;");
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
		public DbSet<Stori.Data.VoteToken> VoteToken { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<StoriNode>()
				.ToTable(nameof(StoriNode));
			modelBuilder.Entity<VoteToken>()
				.ToTable(nameof(VoteToken));
			base.OnModelCreating(modelBuilder);
		}
	}
}
