﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using LibiadaWeb.Helpers;
using LibiadaWeb.Models;
using LibiadaWeb.Models.Repositories;
using LibiadaWeb.Models.Repositories.Chains;

namespace LibiadaWeb.Controllers.Chains
{
    public class GenesImportController : Controller
    {
        private readonly ElementRepository elementRepository;
        private readonly DnaChainRepository dnaChainRepository;
        private readonly LibiadaWebEntities db;

        public GenesImportController()
        {
            db = new LibiadaWebEntities();
            elementRepository = new ElementRepository(db);
            dnaChainRepository = new DnaChainRepository(db);
        }

        //
        // GET: /ChainCheck/

        public ActionResult Index()
        {
            ViewBag.data = new Dictionary<string, object>
                {
                    {"chains", db.dna_chain.Where(c => c.web_api_id != null).Select(c => new
                        {
                            Value = c.id,
                            Text = c.matter.name,
                            Selected = false
                        })}
                };
            return View();
        }

        [HttpPost]
        public ActionResult Index(long chainId, bool localFile)
        { 
            
            dna_chain parentChain = db.dna_chain.Single(c => c.id == chainId);
            Stream stream;
            if (localFile)
            {
                HttpPostedFileBase file = Request.Files[0];

                if (file == null || file.ContentLength == 0)
                {
                    throw new ArgumentNullException("Файл цепочки не задан или пуст");
                }
                stream = file.InputStream;
            }
            else
            {
                stream = NcbiHelper.GetGenes(parentChain.web_api_id.ToString());
            }
            byte[] input = new byte[stream.Length];

            // Read the file into the byte array
            stream.Read(input, 0, (int)stream.Length);

            string data = Encoding.ASCII.GetString(input);

            data = data.Split(new[] { "ORIGIN" }, StringSplitOptions.RemoveEmptyEntries)[0];
            string[] temp = data.Split(new[] { "FEATURES" }, StringSplitOptions.RemoveEmptyEntries);
            string information = temp[0];
            string[] genes = temp[1].Split(new[] { "gene            ", "repeat_region   " }, StringSplitOptions.RemoveEmptyEntries);
            var coordinates = new List<int[]>();
            for (int i = 1; i < genes.Length; i++)
            {
                var dnaChain = new dna_chain
                    {
                        matter_id = parentChain.matter_id,
                        notation_id = Aliases.NotationNucleotide,
                        dissimilar = false,
                        remote_db_id = Aliases.RemoteDbNcbi,
                        partial = false
                    };

                String[] temp2 = genes[i].Split(new[] {'\n', '\r'});
                bool complement = temp2[0].StartsWith("complement");
                string temp3 = complement
                                   ? temp2[0].Split(new[] {"complement"}, StringSplitOptions.RemoveEmptyEntries)[0]
                                   : temp2[0];
                string start = temp3.Split(new[] {"..", "(", ")"}, StringSplitOptions.RemoveEmptyEntries)[0];
                String stop = temp3.Split(new[] {"..", "(", ")"}, StringSplitOptions.RemoveEmptyEntries)[1];
                dnaChain.piece_position = Convert.ToInt32(start);
                dnaChain.complement = complement;
                coordinates.Add(new[]
                    {
                        Convert.ToInt32(start),
                        Convert.ToInt32(stop)
                    });
                string sequenceType = String.Empty;
                for (int j = 1; j < temp2.Length; j++)
                {
                    if (temp2[j].Contains(start + ".." + stop))
                    {
                        sequenceType = temp2[j].Trim();
                        break;
                    }
                }
                if (sequenceType.StartsWith("CDS"))
                {
                    dnaChain.remote_id = GetValue(temp2, "/protein_id=\"");
                    dnaChain.web_api_id = Convert.ToInt32(GetValue(temp2, "/db_xref=\"GI:"));
                    dnaChain.description = GetValue(temp2, "/product=\"");
                }
                else if (sequenceType.StartsWith("tRNA"))
                {
                    dnaChain.description = GetValue(temp2, "/product=\"");
                }
                else if (sequenceType.StartsWith("rRNA"))
                {
                    dnaChain.description = GetValue(temp2, "/product=\"");
                }
                else if (sequenceType.StartsWith("/rpt_type=tandem"))
                {

                }

            }

            /*
            string[] chains = data.Split('>');

            for (int i = 0; i < chains.Length; i++)
            {
                string[] splittedFasta = chains[i].Split(new[] { '\n', '\r' });
                var chainStringBuilder = new StringBuilder();
                String fastaHeader = splittedFasta[0];
                for (int j = 1; j < splittedFasta.Length; j++)
                {
                    chainStringBuilder.Append(splittedFasta[j]);
                }

                string resultStringChain = DataTransformers.CleanFastaFile(chainStringBuilder.ToString());

                var libiadaChain = new BaseChain(resultStringChain);
                if (!elementRepository.ElementsInDb(libiadaChain.Alphabet, parentChain.notation_id))
                {
                    throw new Exception("В БД отсутствует как минимум один элемент алфавита, добавляемой цепочки");
                }
                var resultChain = new chain
                    {
                        notation_id = parentChain.notation_id,
                        created = DateTime.Now,
                        matter_id = parentChain.matter_id,
                        dissimilar = false,
                        piece_type_id = !!!!!!!,
                        piece_position = !!!!!!!,
                        remote_id = !!!!!!!!,
                        remote_db_id = parentChain.remote_db_id
                    };

                long[] alphabet = elementRepository.ToDbElements(libiadaChain.Alphabet, parentChain.notation_id, false);
                dnaChainRepository.Insert(resultChain, fastaHeader, Convert.ToInt32(webApiId), alphabet, libiadaChain.Building);
            
        }*/

            return RedirectToAction("Result");
        }

        private static string GetValue(string[] strings, string pattern)
        {
            for (int i = 1; i < strings.Length; i++)
            {
                if (strings[i].Contains(pattern))
                {
                    return strings[i].Substring(strings[i].IndexOf(pattern) + pattern.Length, strings[i].Length - strings[i].IndexOf(pattern) - pattern.Length - 1);
                }
            }
            return String.Empty;
        }

        public ActionResult Result()
        {
            return View();
        }
    }
}
