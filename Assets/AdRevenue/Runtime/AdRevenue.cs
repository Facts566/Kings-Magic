using System.Collections.Generic;
using Io.AppMetrica.AdRevenueAdapter.Internal;
using JetBrains.Annotations;

namespace Io.AppMetrica.AdRevenueAdapter {
    internal class AdRevenue {
        /// <summary>
        /// Amount of money received via ad string.
        /// It cannot be negative.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        public string AdRevenueValue { get; }

        /// <summary>
        /// Currency in which money from <see cref="AdRevenueValue"/> is represented.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        [NotNull]
        public string Currency { get; }

        /// <summary>
        /// Ad network.
        /// Maximum length is 100 symbols.
        /// If the value exceeds this limit it will be truncated by AppMetrica.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        [CanBeNull]
        public string AdNetwork { get; set; }

        /// <summary>
        /// Id of ad placement.
        /// Maximum length is 100 symbols.
        /// If the value exceeds this limit it will be truncated by AppMetrica.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        [CanBeNull]
        public string AdPlacementId { get; set; }

        /// <summary>
        /// Name of ad placement.
        /// Maximum length is 100 symbols.
        /// If the value exceeds this limit it will be truncated by AppMetrica.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        [CanBeNull]
        public string AdPlacementName { get; set; }

        /// <summary>
        /// Ad type. See possible values in <see cref="AdRevenueAdapter.AdType"/>.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        [CanBeNull]
        public AdType? AdType { get; set; }

        /// <summary>
        /// Id of ad unit.
        /// Maximum length is 100 symbols.
        /// If the value exceeds this limit it will be truncated by AppMetrica.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        [CanBeNull]
        public string AdUnitId { get; set; }

        /// <summary>
        /// Name of ad unit.
        /// Maximum length is 100 symbols.
        /// If the value exceeds this limit it will be truncated by AppMetrica.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        [CanBeNull]
        public string AdUnitName { get; set; }

        /// <summary>
        /// Arbitrary payload: additional info represented as key-value pairs.
        /// Maximum size is 30 KB.
        /// If the value exceeds this limit it will be truncated by AppMetrica.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        [CanBeNull]
        public IDictionary<string, string> Payload { get; set; }

        /// <summary>
        /// Precision.
        /// Example: "publisher_defined", "estimated".
        /// Maximum length is 100 symbols.
        /// If the value exceeds this limit it will be truncated by AppMetrica.
        ///
        /// <p><b>Platforms</b>: Android, iOS.</p>
        /// </summary>
        [CanBeNull]
        public string Precision { get; set; }

        /// <summary>
        /// Creates a AdRevenue.
        /// </summary>
        /// <param name="adRevenue">Amount of money received via ad revenue.</param>
        /// <param name="currency">Currency.</param>
        public AdRevenue(decimal adRevenue, [NotNull] string currency) {
            AdRevenueValue = NumberUtils.SerializeDecimal(adRevenue);
            Currency = currency;
        }

        /// <summary>
        /// Creates a AdRevenue.
        /// </summary>
        /// <param name="adRevenueMicros">Amount of money received via ad revenue represented
        ///                               as micros (actual value multiplied by 10^6).</param>
        /// <param name="currency">Currency.</param>
        public AdRevenue(long adRevenueMicros, [NotNull] string currency) {
            AdRevenueValue = NumberUtils.SerializeMicros(adRevenueMicros);
            Currency = currency;
        }

        /// <summary>
        /// Creates a AdRevenue.
        /// </summary>
        /// <param name="adRevenue">Amount of money received via ad revenue represented as double.</param>
        /// <param name="currency">Currency.</param>
        public AdRevenue(double adRevenue, [NotNull] string currency) {
            AdRevenueValue = NumberUtils.SerializeDouble(adRevenue);
            Currency = currency;
        }
    }

    /// <summary>
    /// Enum containing possible Ad Type values.
    /// </summary>
    /// <seealso cref="AdRevenue.AdType"/>
    internal enum AdType {
        /// <summary>
        /// App Open Ad Type.
        /// </summary>
        AppOpen,

        /// <summary>
        /// Banner Ad Type.
        /// </summary>
        Banner,

        /// <summary>
        /// Interstitial Ad Type.
        /// </summary>
        Interstitial,

        /// <summary>
        /// Mrec Ad Type.
        /// </summary>
        Mrec,

        /// <summary>
        /// Native Ad Type.
        /// </summary>
        Native,

        /// <summary>
        /// Rewarded Ad Type.
        /// </summary>
        Rewarded,

        /// <summary>
        /// Other Ad Type.
        /// </summary>
        Other,
    }
}
