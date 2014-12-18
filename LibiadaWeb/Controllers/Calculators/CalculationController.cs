﻿namespace LibiadaWeb.Controllers.Calculators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Chains;

    /// <summary>
    /// The calculation controller.
    /// </summary>
    public class CalculationController : AbstractCalculationController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The matter repository.
        /// </summary>
        private readonly MatterRepository matterRepository;

        /// <summary>
        /// The characteristic repository.
        /// </summary>
        private readonly CharacteristicTypeRepository characteristicRepository;

        /// <summary>
        /// The notation repository.
        /// </summary>
        private readonly NotationRepository notationRepository;

        /// <summary>
        /// The chain repository.
        /// </summary>
        private readonly ChainRepository chainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CalculationController"/> class.
        /// </summary>
        public CalculationController()
        {
            db = new LibiadaWebEntities();
            this.matterRepository = new MatterRepository(db);
            this.characteristicRepository = new CharacteristicTypeRepository(db);
            this.notationRepository = new NotationRepository(db);
            this.chainRepository = new ChainRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            ViewBag.dbName = DbHelper.GetDbName(db);

            IEnumerable<characteristic_type> characteristicsList =
                db.characteristic_type.Where(c => c.full_chain_applicable);

            var characteristicTypes = this.characteristicRepository.GetSelectListWithLinkable(characteristicsList);

            var links = new SelectList(db.link, "id", "name").ToList();
            links.Insert(0, new SelectListItem { Value = null, Text = "Нет" });

            var translators = new SelectList(db.translator, "id", "name").ToList();
            translators.Insert(0, new SelectListItem { Value = null, Text = "Нет" });

            ViewBag.data = new Dictionary<string, object>
                {
                    { "matters", this.matterRepository.GetSelectListWithNature() }, 
                    { "characteristicTypes", characteristicTypes }, 
                    { "notations", this.notationRepository.GetSelectListWithNature() }, 
                    { "natures", new SelectList(db.nature, "id", "name") }, 
                    { "links", links }, 
                    { "languages", new SelectList(db.language, "id", "name") }, 
                    { "translators", translators }
                };
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="characteristicIds">
        /// The characteristic ids.
        /// </param>
        /// <param name="linkIds">
        /// The link ids.
        /// </param>
        /// <param name="notationIds">
        /// The notation ids.
        /// </param>
        /// <param name="languageIds">
        /// The language ids.
        /// </param>
        /// <param name="translatorIds">
        /// The translator ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds, 
            int[] characteristicIds, 
            int?[] linkIds, 
            int[] notationIds, 
            int[] languageIds, 
            int?[] translatorIds)
        {
            matterIds = matterIds.OrderBy(m => m).ToArray();
            var characteristics = new List<List<double>>();
            var chainNames = db.matter.Where(m => matterIds.Contains(m.id)).OrderBy(m => m.id).Select(m => m.name).ToList();
            var characteristicNames = new List<string>();

            foreach (var matterId in matterIds)
            {
                characteristics.Add(new List<double>());
                for (int i = 0; i < notationIds.Length; i++)
                {
                    int notationId = notationIds[i];

                    long chainId;
                    if (db.matter.Single(m => m.id == matterId).nature_id == Aliases.NatureLiterature)
                    {
                        int languageId = languageIds[i];
                        int? translatorId = translatorIds[i];

                        chainId = db.literature_chain.Single(l => l.matter_id == matterId &&
                                    l.notation_id == notationId
                                    && l.language_id == languageId
                                    && ((translatorId == null && l.translator_id == null)
                                                    || (translatorId == l.translator_id))).id;
                    }
                    else
                    {
                        chainId = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId).id;
                    }

                    int characteristicId = characteristicIds[i];
                    int? linkId = linkIds[i];
                    if (db.characteristic.Any(c => ((linkId == null && c.link_id == null) || (linkId == c.link_id)) &&
                                              c.chain_id == chainId &&
                                              c.characteristic_type_id == characteristicId))
                    {
                        double dataBaseCharacteristic = db.characteristic.Single(c => ((linkId == null && c.link_id == null) || (linkId == c.link_id)) &&
                                                                                        c.chain_id == chainId &&
                                                                                        c.characteristic_type_id == characteristicId).value.Value;
                        characteristics.Last().Add(dataBaseCharacteristic);
                    }
                    else
                    {
                        Chain tempChain = this.chainRepository.ToLibiadaChain(chainId);
                        tempChain.FillIntervalManagers();
                        string className =
                            db.characteristic_type.Single(ct => ct.id == characteristicId).class_name;
                        IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                        var link = linkId != null ? (Link)db.link.Single(l => l.id == linkId).id : Link.None;
                        var characteristicValue = calculator.Calculate(tempChain, link);

                        var dataBaseCharacteristic = new characteristic
                        {
                            chain_id = chainId, 
                            characteristic_type_id = characteristicIds[i], 
                            link_id = linkId, 
                            value = characteristicValue, 
                            value_string = characteristicValue.ToString()
                        };
                        db.characteristic.Add(dataBaseCharacteristic);
                        db.SaveChanges();
                        characteristics.Last().Add(characteristicValue);
                    }
                }
            }

            for (int k = 0; k < characteristicIds.Length; k++)
            {
                int characteristicId = characteristicIds[k];
                int? linkId = linkIds[k];
                int notationId = notationIds[k];
                string linkName = linkId != null ? db.link.Single(l => l.id == linkId).name : string.Empty;

                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                                        linkName + " " +
                                        db.notation.Single(n => n.id == notationId).name);
            }

            var characteristicsList = new List<SelectListItem>();
            for (int i = 0; i < characteristicNames.Count; i++)
            {
                characteristicsList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = characteristicNames[i],
                    Selected = false
                });
            }

            this.TempData["result"] = new Dictionary<string, object>
                                     {
                                         { "characteristics", characteristics }, 
                                         { "chainNames", chainNames }, 
                                         { "characteristicNames", characteristicNames }, 
                                         { "characteristicIds", characteristicIds }, 
                                         { "chainIds", new List<long>(matterIds) },
                                         { "characteristicsList", characteristicsList }
                                     };

            return this.RedirectToAction("Result");
        }
    }
}