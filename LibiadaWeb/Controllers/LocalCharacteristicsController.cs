﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Web.Mvc;
using LibiadaCore.Classes.Misc.Iterators;
using LibiadaCore.Classes.Root;
using LibiadaCore.Classes.Root.Characteristics;
using LibiadaCore.Classes.Root.Characteristics.Calculators;
using LibiadaWeb.Models;

namespace LibiadaWeb.Controllers
{
    public class LocalCharacteristicsController : Controller
    {
        private readonly LibiadaWebEntities db = new LibiadaWebEntities();
        private readonly MatterRepository matterRepository;
        private readonly CharacteristicTypeRepository characteristicRepository;
        private readonly NotationRepository notationRepository;
        private readonly LinkUpRepository linkUpRepository;
        private readonly ChainRepository chainRepository;


        public LocalCharacteristicsController()
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
            ViewBag.characteristicsList = characteristicRepository.GetSelectListItems(null);
            ViewBag.mattersList = matterRepository.GetSelectListItems(null);
            ViewBag.notationsList = notationRepository.GetSelectListItems(null);
            ViewBag.linkUpsList = linkUpRepository.GetSelectListItems(null);

            return View();
        }

        [HttpPost]
        public ActionResult Index(long matterId, int[] characteristicIds, int[] linkUpIds, int notationId, int length,
                                  int step, bool isDelta, bool isSort, bool isFurie)
        {

            List<List<Double>> characteristicsTemp = new List<List<Double>>();
            String chainName = db.matter.Single(m => m.id == matterId).name;
            List<String> partNames = new List<string>();
            List<String> characteristicNames = new List<string>();



            for (int i = 0; i < characteristicIds.Length; i++)
            {
                characteristicsTemp.Add(new List<Double>());
                matter matter = db.matter.Single(m => m.id == matterId);
                chain chain = matter.chain.Single(c => c.building_type_id == 1 && c.notation_id == notationId);
                Chain libiadaChain = chainRepository.FromDbChainToLibiadaChain(chain);
                int characteristicId = characteristicIds[i];
                int linkUpId = linkUpIds[i];
                characteristicNames.Add(db.characteristic_type.Single(c => c.id == characteristicId).name + " " +
                                        db.link_up.Single(l => l.id == linkUpId).name + " " +
                                        db.notation.Single(n => n.id == notationId).name);

                String className = db.characteristic_type.Single(c => c.id == characteristicId).class_name;
                ICharacteristicCalculator calculator = CharacteristicsFactory.Create(className);
                LinkUp linkUp = (LinkUp)db.link_up.Single(l => l.id == linkUpId).id;

                IteratorStart<Chain, Chain> iter = new IteratorStart<Chain, Chain>(libiadaChain, length, step);
                while (iter.Next())
                {
                    Chain tempChain = iter.Current();
                    partNames.Add(tempChain.ToString());
                    characteristicsTemp.Last().Add(calculator.Calculate(tempChain, linkUp));
                }
            }

            if (isDelta)
            {
                //Перебираем характеристики
                for (int i = 0; i < characteristicsTemp.Count; i++)
                {
                    //перебираем фрагменты цепочек
                    for (int j = (characteristicsTemp[i].Count) - 1; j > 0; j--)
                    {
                        characteristicsTemp[i][j] -= characteristicsTemp[i][j - 1];
                    }
                    characteristicsTemp[i].RemoveAt(0);
                }
            }
            if (isSort)
            {
                //Перебираем характеристики
                for (int i = 0; i < characteristicsTemp.Count; i++)
                {
                    //перебираем фрагменты цепочек
                    characteristicsTemp[i].Sort();
                }
            }

            if (isFurie)
            {

                //переводим в комлексный вид
                for (int i = 0; i < characteristicsTemp.Count; i++)
                {
                    List<Complex> comp = new List<Complex>();
                    int j = 0;

                    for (j = 0; j < characteristicsTemp[i].Count; j++)
                    {
                        comp.Add(new Complex(characteristicsTemp[i][j], 0));
                    }

                    int k = 1;

                    while (k < j)
                    {
                        k *= 2;
                    }

                    for (; j < k; j++)
                    {
                        comp.Add(new Complex(0, 0));
                    }

                    Complex[] data = FFT.Fft(comp.ToArray()); //вернёт массив
                    List<double> temp = new List<double>();
                    //переводим в массив double

                    foreach (var fftElement in data)
                    {
                        temp.Add(fftElement.Real);
                    }

                    characteristicsTemp[i] = temp;
                }
            }


            List<List<Double>> characteristics = new List<List<Double>>();

            for (int t = 0; t < characteristicsTemp[0].Count; t++)
            {
                characteristics.Add(new List<double>());
                for (int w = 0; w < characteristicsTemp.Count; w++)
                {
                    characteristics[t].Add(characteristicsTemp[w][t]);
                }
            }



            TempData["characteristics"] = characteristics;
            TempData["chainName"] = chainName;
            TempData["partNames"] = partNames;
            TempData["characteristicNames"] = characteristicNames;
            TempData["characteristicIds"] = characteristicIds;
            TempData["chainIds"] = matterId;
            return RedirectToAction("Result");
        }

        public ActionResult Result()
        {
            List<List<double>> characteristics = TempData["characteristics"] as List<List<double>>;
            List<String> characteristicNames = TempData["characteristicNames"] as List<String>;
            ViewBag.chainIds = TempData["chainIds"] as List<long>;
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
            ViewBag.characteristicIds = new List<int>(characteristicIds);
            ViewBag.characteristicsList = characteristicsList;
            ViewBag.characteristics = characteristics;
            ViewBag.chainName = TempData["chainName"] as String;
            ViewBag.partNames = TempData["partNames"] as List<String>;
            ViewBag.characteristicNames = characteristicNames;
            return View();
        }
    }
}
