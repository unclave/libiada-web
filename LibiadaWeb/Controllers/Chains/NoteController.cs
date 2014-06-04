﻿namespace LibiadaWeb.Controllers.Chains
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web.Mvc;

    /// <summary>
    /// The note controller.
    /// </summary>
    public class NoteController : Controller
    {
        /// <summary>
        /// The db.
        /// </summary>
        private LibiadaWebEntities db = new LibiadaWebEntities();

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var note = this.db.note.Include(n => n.notation).Include(n => n.tie);
            return View(note.ToList());
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            note note = this.db.note.Find(id);
            if (note == null)
            {
                return this.HttpNotFound();
            }

            return View(note);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name");
            this.ViewBag.tie_id = new SelectList(this.db.tie, "id", "name");
            return this.View();
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="note">
        /// The note.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,value,description,name,notation_id,created,numerator,denominator,ticks,onumerator,odenominator,triplet,priority,tie_id,modified")] note note)
        {
            if (this.ModelState.IsValid)
            {
                this.db.note.Add(note);
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", note.notation_id);
            this.ViewBag.tie_id = new SelectList(this.db.tie, "id", "name", note.tie_id);
            return View(note);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            note note = this.db.note.Find(id);
            if (note == null)
            {
                return this.HttpNotFound();
            }

            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", note.notation_id);
            this.ViewBag.tie_id = new SelectList(this.db.tie, "id", "name", note.tie_id);
            return View(note);
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="note">
        /// The note.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,value,description,name,notation_id,created,numerator,denominator,ticks,onumerator,odenominator,triplet,priority,tie_id,modified")] note note)
        {
            if (this.ModelState.IsValid)
            {
                this.db.Entry(note).State = EntityState.Modified;
                this.db.SaveChanges();
                return this.RedirectToAction("Index");
            }

            this.ViewBag.notation_id = new SelectList(this.db.notation, "id", "name", note.notation_id);
            this.ViewBag.tie_id = new SelectList(this.db.tie, "id", "name", note.tie_id);
            return View(note);
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            note note = this.db.note.Find(id);
            if (note == null)
            {
                return this.HttpNotFound();
            }

            return View(note);
        }

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            note note = this.db.note.Find(id);
            this.db.note.Remove(note);
            this.db.SaveChanges();
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}