﻿namespace LibiadaWeb.Controllers.Catalogs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;

    /// <summary>
    /// The attribute check controller.
    /// </summary>
    public class AttributesCheckController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributesCheckController"/> class.
        /// </summary>
        public AttributesCheckController() : base("AttributesCheck", "Attributes check")
        {
            db = new LibiadaWebEntities();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var subsequencesSequenceIds = db.Subsequence.Select(g => g.SequenceId).Distinct();
            var matterIds = db.DnaSequence.Where(c => c.WebApiId != null && !subsequencesSequenceIds.Contains(c.Id) && c.FeatureId == Aliases.Feature.FullGenome).Select(c => c.MatterId).ToList();

            var calculatorsHelper = new ViewDataHelper(db);

            var data = calculatorsHelper.FillMattersData(1, int.MaxValue, true, m => matterIds.Contains(m.Id));

            data.Add("natureId", Aliases.Nature.Genetic);

            ViewBag.data = data;

            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matters ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if there is no file with sequence.
        /// </exception>
        /// <exception cref="Exception">
        /// Thrown if unknown part is found.
        /// </exception>
        [HttpPost]
        public ActionResult Index(long[] matterIds)
        {
            return Action(() =>
                {
                    var matterNames = new List<string>();
                    var attributes = new List<string>();

                    foreach (var matterId in matterIds)
                    {
                        long sequenceId = this.db.DnaSequence.Single(d => d.MatterId == matterId).Id;
                        DnaSequence parentSequence = this.db.DnaSequence.Single(c => c.Id == sequenceId);

                        Stream stream = NcbiHelper.GetGenesFileStream(parentSequence.WebApiId.ToString());
                        var features = NcbiHelper.GetFeatures(stream);

                        for (int j = 1; j < features.Count; j++)
                        {
                            var featureAttributes = features[j].Qualifiers;
                            attributes.AddRange(featureAttributes.Select(attribute => attribute.Key));
                        }

                        matterNames.Add(db.Matter.Single(m => m.Id == matterId).Name);
                    }

                    var databaseAttributes = db.Attribute.Select(a => a.Name).ToList();
                    attributes = attributes.Distinct().Where(a => !databaseAttributes.Contains(a)).ToList();

                    return new Dictionary<string, object>
                                     {
                                         { "attributes", attributes }, 
                                         { "matterNames", matterNames }
                                     };
                });
        }
    }
}