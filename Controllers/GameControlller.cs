using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace caso_de_uso_6_ejercer_turno.Controllers
{
    public class GameControlller : Controller
    {
        // GET: GameControlller
        public ActionResult Index()
        {
            return View();
        }

        // GET: GameControlller/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: GameControlller/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: GameControlller/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: GameControlller/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: GameControlller/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: GameControlller/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: GameControlller/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
