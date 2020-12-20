using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stori.Data;
using Stori.Models;

namespace Stori.Controllers
{
	public class HomeController : StoriCustomController
	{
		public const string VOTE_COOKIE_KEY = "Stori_VoteToken";
		public const string NODE_COOKIE_KEY = "Stori_CurrentNode";

		public HomeController(ApplicationDbContext context, ILogger<HomeController> logger) : base(context, logger)
		{
		}

		public IActionResult Index()
		{
			if(!Request.Cookies.TryGetValue(NODE_COOKIE_KEY, out var currentNodeCookie) || !Guid.TryParse(currentNodeCookie, out var currentGuid))
			{
				currentGuid = default;
			}
			var queryable = DBContext.StoriNode.AsQueryable();

			var requestedNode = queryable.SingleOrDefault(n => n.ID == currentGuid);
			if(requestedNode == null) // maybe it got deleted?
			{
				currentGuid = default;
				requestedNode = StoriApp.DefaultNode;
			}
			ViewData["controller"] = this;
			ViewData["history"] = GetHistory(currentGuid).Reverse();
			ViewData["next"] = queryable.Where(n => n != null && n.Parent == currentGuid).OrderByDescending(n => n.Votes);
			var l = queryable.Where(n => n != null && n.Parent == currentGuid).ToList();
			if(requestedNode != null)
			{
				if(TryVote(out var voteToken, requestedNode.Parent, currentGuid))
				{
					requestedNode.Votes++;
					DBContext.StoriNode.Update(requestedNode);
					DBContext.SaveChanges();
				}
				Response.Cookies.Append(VOTE_COOKIE_KEY, voteToken);
			}
			return View(requestedNode?? StoriApp.DefaultNode);
		}

		private bool TryVote(out string voteToken, Guid previousNodeGUID, Guid newNodeGUID)
		{
			var userGuid = User.GetUserGUID();
			var token = DBContext.VoteToken.Find(userGuid);
			if(token == null || // User doesn't have an active token
				!Request.Cookies.TryGetValue(VOTE_COOKIE_KEY, out voteToken) || // Client hasn't sent a token
				newNodeGUID == default)	// We're at the start of the story
			{
				// Create from scratch
				if (token == null)
				{
					token = new VoteToken
					{
						UserID = userGuid,
						Node = newNodeGUID,
						Token = Guid.NewGuid().ToString()
					};
					DBContext.Add(token);
				}
				else
				{
					token.Node = newNodeGUID;
					token.Token = Guid.NewGuid().ToString();
					DBContext.Update(token);
				}
				voteToken = token.Token;
				DBContext.SaveChanges();
				return false;
			}
			if(token.Node != previousNodeGUID)
			{
				return false;
			}
			token.Token = Guid.NewGuid().ToString();
			voteToken = token.Token;
			token.Node = newNodeGUID;
			DBContext.VoteToken.Update(token);
			DBContext.SaveChanges();
			Logger.LogInformation($"User {userGuid} voted for node {previousNodeGUID}");
			return true;
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
