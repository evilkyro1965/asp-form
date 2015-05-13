using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMusicStore.Models;

namespace Basic.Controllers
{
    public class MusicStoreController : Controller
    {
        private MusicStoreDB db = new MusicStoreDB();

        // GET: MusicStore
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            ViewBag.ArtistId = new SelectList(db.Artists, "ArtistId", "Name");
            ViewBag.GenreId = new SelectList(db.Genres, "GenreId", "Name");
            IEnumerable<SelectListItem> arrGenres = new SelectList(db.Genres, "GenreId", "Name").ToList();
            ViewBag.arrGenres = arrGenres;
            ViewBag.nama = "fahrur";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "AlbumId,GenreId,ArtistId,Title,Price,AlbumArtUrl")] Album album)
        {
            if (ModelState.IsValid)
            {
                db.Albums.Add(album);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ArtistId = new SelectList(db.Artists, "ArtistId", "Name", album.ArtistId);
            ViewBag.GenreId = new SelectList(db.Genres, "GenreId", "Name", album.GenreId);
            IEnumerable<SelectListItem> arrGenres = new SelectList(db.Genres, "GenreId", "Name").ToList();
            ViewBag.arrGenres = arrGenres;
            return View(album);
        }

    }
}