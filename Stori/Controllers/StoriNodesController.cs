using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
            if(!user.Identity.IsAuthenticated)
			{
                return default;
			}
            return Guid.Parse(user.Claims.First().Value);
        }

    }

    [Authorize]
    [Route(ROUTE)]
    public class StoriNodesController : StoriCustomController
    {
        public const string ROUTE = "r";

		public StoriNodesController(ApplicationDbContext context, ILogger<StoriNodesController> logger) : 
            base(context, logger)
		{
		}

		[HttpGet]
        public async Task<IActionResult> Index()
        {
            var userGuid = User.GetUserGUID();
            var enumerable = DBContext.StoriNode.AsQueryable();
            ViewData["controller"] = this;
            return View(enumerable.Where(n => n.Creator == userGuid).ToList());
        }

        [HttpGet("create")]
        public IActionResult Create([FromQuery]Guid? parent)
        {
            if(parent == null)
			{
                return BadRequest("Parent cannot be null");
			}
            var parentNode = DBContext.StoriNode.Find(parent.Value);
            if(parentNode == null && parent.Value != default)
			{
                return BadRequest($"Couldn't find parent with ID {parent.Value}");
			}
            parentNode = parentNode ?? StoriApp.DefaultNode;
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
        public async Task<IActionResult> Create([Bind(nameof(StoriNode.Action), nameof(StoriNode.Content), nameof(StoriNode.Parent))] StoriNode storiNode)
        {
            if (ModelState.IsValid)
            {
                storiNode.ID = Guid.NewGuid();
                storiNode.Action = storiNode.Action.Trim();
                storiNode.Content = storiNode.Content.Trim();
                storiNode.Creator = Guid.Parse(User.Claims.First().Value);
                storiNode.CreationDate = DateTime.UtcNow;
                storiNode.Votes = 0;
                DBContext.Add(storiNode);
                await DBContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(storiNode);
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
