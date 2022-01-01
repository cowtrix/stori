using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stori.Data;

namespace Stori.Controllers
{
	public static class ControllerExtensions
	{
		public static Guid GetUserGUID(this ClaimsPrincipal user)
		{
			if (!user.Identity.IsAuthenticated)
			{
				return default;
			}
			return Guid.Parse(user.Claims.First().Value);
		}

	}

	[AllowAnonymous]
	[Route(ROUTE)]
	public class StoriNodesController : StoriCustomController
	{
		public enum eSortMode
		{
			Date,
			Votes,
		}

		public const string ROUTE = "r";
		private IEmailSender m_emailManager;

		public StoriNodesController(ApplicationDbContext context, ILogger<StoriNodesController> logger, IEmailSender emailManager) :
			base(context, logger)
		{
			m_emailManager = emailManager;
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> Index([FromQuery] eSortMode sortmode = eSortMode.Date)
		{
			if(!User.Identity.IsAuthenticated)
			{
				return Redirect("~/");
			}
			var userGuid = User.GetUserGUID();
			var enumerable = DBContext.StoriNode.AsQueryable();
			ViewData["controller"] = this;
			ViewData["sortmode"] = sortmode;
			return View(enumerable.Where(n => n.Creator == userGuid).ToList());
		}

		[HttpGet("create")]
		public IActionResult Create([FromQuery] Guid? parent)
		{
			if (parent == null)
			{
				return BadRequest("Parent cannot be null");
			}
			var parentNode = DBContext.StoriNode.Find(parent.Value);
			if (parentNode == null && parent.Value != default)
			{
				return BadRequest($"Couldn't find parent with ID {parent.Value}");
			}
			parentNode = parentNode ?? StoriApp.DefaultNode;
			if(parentNode.Stub)
			{
			  return BadRequest($"Node with ID {parentNode.ID} is a stub and cannot have sub-stories.");
			}
			ViewData["parent"] = parentNode;
			var newNode = new StoriNode
			{
				Parent = parentNode.ID,
			};
			return View(newNode);
		}

		// POST: StoriNodes/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to, for 
		// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost("create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind(nameof(StoriNode.Action), nameof(StoriNode.Content), nameof(StoriNode.Parent), nameof(StoriNode.Stub))] StoriNode storiNode)
		{
			if (ModelState.IsValid)
			{
				storiNode.ID = Guid.NewGuid();
				storiNode.Action = storiNode.Action.Trim();
				storiNode.Content = storiNode.Content.Trim();
				storiNode.CreationDate = DateTime.UtcNow;
				storiNode.Votes = 0;
				if (User.Identity.IsAuthenticated)
				{
					storiNode.Creator = Guid.Parse(User.Claims.First().Value);
				}
				DBContext.Add(storiNode);
				await DBContext.SaveChangesAsync();
				new Task(async () =>
				{
					Logger.LogInformation($"Sending new submission info to {StoriApp.ContactEmail}");
					await m_emailManager.SendEmailAsync(StoriApp.ContactEmail, "New Stori Submission",
						 NodeToHTML(storiNode));
				}).Start();
				return RedirectToAction(nameof(Index));
			}
			return View(storiNode);
		}

		private static string NodeToHTML(StoriNode node)
		{
			return "<h1>New Submission:</h1>" +
				$"<p>By {node.Creator}</p>" +
				$"<p>{node.Action}</p>" +
				$"<p>{node.Content}</p>";
		}

		private bool StoriNodeExists(Guid id)
		{
			return DBContext.StoriNode.Any(e => e.ID == id);
		}

		private IEnumerable<StoriNode> GetHistory(Guid currentGuid)
		{
			if (currentGuid == default)
			{
				yield return StoriApp.DefaultNode;
				yield break;
			}
			var n = DBContext.StoriNode.Find(currentGuid);
			if (n == null)
			{
				yield break;
			}
			yield return n;
			foreach (var parent in GetHistory(n.Parent))
			{
				yield return parent;
			}
		}
	}
}
