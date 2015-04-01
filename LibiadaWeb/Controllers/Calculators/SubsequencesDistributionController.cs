﻿namespace LibiadaWeb.Controllers.Calculators
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using LibiadaCore.Core;
    using LibiadaCore.Core.Characteristics;
    using LibiadaCore.Core.Characteristics.Calculators;

    using LibiadaWeb.Helpers;
    using LibiadaWeb.Models;
    using LibiadaWeb.Models.Calculators;
    using LibiadaWeb.Models.Repositories.Catalogs;
    using LibiadaWeb.Models.Repositories.Sequences;

    /// <summary>
    /// The subsequences distribution controller.
    /// </summary>
    public class SubsequencesDistributionController : AbstractResultController
    {
        /// <summary>
        /// The db.
        /// </summary>
        private readonly LibiadaWebEntities db;

        /// <summary>
        /// The sequence repository.
        /// </summary>
        private readonly CommonSequenceRepository commonSequenceRepository;

        /// <summary>
        /// The subsequence extracter.
        /// </summary>
        private readonly SubsequenceExtracter subsequenceExtracter;

        /// <summary>
        /// The characteristic type repository.
        /// </summary>
        private readonly CharacteristicTypeLinkRepository characteristicTypeLinkRepository;

        /// <summary>
        /// The sequence attribute repository.
        /// </summary>
        private readonly SequenceAttributeRepository sequenceAttributeRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubsequencesDistributionController"/> class.
        /// </summary>
        public SubsequencesDistributionController() : base("SubsequencesDistribution", "Subsequences distribution")
        {
            db = new LibiadaWebEntities();
            commonSequenceRepository = new CommonSequenceRepository(db);
            subsequenceExtracter = new SubsequenceExtracter(db);
            characteristicTypeLinkRepository = new CharacteristicTypeLinkRepository(db);
            sequenceAttributeRepository = new SequenceAttributeRepository(db);
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Index()
        {
            var calculatorsHelper = new ViewDataHelper(db);
            var data = calculatorsHelper.GetSubsequencesCalculationData(1, int.MaxValue, true);
            ViewBag.data = data;
            return View();
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="matterIds">
        /// The matter ids.
        /// </param>
        /// <param name="firstCharacteristicTypeLinkId">
        /// The first characteristic type and link id.
        /// </param>
        /// <param name="firstNotationId">
        /// The first notation id.
        /// </param>
        /// <param name="secondCharacteristicTypeLinkId">
        /// The second characteristic type and link id.
        /// </param>
        /// <param name="secondNotationId">
        /// The second notation id.
        /// </param>
        /// <param name="featureIds">
        /// The feature ids.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Index(
            long[] matterIds,
            int firstCharacteristicTypeLinkId,
            int firstNotationId,
            int secondCharacteristicTypeLinkId,
            int secondNotationId,
            int[] featureIds)
        {
            return Action(() =>
            {
                var result = new List<SequenceCharacteristics>();

                var sequenceIds = db.DnaSequence.Where(c => matterIds.Contains(c.MatterId) && c.NotationId == secondNotationId).Select(c => c.Id).ToList();

                double maxSubsequences = 0;

                for (int w = 0; w < matterIds.Length; w++)
                {
                    long matterId = matterIds[w];
                    var matterName = db.Matter.Single(m => m.Id == matterId).Name;

                    long sequenceId = db.CommonSequence.Single(c => c.MatterId == matterId && c.NotationId == firstNotationId).Id;

                    double sequenceCharacteristic;

                    if (db.Characteristic.Any(c => c.SequenceId == sequenceId && c.CharacteristicTypeLinkId == firstCharacteristicTypeLinkId))
                    {
                        sequenceCharacteristic = db.Characteristic.Single(c => c.SequenceId == sequenceId && c.CharacteristicTypeLinkId == firstCharacteristicTypeLinkId).Value;
                    }
                    else
                    {
                        Chain tempChain = commonSequenceRepository.ToLibiadaChain(sequenceId);
                        tempChain.FillIntervalManagers();
                        string fullClassName = characteristicTypeLinkRepository.GetCharacteristicType(firstCharacteristicTypeLinkId).ClassName;
                        IFullCalculator fullCalculator = CalculatorsFactory.CreateFullCalculator(fullClassName);
                        var fullLink = characteristicTypeLinkRepository.GetLibiadaLink(firstCharacteristicTypeLinkId);
                        sequenceCharacteristic = fullCalculator.Calculate(tempChain, fullLink);

                        var dataBaseCharacteristic = new Characteristic
                        {
                            SequenceId = sequenceId,
                            CharacteristicTypeLinkId = firstCharacteristicTypeLinkId,
                            Value = sequenceCharacteristic,
                            ValueString = sequenceCharacteristic.ToString()
                        };
                        db.Characteristic.Add(dataBaseCharacteristic);
                        db.SaveChanges();
                    }

                    List<Subsequence> subsequences = subsequenceExtracter.GetSubsequences(sequenceIds[w], featureIds);
                    var sequences = subsequenceExtracter.ExtractChains(subsequences, sequenceIds[w]);

                    if (maxSubsequences < subsequences.Count)
                    {
                        maxSubsequences = subsequences.Count;
                    }

                    var subsequencesCharacteristics = new List<SubsequenceCharacteristic>();
                    string className = characteristicTypeLinkRepository.GetCharacteristicType(secondCharacteristicTypeLinkId).ClassName;
                    IFullCalculator calculator = CalculatorsFactory.CreateFullCalculator(className);
                    var link = characteristicTypeLinkRepository.GetLibiadaLink(secondCharacteristicTypeLinkId);

                    for (int j = 0; j < sequences.Count; j++)
                    {
                        long subsequenceId = subsequences[j].Id;

                        if (!db.Characteristic.Any(c => c.SequenceId == subsequenceId && c.CharacteristicTypeLinkId == secondCharacteristicTypeLinkId))
                        {
                            double value = calculator.Calculate(sequences[j], link);
                            var currentCharacteristic = new Characteristic
                            {
                                SequenceId = subsequenceId,
                                CharacteristicTypeLinkId = secondCharacteristicTypeLinkId,
                                Value = value,
                                ValueString = value.ToString()
                            };

                            db.Characteristic.Add(currentCharacteristic);
                            db.SaveChanges();
                        }
                    }

                    for (int d = 0; d < sequences.Count; d++)
                    {
                        long subsequenceId = subsequences[d].Id;
                        double characteristic = db.Characteristic.Single(c => c.SequenceId == subsequenceId && c.CharacteristicTypeLinkId == secondCharacteristicTypeLinkId).Value;

                        var geneCharacteristic = new SubsequenceCharacteristic(subsequences[d], characteristic, sequenceAttributeRepository.GetAttributes(subsequences[d].Id));
                        subsequencesCharacteristics.Add(geneCharacteristic);
                    }

                    subsequencesCharacteristics = subsequencesCharacteristics.OrderBy(g => g.Characteristic).ToList();

                    result.Add(new SequenceCharacteristics(matterName, sequenceCharacteristic, subsequencesCharacteristics));
                }

                result = result.OrderBy(r => r.Characteristic).ToList();

                var fullCharacteristicName = characteristicTypeLinkRepository.GetCharacteristicName(firstCharacteristicTypeLinkId, firstNotationId);
                var subsequencesCharacteristicName = characteristicTypeLinkRepository.GetCharacteristicName(secondCharacteristicTypeLinkId, secondNotationId);

                return new Dictionary<string, object>
                {
                    { "result", result },
                    { "maxSubsequences", maxSubsequences },
                    { "subsequencesCharacteristicName", subsequencesCharacteristicName },
                    { "fullCharacteristicName", fullCharacteristicName }
                };
            });
        }
    }
}