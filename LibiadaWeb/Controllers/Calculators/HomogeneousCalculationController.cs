﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Catalogs;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Calculators
{
    public class HomogeneousCalculationController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly NotationRepository notationRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly ChainRepository chainRepository;


        public HomogeneousCalculationController()
        {
            matterRepository = new MatterRepository(db);
            characteristicRepository = new CharacteristicTypeRepository(db);
            notationRepository = new NotationRepository(db);
            linkUpRepository = new LinkUpRepository(db);
            chainRepository = new ChainRepository(db);
        }

        //
        // GET: /Transformation/

        public ActionResult Index()
        {
            ViewBag.characteristics = db.characteristic_type.ToList();

            ViewBag.linkUps = db.link_up.ToList();
            ViewBag.notations = db.notation.ToList();
            ViewBag.objects = db.matter.Include("chain").ToList();
            IEnumerable<characteristic_type> characteristics =
                db.characteristic_type.Where(c => new List<int> { 2, 4, 6, 7 }.Contains(c.characteristic_applicability_id));
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(characteristics, null);
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);
            ViewBag.language_id = new SelectList(db.language, "id", "name");
            
            return View();
        }

        [HttpPost]
        public ActionResult Index(long[] matterIds, int[] characteristicIds, int[] linkUpIds, int notationId, int languageId, bool isSort, bool theoretical)
        {

            List<List<List<KeyValuePair<int, double>>>> characteristics = new List<List<List<KeyValuePair<int, double>>>>();
            List<string> chainNames = new List<string>();
            List<List<string>> elementNames = new List<List<string>>();
            List<string> characteristicNames = new List<string>();
            List<Chain> libiadaChains = new List<Chain>();

            for (int w = 0; w < matterIds.Length; w++)
            {
                long matterId = matterIds[w];
                chainNames.Add(db.matter.Single(m => m.id == matterId).name);
                elementNames.Add(new List<string>());
                characteristics.Add(new List<List<KeyValuePair<int, double>>>());

                chain dbChain;
                if (db.matter.Single(m => m.id == matterId).nature_id == 3)
                {
                    long chainId =
                        db.literature_chain.Single(
                            l => l.matter_id == matterId && l.notation_id == notationId && l.language_id == languageId)
                          .id;
                    dbChain = db.chain.Single(c => c.id == chainId);
                }
                else
                {
                    dbChain = db.chain.Single(c => c.matter_id == matterId && c.notation_id == notationId);
                }

                matter matter = db.matter.Single(m => m.id == matterId);
                chain chain = matter.chain.Single(c => c.notation_id == notationId);
                libiadaChains.Add(chainRepository.FromDbChainToLibiadaChain(chain));

                for (int i = 0; i < characteristicIds.Length; i++)
                {
                    characteristics.Last().Add(new List<KeyValuePair<int, double>>());
                    int characteristicId = characteristicIds[i];
                    int linkUpId = linkUpIds[i];

                    String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                    ICharacteristicCalculator calculator = CharacteristicsFactory.Create(className);
                    LinkUp linkUp = (LinkUp) db.link_up.Single(l => l.id == linkUpId).id;


                    for (int j = 0; j < libiadaChains[w].Alphabet.Power; j++)
                    {
                        long elementId = dbChain.alphabet.Single(a => a.number == j + 1).element_id;

                        UniformChain tempChain = libiadaChains[w].GetUniformChain(j);
                        elementNames.Last().Add(libiadaChains[w].Alphabet[j].ToString());

                        if (db.homogeneous_characteristic.Any(b =>
                                                              b.chain_id == dbChain.id &&
                                                              b.characteristic_type_id == characteristicId &&
                                                              b.element_id == elementId && b.link_up_id == linkUpId))
                        {
                            characteristics.Last().Last().Add(new KeyValuePair<int, double>(j, (double)db.homogeneous_characteristic.Single(b =>
                                                                                                                   b.chain_id == dbChain.id &&
                                                                                                                   b.characteristic_type_id == characteristicId &&
                                                                                                                   b.element_id ==elementId &&
                                                                                                                   b.link_up_id == linkUpId).value));
                        }
                        else
                        {
                            double value = calculator.Calculate(tempChain, linkUp);
                            characteristics.Last().Last().Add(new KeyValuePair<int, double>(j, value));
                            homogeneous_characteristic currentCharacteristic = new homogeneous_characteristic();
                            currentCharacteristic.id =
                                db.ExecuteStoreQuery<long>("SELECT seq_next_value('characteristics_id_seq')").First();
                            currentCharacteristic.chain_id = dbChain.id;
                            currentCharacteristic.characteristic_type_id = characteristicId;
                            currentCharacteristic.link_up_id = linkUpId;
                            currentCharacteristic.element_id = elementId;
                            currentCharacteristic.value = value;
                            currentCharacteristic.value_string = value.ToString();
                            currentCharacteristic.creation_date = DateTime.Now;
                            db.homogeneous_characteristic.AddObject(currentCharacteristic);
                            db.SaveChanges();
                        }
                    }
                }

            }

            for (int k = 0; k < characteristicIds.Length; k++)
            {
                int characteristicId = characteristicIds[k];
                int linkUpId = linkUpIds[k];
                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                                        db.link_up.Single(l => l.id == linkUpId).name + " " +
                                        db.notation.Single(n => n.id == notationId).name);
            }

            if (isSort)
            {
                for (int f = 0; f < matterIds.Length; f++)
                {
                    for (int p = 0; p < characteristics.Count; p++)
                    {
                        SortKeyValuePairList(characteristics[f][p]);
                    }
                }
                    
            }

            List<List<double>> teoreticalRanks = new List<List<double>>();

            if (theoretical)
            {
                for (int g = 0; g < matterIds.Length; g++)
                {
                    teoreticalRanks.Add(new List<double>());
                    ICharacteristicCalculator countCalculator = CharacteristicsFactory.Create("Count");
                    List<int> counts = new List<int>();
                    for (int f = 0; f < libiadaChains[g].Alphabet.Power; f++)
                    {
                        counts.Add((int)countCalculator.Calculate(libiadaChains[g].GetUniformChain(f), LinkUp.End));
                    }

                    ICharacteristicCalculator frequencyCalculator = CharacteristicsFactory.Create("Probability");
                    List<double> frequency = new List<double>();
                    for (int f = 0; f < libiadaChains[g].Alphabet.Power; f++)
                    {
                        frequency.Add(frequencyCalculator.Calculate(libiadaChains[g].GetUniformChain(f), LinkUp.End));
                    }

                    double maxFrequency = frequency.Max();
                    double K = 1 / Math.Log(counts.Max());
                    double B = (K / maxFrequency) - 1;
                    int i = 1;
                    double Plow = libiadaChains[g].Length;
                    double P = K / (B + i);
                    while (P >= (1 / Plow))
                    {
                        teoreticalRanks.Last().Add(P);
                        i++;
                        P = K / (B + i);
                    }
                }
                
            }

            TempData["characteristics"] = characteristics;
            TempData["chainNames"] = chainNames;
            TempData["elementNames"] = elementNames;
            TempData["characteristicNames"] = characteristicNames;
            TempData["characteristicIds"] = characteristicIds;
            TempData["matterIds"] = matterIds;
            TempData["teoreticalRanks"] = teoreticalRanks;
            return RedirectToAction("Result");
        }

        private void SortKeyValuePairList(List<KeyValuePair<int, double>> arrayForSort)
        {
            arrayForSort.Sort(
                delegate(KeyValuePair<int, double> firstPair,
                         KeyValuePair<int, double> nextPair)
                    {
                        return nextPair.Value.CompareTo(firstPair.Value);
                    }
                );
        }

        public ActionResult Result()
        {
            List<List<List<KeyValuePair<int, double>>>> characteristics = TempData["characteristics"] as List<List<List<KeyValuePair<int, double>>>>;
            List<String> characteristicNames = TempData["characteristicNames"] as List<String>;
            int[] characteristicIds = TempData["characteristicIds"] as int[];
            List<SelectListItem> characteristicsList = new List<SelectListItem>();
            for (int i = 0; i < characteristicNames.Count; i++)
            {
                characteristicsList.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = characteristicNames[i],
                    Selected = false
                });
            }
            ViewBag.matterIds = TempData["matterIds"];
            ViewBag.characteristicIds = new List<int>(characteristicIds);
            ViewBag.characteristicsList = characteristicsList;
            ViewBag.characteristics = characteristics;
            ViewBag.chainNames = TempData["chainNames"];
            ViewBag.elementNames = TempData["elementNames"];
            ViewBag.characteristicNames = characteristicNames;
            ViewBag.theoreticalRanks = TempData["teoreticalRanks"];
            return View();
        }

    }
}