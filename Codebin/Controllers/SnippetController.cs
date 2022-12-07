using Codebin.Models;
using Codebin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codebin.Controllers
{
    [Authorize]
    public class SnippetController : Controller
    {
        private readonly MongoDBService _mongoDBService;

        public SnippetController(MongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        // GET: SnippetController
        public async Task<ActionResult> Index()
        {
            var snippets = await _mongoDBService.GetAsync();
            return View(snippets);
        }

        // GET: SnippetController/Details/5
        public async Task<ActionResult> Details(string id)
        {
            Snippet snippet = await _mongoDBService.GetOneAsync(id);
            return View(snippet);
        }

        // GET: SnippetController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SnippetController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([FromForm] Snippet snippet)
        {
            try
            {
                snippet.Tags = snippet.Tags[0].Split(",").ToList<string>();
                await _mongoDBService.CreateAsync(snippet);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: SnippetController/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            Snippet snippet = await _mongoDBService.GetOneAsync(id);
            return View(snippet);
        }

        // POST: SnippetController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Snippet snippet, int id, IFormCollection collection)
        {
            try
            {
                snippet.Tags = snippet.Tags[0].Split(",").ToList<string>();
                await _mongoDBService.UpdateAsync(snippet);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: SnippetController/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            var snippet = await _mongoDBService.GetOneAsync(id);
            return View(snippet);
        }

        // POST: SnippetController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id, IFormCollection collection)
        {
            try
            {
                await _mongoDBService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: SnippetController/Search?query=code
        [HttpGet]
        public async Task<ActionResult> Search(string query)
        {
            var snippets = await _mongoDBService.FindAsync(query);
            return View(snippets);
        }
    }
}
