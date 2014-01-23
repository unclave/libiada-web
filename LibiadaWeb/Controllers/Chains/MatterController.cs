﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.SimpleTypes;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{
    public class MatterController : Controller
    {
        private readonly LibiadaWebEntities db;
        private readonly ElementRepository elementRepository;
        private readonly DnaChainRepository dnaChainRepository;
        private readonly NotationRepository notationRepository;
        private readonly PieceTypeRepository pieceTypeRepository;
        private readonly LiteratureChainRepository literatureChainRepository;

        public MatterController()
        {
            db = new LibiadaWebEntities();
            elementRepository = new ElementRepository(db);
            dnaChainRepository = new DnaChainRepository(db);
            notationRepository = new NotationRepository(db);
            pieceTypeRepository = new PieceTypeRepository(db);
            literatureChainRepository = new LiteratureChainRepository(db);
        }

        //
        // GET: /Matter/

        public ActionResult Index()
        {
            var matter = db.matter.Include("nature");
            return View(matter.ToList());
        }

        //
        // GET: /Matter/Details/5

        public ActionResult Details(long id)
        {
            matter matter = db.matter.Single(m => m.id == id);
            if (matter == null)
            {
                return HttpNotFound();
            }
            return View(matter);
        }

        //
        // GET: /Matter/Create

        public ActionResult Create()
        {
            ViewBag.data = new Dictionary<string, object>
                {
                    {"notations", notationRepository.GetSelectListWithNature()},
                    {"pieceTypes", pieceTypeRepository.GetSelectListWithNature()},
                    {"languages", new SelectList(db.language, "id", "name")},
                    {"remoteDbs", new SelectList(db.remote_db, "id", "name")},
                    {"natures", new SelectList(db.nature, "id", "name")},
                    {"natureLiterature", Aliases.NatureLiterature}
                };
            return View();
        }

        //
        // POST: /Matter/Create

        [HttpPost]
        public ActionResult Create(matter matter, int notationId, int pieceTypeId, int? remoteDbId, String remoteId, bool localFile, int? languageId, bool? original)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    String webApiId = String.Empty;
                    // Initialize the stream
                    Stream fileStream;
                    if (localFile)
                    {
                        var file = Request.Files[0];

                        if (file == null || file.ContentLength == 0)
                        {
                            throw new ArgumentNullException("Файл цепочки не задан или пуст");
                        }
                        fileStream = file.InputStream;
                    }
                    else
                    {
                        webApiId = NcbiHelper.GetId(remoteId);
                        fileStream = NcbiHelper.GetFile(webApiId);
                    }

                    var input = new byte[fileStream.Length];

                    // Read the file into the byte array
                    fileStream.Read(input, 0, (int) fileStream.Length);

                    string stringChain = matter.nature_id == Aliases.NatureGenetic
                        ? Encoding.ASCII.GetString(input)
                        : Encoding.UTF8.GetString(input);

                    BaseChain libiadaChain;
                    long[] alphabet;
                    matter.created = DateTime.Now;
                    switch (matter.nature_id)
                    {
                        case Aliases.NatureGenetic:
                            //отделяем заголовок fasta файла от цепочки
                            string[] splittedFasta = stringChain.Split(new[] {'\n', '\r'});
                            var chainStringBuilder = new StringBuilder();
                            String fastaHeader = splittedFasta[0];
                            for (int j = 1; j < splittedFasta.Length; j++)
                            {
                                chainStringBuilder.Append(splittedFasta[j]);
                            }

                            string resultStringChain = DataTransformers.CleanFastaFile(chainStringBuilder.ToString());

                            libiadaChain = new BaseChain(resultStringChain);

                            if (!elementRepository.ElementsInDb(libiadaChain.Alphabet, notationId))
                            {
                                throw new Exception(
                                    "В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                            }



                            db.matter.AddObject(matter);
                            db.SaveChanges();

                            var dnaChain = new chain
                            {
                                dissimilar = false,
                                notation_id = notationId,
                                piece_type_id = pieceTypeId,
                                created = new DateTimeOffset(DateTime.Now),
                                matter_id = matter.id,
                                remote_db_id = remoteDbId,
                                remote_id = remoteId
                            };

                            alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, dnaChain.notation_id, false);
                            dnaChainRepository.Insert(dnaChain, fastaHeader, Convert.ToInt32(webApiId), alphabet,
                                libiadaChain.Building);
                            break;
                        case Aliases.NatureMusic:
                            var doc = new XmlDocument();
                            doc.LoadXml(stringChain);
                            //MusicXmlParser parser = new MusicXmlParser();
                            //parser.Execute(doc, "test");
                            //ScoreTrack tempTrack = parser.ScoreModel;

                            break;
                        case Aliases.NatureLiterature:
                            string[] text = stringChain.Split('\n');
                            for (int l = 0; l < text.Length - 1; l++)
                            {
                                // убираем \r
                                text[l] = text[l].Substring(0, text[l].Length - 1);
                            }

                            libiadaChain = new BaseChain(text.Length - 1);
                            // в конце файла всегда пустая строка поэтому последний элемент не считаем
                            for (int i = 0; i < text.Length - 1; i++)
                            {
                                libiadaChain.Add(new ValueString(text[i]), i);
                            }

                            db.matter.AddObject(matter);
                            db.SaveChanges();

                            var literatureChain = new chain
                            {
                                dissimilar = false,
                                notation_id = notationId,
                                piece_type_id = pieceTypeId,
                                created = new DateTimeOffset(DateTime.Now),
                                matter_id = matter.id,
                                remote_db_id = remoteDbId,
                                remote_id = remoteId
                            };

                            alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, literatureChain.notation_id, true);

                            literatureChainRepository.Insert(literatureChain, (bool)original, (int)languageId, alphabet, libiadaChain.Building);

                            break;
                    }
                }
                catch (Exception e)
                {
                    ViewBag.nature_id = new SelectList(db.nature, "id", "name");
                    ViewBag.remote_db_id = new SelectList(db.remote_db, "id", "name");
                    ViewBag.notation_id = new SelectList(db.notation, "id", "name");
                    ViewBag.language_id = new SelectList(db.language, "id", "name");
                    ModelState.AddModelError("Error", e.Message);
                    return View(matter);
                }
                return RedirectToAction("Index");
            }

            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        //
        // GET: /Matter/Edit/5

        public ActionResult Edit(long id)
        {
            matter matter = db.matter.Single(m => m.id == id);
            if (matter == null)
            {
                return HttpNotFound();
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        //
        // POST: /Matter/Edit/5

        [HttpPost]
        public ActionResult Edit(matter matter)
        {
            if (ModelState.IsValid)
            {
                db.matter.Attach(matter);
                db.ObjectStateManager.ChangeObjectState(matter, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.nature_id = new SelectList(db.nature, "id", "name", matter.nature_id);
            return View(matter);
        }

        //
        // GET: /Matter/Delete/5

        public ActionResult Delete(long id)
        {
            matter matter = db.matter.Single(m => m.id == id);
            if (matter == null)
            {
                return HttpNotFound();
            }
            return View(matter);
        }

        //
        // POST: /Matter/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(long id)
        {
            matter matter = db.matter.Single(m => m.id == id);
            db.matter.DeleteObject(matter);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        public ActionResult ImportFailure()
        {
            ViewBag.failedElement = TempData["failedElement"];
            return View();
        }
    }
}