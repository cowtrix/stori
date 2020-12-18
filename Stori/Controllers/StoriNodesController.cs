using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stori.Data;

namespace Stori.Controllers
{
    [Authorize]
    [Route(ROUTE)]
    public class StoriNodesController : Controller
    {
        public const string ROUTE = "r";
        private readonly ApplicationDbContext m_context;

        public StoriNodesController(ApplicationDbContext context)
        {
            m_context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userGuid = Guid.Parse(User.Claims.First().Value); ;
            var enumerable = m_context.StoriNode.AsQueryable();
            return View(enumerable.Where(n => n.Creator == userGuid).ToList());
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storiNode = await m_context.StoriNode
                .FirstOrDefaultAsync(m => m.ID == id);
            if (storiNode == null)
            {
                return NotFound();
            }

            return View(storiNode);
        }

        [HttpGet("create")]
        public IActionResult Create([FromQuery]Guid? parent)
        {
            if(parent == null)
			{
                return BadRequest("Parent cannot be null");
			}
            var parentNode = m_context.StoriNode.Find(parent.Value);
            if(parentNode == null)
			{
                return NotFound();
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
        public async Task<IActionResult> Create([Bind(nameof(StoriNode.Action), nameof(StoriNode.Content), nameof(StoriNode.Parent))] StoriNode storiNode)
        {
            if (ModelState.IsValid)
            {
                storiNode.ID = Guid.NewGuid();
                storiNode.Creator = Guid.Parse(User.Claims.First().Value);
                storiNode.CreationDate = DateTime.UtcNow;
                storiNode.Votes = 0;
                m_context.Add(storiNode);
                await m_context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(storiNode);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute]Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storiNode = await m_context.StoriNode.FindAsync(id);
            if (storiNode == null)
            {
                return NotFound();
            }
            var parentNode = m_context.StoriNode.Find(storiNode.Parent) ?? StoriApp.DefaultNode;
            ViewData["parent"] = parentNode;
            return View(storiNode);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromRoute] Guid id, [Bind("ID,Action,Content,Votes")] StoriNode storiNode)
        {
            if (id != storiNode.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    m_context.Update(storiNode);
                    await m_context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StoriNodeExists(storiNode.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(storiNode);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete([FromRoute]Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var storiNode = await m_context.StoriNode
                .FirstOrDefaultAsync(m => m.ID == id);
            if (storiNode == null)
            {
                return NotFound();
            }

            return View(storiNode);
        }

        // POST: StoriNodes/Delete/5
        [HttpPost("delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var storiNode = await m_context.StoriNode.FindAsync(id);
            m_context.StoriNode.Remove(storiNode);
            await m_context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StoriNodeExists(Guid id)
        {
            return m_context.StoriNode.Any(e => e.ID == id);
        }
    }
}
