using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;
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
		public bool Stub { get; set; }

		public static string MDToHTML(string str, string pclass)
		{
			if (string.IsNullOrEmpty(str))
			{
				return str;
			}
			// Firstly escape any explicit html to prevent injection
			str = System.Web.HttpUtility.HtmlEncode(str);
			// While this is nice, we want quotation marks to work
			str = str.Replace("&quot;", "\"");

			// Replace newlines
			var newlineRgx = @"(\n|\r\n)+";
			var pstart = $"<p class=\"{pclass}\">&emsp;";
			Match match = Regex.Match(str, newlineRgx);
			while (match.Success)
			{
				// Handle match here...
				try
				{
					var txt = match.Groups[1].Value;
					var link = match.Groups[2].Value;
					str = str.Substring(0, match.Index) + "</p>" + pstart + str.Substring(match.Index + match.Length);
				}
				catch { }
				match = Regex.Match(str, newlineRgx);
			}
			// Finally replace linebreaks with <br/>
			return pstart
				+ str.Replace("\t", "&emsp;")
				+ "</p>";
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
