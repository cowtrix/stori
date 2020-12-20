using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stori.Data;

namespace Stori.Controllers
{
	public class StoriCustomController: Controller
	{
        protected readonly ApplicationDbContext DBContext;
        protected readonly ILogger Logger;

        public StoriCustomController(ApplicationDbContext context, ILogger logger)
		{
            DBContext = context;
            DBContext.Database.EnsureCreated();
            Logger = logger;
		}

        public int GetSubDocumentCount(Guid id)
        {
            int counter = 0;
            var qu = DBContext.StoriNode.AsQueryable();
            foreach (var c in qu.Where(n => n.Parent == id).ToList())
            {
                counter++;
                counter += GetSubDocumentCount(c.ID);
            }
            return counter;
        }
    }
}
