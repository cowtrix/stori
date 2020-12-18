using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stori.Data;
using Stori.Models;

namespace Stori.Controllers
{
	public class HomeController : Controller
	{
		public const string NODE_COOKIE_KEY = "Stori_CurrentNode";
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext m_context;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
		{
			_logger = logger;
			m_context = context;
			m_context.Database.EnsureCreated();
		}

		public IActionResult Index()
		{
			if(!Request.Cookies.TryGetValue(NODE_COOKIE_KEY, out var currentNodeCookie) || !Guid.TryParse(currentNodeCookie, out var currentGuid))
			{
				currentGuid = default;
			}
			var queryable = m_context.StoriNode.AsQueryable();
			ViewData["history"] = GetHistory(currentGuid).Reverse();
			ViewData["next"] = queryable.Where(n => n != null && n.Parent == currentGuid).OrderByDescending(n => n.Votes);
			var l = queryable.Where(n => n != null && n.Parent == currentGuid).ToList();
			var requestedNode = queryable.SingleOrDefault(n => n.ID == currentGuid) ;
			if(requestedNode != null)
			{
				requestedNode.Votes++;
				m_context.StoriNode.Update(requestedNode);
				m_context.SaveChanges();
			}
			return View(requestedNode?? StoriApp.DefaultNode);
		}

		private IEnumerable<StoriNode> GetHistory(Guid currentGuid)
		{
			if (currentGuid == default)
			{
				yield return StoriApp.DefaultNode;
				yield break;
			}
			var n = m_context.StoriNode.Find(currentGuid);
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

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
